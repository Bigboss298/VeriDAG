using Microsoft.EntityFrameworkCore;

namespace DataTrustNexus.Api.Models;

/// <summary>
/// Database context for DataTrust Nexus
/// </summary>
public class DataTrustDbContext : DbContext
{
    public DataTrustDbContext(DbContextOptions<DataTrustDbContext> options)
        : base(options)
    {
    }

    public DbSet<Institution> Institutions { get; set; }
    public DbSet<DataRecord> DataRecords { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Institution configuration
        modelBuilder.Entity<Institution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WalletAddress).IsUnique();
            entity.HasIndex(e => e.RegistrationNumber);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.InstitutionType).IsRequired();
            entity.Property(e => e.WalletAddress).IsRequired();
        });

        // DataRecord configuration
        modelBuilder.Entity<DataRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecordId).IsUnique();
            entity.HasIndex(e => e.DataHash);
            entity.HasIndex(e => e.IpfsHash);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.UploadedAt);
            
            entity.HasOne(e => e.Owner)
                .WithMany(i => i.DataRecords)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AccessRequest configuration
        modelBuilder.Entity<AccessRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RecordId, e.GranteeWalletAddress });
            entity.HasIndex(e => e.GranteeWalletAddress);
            entity.HasIndex(e => e.GrantedAt);
            entity.HasIndex(e => e.ExpiresAt);
            
            entity.HasOne(e => e.DataRecord)
                .WithMany(d => d.AccessRequests)
                .HasForeignKey(e => e.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Granter)
                .WithMany(i => i.GrantedAccess)
                .HasForeignKey(e => e.GranterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ActionType);
            entity.HasIndex(e => e.ActorId);
            entity.HasIndex(e => e.RecordId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.ActionType, e.Timestamp });
            
            entity.HasOne(e => e.Actor)
                .WithMany(i => i.AuditLogs)
                .HasForeignKey(e => e.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

