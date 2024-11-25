using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RPA.Entities;

namespace RPA.Context;

public partial class U0987408AcdmyContext : DbContext
{
    public U0987408AcdmyContext()
    {
    }

    public U0987408AcdmyContext(DbContextOptions<U0987408AcdmyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Instance> Instances { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=94.73.146.3;Initial Catalog=u0987408_Acdmy;Persist Security Info=True;User ID=u0987408_user23E;Password=7VPK-0M-0_zw2g=d;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instance>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Instance");

            entity.Property(e => e.IdProcess)
                .HasMaxLength(100)
                .HasColumnName("ID_Process");
            entity.Property(e => e.IdProcessInstance)
                .HasMaxLength(100)
                .HasColumnName("ID_Process_Instance");
            entity.Property(e => e.Software1).HasColumnName("Software-1");
            entity.Property(e => e.Software2).HasColumnName("Software-2");
            entity.Property(e => e.Software3).HasColumnName("Software-3");
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Process");

            entity.Property(e => e.IdProcess)
                .HasMaxLength(100)
                .HasColumnName("ID_Process");
            entity.Property(e => e.RequiredSoftware1).HasColumnName("RequiredSoftware_1");
            entity.Property(e => e.RequiredSoftware2).HasColumnName("RequiredSoftware_2");
            entity.Property(e => e.RequiredSoftware3).HasColumnName("RequiredSoftware_3");
            entity.Property(e => e.TransactionClock1)
                .HasMaxLength(100)
                .HasColumnName("TransactionClock_1");
            entity.Property(e => e.TransactionClock2)
                .HasMaxLength(100)
                .HasColumnName("TransactionClock_2");
            entity.Property(e => e.TransactionClock3)
                .HasMaxLength(100)
                .HasColumnName("TransactionClock_3");
            entity.Property(e => e.TransactionClock4)
                .HasMaxLength(100)
                .HasColumnName("TransactionClock_4");
            entity.Property(e => e.TransactionClock5)
                .HasMaxLength(100)
                .HasColumnName("TransactionClock_5");
            entity.Property(e => e.TransactionDay1).HasColumnName("TransactionDay_1");
            entity.Property(e => e.TransactionDay2).HasColumnName("TransactionDay_2");
            entity.Property(e => e.TransactionDay3).HasColumnName("TransactionDay_3");
            entity.Property(e => e.TransactionDay4).HasColumnName("TransactionDay_4");
            entity.Property(e => e.TransactionDay5).HasColumnName("TransactionDay_5");
            entity.Property(e => e.TransactionDay6).HasColumnName("TransactionDay_6");
            entity.Property(e => e.TransactionDay7).HasColumnName("TransactionDay_7");
            entity.Property(e => e.TransactionTime1).HasColumnName("TransactionTime_1");
            entity.Property(e => e.TransactionTime2).HasColumnName("TransactionTime_2");
            entity.Property(e => e.TransactionTime3).HasColumnName("TransactionTime_3");
            entity.Property(e => e.TransactionTime4).HasColumnName("TransactionTime_4");
            entity.Property(e => e.TransactionTime5).HasColumnName("TransactionTime_5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
