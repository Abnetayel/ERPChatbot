using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERPChatbotAssistant.Server.Models;

namespace ERPChatbotAssistant.Server.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConversationHistory> ConversationHistories { get; set; }
    public DbSet<TrainingData> TrainingData { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ConversationHistory>()
            .HasIndex(c => c.SessionId);

        builder.Entity<TrainingData>()
            .HasIndex(t => t.Keywords);
    }
} 