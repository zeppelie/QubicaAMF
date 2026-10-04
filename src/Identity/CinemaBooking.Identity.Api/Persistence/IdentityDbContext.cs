using System;
using System.Collections.Generic;
using CinemaBooking.Identity.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Identity.Api.Persistence;

public partial class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserAccount");

            entity.HasIndex(e => e.UserName, "UQ_UserAccount_UserName").IsUnique();

            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Customer", "DF_UserAccount_Role");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
