using System;
using System.Collections.Generic;
using CinemaBooking.Bookings.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Bookings.Infrastructure.Persistence;

public partial class BookingsDbContext : DbContext
{
    public BookingsDbContext(DbContextOptions<BookingsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookedSeat> BookedSeats { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookedSeat>(entity =>
        {
            entity.ToTable("BookedSeat");

            entity.HasIndex(e => e.BookingId, "IX_BookedSeat_Booking");

            entity.HasIndex(e => new { e.ShowId, e.SeatId }, "UX_BookedSeat_Show_Seat")
                .IsUnique()
                .HasFilter("([CancelledAt] IS NULL)");

            entity.Property(e => e.CancelledAt).HasPrecision(0);

            entity.HasOne(d => d.Booking).WithMany(p => p.BookedSeats)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookedSeat_Booking");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");

            entity.Property(e => e.CancelledAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Booking_CreatedAt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
