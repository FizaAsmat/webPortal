using Microsoft.EntityFrameworkCore;

namespace WebPortalAPI.Models
{
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
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        // NOTE: Move your connection string to appsettings.json in production.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code.
                optionsBuilder.UseSqlServer("Server=DESKTOP-3TL1BCC\\SQLEXPRESS;Database=PMFDatabase;User Id=sa;Password=abcd.1234;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applicant>(entity =>
            {
                entity.HasKey(e => e.ApplicantId).HasName("PK__Applicant");

                entity.Property(e => e.Cnic)
                      .HasMaxLength(15)
                      .HasColumnName("CNIC");

                entity.Property(e => e.FullName)
                      .HasMaxLength(255);

                entity.Property(e => e.MobileNo)
                      .HasMaxLength(15);
            });

            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId).HasName("PK__BankTransaction");

                entity.Property(e => e.BranchCode)
                      .HasMaxLength(50);

                entity.Property(e => e.BranchName)
                      .HasMaxLength(100);

                entity.Property(e => e.ChallanAmount)
                      .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.FeeTitle)
                      .HasMaxLength(255);

                entity.HasOne(d => d.ChallanNoNavigation)
                      .WithMany(p => p.BankTransactions)
                      .HasForeignKey(d => d.ChallanNo)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_BankTransaction_Challan");
            });

            modelBuilder.Entity<Challan>(entity =>
            {
                entity.HasKey(e => e.ChallanNo).HasName("PK__Challan");

                entity.Property(e => e.FeeAmount)
                      .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.GeneratedDate)
                      .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsExpired)
                      .HasDefaultValue(false);

                entity.Property(e => e.IsPaid)
                      .HasDefaultValue(false);

                entity.HasOne(d => d.Applicant)
                      .WithMany(p => p.Challans)
                      .HasForeignKey(d => d.ApplicantId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Challan_Applicant");

                entity.HasOne(d => d.FeeTitle)
                      .WithMany(p => p.Challans)
                      .HasForeignKey(d => d.FeeTitleId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Challan_FeeTitle");
            });

            modelBuilder.Entity<FeeTitle>(entity =>
            {
                entity.HasKey(e => e.FeeTitleId).HasName("PK__FeeTitle");

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Title)
                      .HasMaxLength(255);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Users");

                entity.Property(e => e.Username)
                      .HasMaxLength(100);

                entity.Property(e => e.Password)
                      .HasMaxLength(255);

                entity.Property(e => e.Role)
                      .HasMaxLength(50);

                entity.Property(e => e.IsApproved)
                      .HasDefaultValue(false);

                entity.Property(e => e.BankName)
                      .HasMaxLength(100);

                entity.Property(e => e.BranchCode)
                      .HasMaxLength(50);

                entity.Property(e => e.ContactPerson)
                      .HasMaxLength(100);

                entity.Property(e => e.ContactNumber)
                      .HasMaxLength(20);

                entity.Property(e => e.Email)
                      .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ApprovedAt);

                entity.Property(e => e.RejectionReason)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
