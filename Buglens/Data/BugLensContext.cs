using Buglens.Model;
using Microsoft.EntityFrameworkCore;

namespace BugLens.Data
{
    public class BugLensContext : DbContext
    {
        public BugLensContext(DbContextOptions<BugLensContext> options) 
            : base(options)
        {
        }

        public DbSet<Analysis> Analyses { get; set; }
        public DbSet<User> Users { get; set; }
        
        public DbSet<OverviewStats> OverviewStats { get; set;}
        public DbSet<AnalysisStatistic> AnalysisStatistics { get; set;}
 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.ToTable("analyses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime(6)")
                    .ValueGeneratedNever();
                entity.HasIndex(e => e.CreatedAt);
            });
            
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });
            
            modelBuilder.Entity<AnalysisStatistic>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ErrorType)
                    .HasMaxLength(200);

                entity.Property(e => e.ResponseTimeSeconds)
                    .HasPrecision(10, 2);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

            
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            });
            
            modelBuilder.Ignore<UserStatisticsDto>();
            modelBuilder.Ignore<OverviewStats>();
            
            modelBuilder.Ignore<LanguageUsageDto>();
            modelBuilder.Ignore<TimelineDataDto>();
            modelBuilder.Ignore<ErrorTypeDto>();
            modelBuilder.Ignore<QuickStatsDto>();
        }
    }
}