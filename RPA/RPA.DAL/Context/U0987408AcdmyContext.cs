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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=94.73.146.3;Initial Catalog=u0987408_Acdmy;Persist Security Info=True;User ID=u0987408_user23E;Password=7VPK-0M-0_zw2g=d;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True");

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
