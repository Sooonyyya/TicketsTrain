using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using Route = TicketsTrainDomain.Model.Route;

namespace TicketsTrainInfrastructure;

public partial class TicketsTrainContext : DbContext
{
    public TicketsTrainContext()
    {
    }

    public TicketsTrainContext(DbContextOptions<TicketsTrainContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RailwayStation> RailwayStations { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<StationAtRoute> StationAtRoutes { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    public virtual DbSet<TicketTypeTrain> TicketTypeTrains { get; set; }

    public virtual DbSet<Train> Trains { get; set; }

    public virtual DbSet<TrainAtRoute> TrainAtRoutes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=SofiaHP\\SQLEXPRESS; Database=TicketsTrain; Trusted_Connection=True; TrustServerCertificate=True; ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RailwayStation>(entity =>
        {
            entity.ToTable("RailwayStation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CityTown)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("Route");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndStation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartStation)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StationAtRoute>(entity =>
        {
            entity.ToTable("StationAtRoute");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RailwayStationId).HasColumnName("RailwayStation_id");
            entity.Property(e => e.RouteId).HasColumnName("Route_id");

            entity.HasOne(d => d.RailwayStation).WithMany(p => p.StationAtRoutes)
                .HasForeignKey(d => d.RailwayStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StationAtRoute_RailwayStation");

            entity.HasOne(d => d.Route).WithMany(p => p.StationAtRoutes)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StationAtRoute_Route");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArrivalStationId).HasColumnName("ArrivalStation_id");
            entity.Property(e => e.DispatchStationId).HasColumnName("DispatchStation_id");
            entity.Property(e => e.TicketTypeTrainId).HasColumnName("TicketType_Train_id");
            entity.Property(e => e.TrainId).HasColumnName("Train_id");
            entity.Property(e => e.UserId).HasColumnName("User_id");

            entity.HasOne(d => d.ArrivalStation).WithMany(p => p.TicketArrivalStations)
                .HasForeignKey(d => d.ArrivalStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_RailwayStation");

            entity.HasOne(d => d.DispatchStation).WithMany(p => p.TicketDispatchStations)
                .HasForeignKey(d => d.DispatchStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_RailwayStation1");

            entity.HasOne(d => d.TicketTypeTrain).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicketTypeTrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_TicketType_Train");

            entity.HasOne(d => d.Train).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Train");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_User");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.ToTable("TicketType");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TicketTypeTrain>(entity =>
        {
            entity.ToTable("TicketType_Train");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketType_id");
            entity.Property(e => e.TrainId).HasColumnName("Train_id");

            entity.HasOne(d => d.TicketType).WithMany(p => p.TicketTypeTrains)
                .HasForeignKey(d => d.TicketTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketType_Train_TicketType");

            entity.HasOne(d => d.Train).WithMany(p => p.TicketTypeTrains)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketType_Train_Train");
        });

        modelBuilder.Entity<Train>(entity =>
        {
            entity.ToTable("Train");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DestinationId).HasColumnName("Destination_id");
            entity.Property(e => e.TrainName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TrainAtRoute>(entity =>
        {
            entity.ToTable("TrainAtRoute");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RouteId).HasColumnName("Route_id");
            entity.Property(e => e.TrainId).HasColumnName("Train_id");

            entity.HasOne(d => d.Route).WithMany(p => p.TrainAtRoutes)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainAtRoute_Route");

            entity.HasOne(d => d.Train).WithMany(p => p.TrainAtRoutes)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainAtRoute_Train");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
