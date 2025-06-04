using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebPortalAPI.Models;

public partial class PmfdatabaseContext : DbContext
{
    public PmfdatabaseContext()
    {
    }

    public PmfdatabaseContext(DbContextOptions<PmfdatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Applicant> Applicants { get; set; }

    public virtual DbSet<BankTransaction> BankTransactions { get; set; }

    public virtual DbSet<Challan> Challans { get; set; }

    public virtual DbSet<FeeTitle> FeeTitles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-3TL1BCC\\SQLEXPRESS;Database=PMFDatabase;User Id=sa;Password=abcd.1234;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.HasKey(e => e.ApplicantId).HasName("PK__Applican__39AE91A832F6383E");

            entity.Property(e => e.Cnic)
                .HasMaxLength(15)
                .HasColumnName("CNIC");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.MobileNo).HasMaxLength(15);
        });

        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__BankTran__55433A6B9A112D92");

            entity.Property(e => e.BranchCode).HasMaxLength(50);
            entity.Property(e => e.BranchName).HasMaxLength(100);
            entity.Property(e => e.ChallanAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FeeTitle).HasMaxLength(255);

            entity.HasOne(d => d.ChallanNoNavigation).WithMany(p => p.BankTransactions)
                .HasForeignKey(d => d.ChallanNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BankTrans__Chall__46E78A0C");
        });

        modelBuilder.Entity<Challan>(entity =>
        {
            entity.HasKey(e => e.ChallanNo).HasName("PK__Challans__BFB7BB8B00BDB549");

            entity.Property(e => e.FeeAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.GeneratedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsExpired).HasDefaultValue(false);
            entity.Property(e => e.IsPaid).HasDefaultValue(false);

            entity.HasOne(d => d.Applicant).WithMany(p => p.Challans)
                .HasForeignKey(d => d.ApplicantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Challans__Applic__403A8C7D");

            entity.HasOne(d => d.FeeTitle).WithMany(p => p.Challans)
                .HasForeignKey(d => d.FeeTitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Challans__FeeTit__412EB0B6");
        });

        modelBuilder.Entity<FeeTitle>(entity =>
        {
            entity.HasKey(e => e.FeeTitleId).HasName("PK__FeeTitle__9C0A7F7782F00C1E");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C6670D570");

            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
