using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyFinances.Api.Mapping;
using MyFinances.Api.Middleware;
using MyFinances.App.Abstractions;
using MyFinances.App.Queries.CategoryReport;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Queries.Summary;
using MyFinances.App.Services.Admin;
using MyFinances.App.Services.PasswordReset;
using MyFinances.App.Configuration;
using MyFinances.Infrastructure.Data;
using MyFinances.Infrastructure.Email;
using MyFinances.Infrastructure.Repositories;
using MyFinances.Infrastructure.Security;
using MyFinances.Infrastructure.Storage;
using MyFinances.Infrastructure.Validators;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// STARTUP GUARDS
// ============================================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT signing key (Jwt:Key) is not configured. Set it via user-secrets or an environment variable.");

// ============================================
// CONFIGURATION
// ============================================
builder.Services.Configure<SwaggerSettings>(builder.Configuration.GetSection("Swagger"));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Google"));

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
    // Pol�tica para Desenvolvimento (permissiva)
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Permite cookies e autentica��o
              .WithExposedHeaders("Content-Disposition"); // Para download de arquivos
    });

    // Pol�tica para Produ��o (restritiva)
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:ProductionOrigin"] ?? "https://seu-frontend.com")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type", "Accept")
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains(); // Permite subdom�nios
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

    });

builder.Services.AddAuthorization();

// ============================================
// CONTROLLERS & JSON
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddAutoMapper(typeof(Program).Assembly);

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
    var swaggerSettings = builder.Configuration.GetSection("Swagger").Get<SwaggerSettings>() 
        ?? new SwaggerSettings();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = swaggerSettings.Title,
        Version = swaggerSettings.Version,
        Description = swaggerSettings.Description,
        Contact = new OpenApiContact
        {
            Name = swaggerSettings.ContactName,
            Email = swaggerSettings.ContactEmail
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
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileValidator, FileValidator>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISummaryQuery, SummaryQuery>();
builder.Services.AddScoped<ICategoryReport, CategoryReport>();
builder.Services.AddScoped<ICategoryReportRepository, CategoryReportRepository>();
builder.Services.AddScoped<ISummaryRepository, SummaryRepository>();
builder.Services.AddHttpClient<IFileStorageService, SupabaseStorageService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var supabaseUrl = configuration["Supabase:Url"];
    var apiKey = configuration["Supabase:ApiKey"];

    if (string.IsNullOrEmpty(apiKey))
        throw new InvalidOperationException("Supabase service key (Supabase:ApiKey) is not configured. Set it via user-secrets or an environment variable.");

    client.BaseAddress = new Uri(supabaseUrl ?? throw new InvalidOperationException("Supabase URL is not configured."));

    client.DefaultRequestHeaders.Add("apikey", apiKey);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My Finances API v1");
        options.RoutePrefix = "swagger";
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
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
