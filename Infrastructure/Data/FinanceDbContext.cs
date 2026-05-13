namespace MyFinances.Infrastructure.Data
{
    public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
        {
        public DbSet<User> Users => Set<User>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Email).IsRequired();
                entity.Property(x => x.FullName).IsRequired();
                
                entity.HasIndex(x => x.Email)
                    .IsUnique();
                entity.HasIndex(x => x.GoogleSubjectId)
                    .IsUnique()
                    .HasFilter("\"GoogleSubjectId\" IS NOT NULL");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Amount).IsRequired();
                entity.Property(x => x.Type).IsRequired();
                entity.Property(x => x.Description).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.HasOne(t => t.Account)
                      .WithMany()
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Category)
                      .WithMany()
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => new { t.UserId, t.CreatedAt });
                entity.HasIndex(t => new { t.UserId, t.CategoryId });
                entity.HasIndex(t => new { t.UserId, t.AccountId });
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.Property(x => x.Type).IsRequired();

                entity.HasIndex(x => x.UserId);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.Property(x => x.Balance).IsRequired();
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(x => x.IsActive).HasDefaultValue(true);

                entity.HasIndex(x => x.UserId);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TokenHash).IsRequired();
                entity.Property(x => x.ExpiresAt).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.UserId, t.Used, t.ExpiresAt });
            });
        }
    }
}
