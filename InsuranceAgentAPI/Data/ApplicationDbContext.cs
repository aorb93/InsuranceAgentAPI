using InsuranceAgentAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgentAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentRefreshToken> AgentRefreshTokens => Set<AgentRefreshToken>();
    public DbSet<AgentLoginLog> AgentLoginLogs => Set<AgentLoginLog>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // Configuración: Agent
        // ==========================================
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");

            entity.HasKey(e => e.AgentId);

            entity.Property(e => e.AgentGuid)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Índices y restricciones únicas
            entity.HasIndex(e => e.AgentGuid).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

        // ==========================================
        // Configuración: AgentRefreshToken
        // ==========================================
        modelBuilder.Entity<AgentRefreshToken>(entity =>
        {
            entity.ToTable("AgentRefreshTokens");

            entity.HasKey(e => e.RefreshTokenId);

            entity.Property(e => e.Token)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.Property(e => e.RevokedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.Property(e => e.ReplacedByToken)
                .HasMaxLength(256);

            entity.Property(e => e.ReasonRevoked)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Token).IsUnique();

            // Relación con Agent (Cascade Delete)
            entity.HasOne(d => d.Agent)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Configuración: AgentLoginLog
        // ==========================================
        modelBuilder.Entity<AgentLoginLog>(entity =>
        {
            entity.ToTable("AgentLoginLogs");

            entity.HasKey(e => e.LoginLogId);

            entity.Property(e => e.AttemptedEmail)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.FailureReason)
                .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.LoginAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Índices
            entity.HasIndex(e => new { e.AgentId, e.LoginAt });
            entity.HasIndex(e => e.IpAddress);

            // Relación con Agent (Set Null si se elimina el agente)
            entity.HasOne(d => d.Agent)
                .WithMany(p => p.LoginLogs)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ==========================================
        // Configuración: Client
        // ==========================================
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Guid)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            entity.HasIndex(c => c.Guid).IsUnique();
        });

        // Configuración de la entidad Policy
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("Policies");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Guid)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            entity.HasIndex(p => p.Guid).IsUnique();

            entity.Property(p => p.InsuredFirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.InsuredLastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.PolicyType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.PolicyNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Company)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.PaymentFrequency)
                .IsRequired()
                .HasMaxLength(30);

            // Configuración de precisión decimal
            entity.Property(p => p.NetPremium)
                .HasPrecision(18, 2);

            entity.Property(p => p.TotalPremium)
                .HasPrecision(18, 2);

            entity.Property(p => p.CommissionPercentage)
                .HasPrecision(5, 2);

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relación uno a muchos: Client -> Policies
            entity.HasOne(p => p.Client)
                .WithMany(c => c.Policies)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}