using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyFinances.Api.Mapping;
using MyFinances.Api.Middleware;
using MyFinances.App.Abstractions;
using MyFinances.App.Queries.CategoryReport;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Queries.Summary;
using MyFinances.App.Services;
using MyFinances.App.Services.Interfaces;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories;
using MyFinances.Infrasctructure.Repositories.Interfaces;
using MyFinances.Infrasctructure.Security;
using MyFinances.Infrasctructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// LOGGING
// ============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("My Finances", LogLevel.Information);

// ============================================
// CORS CONFIGURATION
// ============================================
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:4200"];

builder.Services.AddCors(options =>
{
    // Política para Desenvolvimento (permissiva)
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Permite cookies e autenticação
              .WithExposedHeaders("Content-Disposition"); // Para download de arquivos
    });

    // Política para Produção (restritiva)
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:ProductionOrigin"] ?? "https://seu-frontend.com")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type", "Accept")
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains(); // Permite subdomínios
    });

    // Política ABERTA (APENAS PARA TESTES - NÃO USE EM PRODUÇÃO!)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================
// AUTHENTICATION & AUTHORIZATION
// ============================================
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };

        // Configuração para permitir JWT via query string (útil para SignalR/WebSockets)
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ============================================
// CONTROLLERS & JSON
// ============================================
builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<AutoMapperProfile>();
});

// ============================================
// DATABASE
// ============================================
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ============================================
// SWAGGER/OPENAPI
// ============================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Finances",
        Version = "v1",
        Description = "API for managing my finances",
        Contact = new OpenApiContact
        {
            Name = "My Finances Team",
            Email = "contact@myfinances.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// SERVICES
// ============================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileStorageService, SupabaseStorageService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISummaryQuery, SummaryQuery>();
builder.Services.AddScoped<ICategoryReport, CategoryReport>();
builder.Services.AddHttpClient<IFileStorageService, SupabaseStorageService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var supabaseUrl = configuration["Supabase:Url"];
    var serviceKey = configuration["Supabase:ServiceKey"];

    client.BaseAddress = new Uri(supabaseUrl ?? throw new InvalidOperationException("Supabase URL is not configured."));

    client.DefaultRequestHeaders.Add("apikey", serviceKey);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", serviceKey);
});

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE (ORDEM IMPORTA!)
// ============================================

// 1. Exception Handler (SEMPRE PRIMEIRO)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 2. CORS (ANTES de Authentication/Authorization)
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
}

// 3. Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API v1");
        options.RoutePrefix = "swagger";
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ============================================
// STARTUP LOG
// ============================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("My Finances API started successfully at {Time}", DateTime.UtcNow);
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("CORS Policy: {Policy}", app.Environment.IsDevelopment() ? "DevelopmentPolicy" : "ProductionPolicy");


app.Run();
