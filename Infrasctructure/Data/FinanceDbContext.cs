using System;
using Microsoft.EntityFrameworkCore;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Data
{
    public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
        {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Email).IsRequired();
                entity.Property(x => x.FullName).IsRequired();
            });
        }
    }
}
