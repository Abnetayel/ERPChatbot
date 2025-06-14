using Microsoft.EntityFrameworkCore;
using ERPChatbotAssistant.Server.Models;

namespace ERPChatbotAssistant.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Messages)
            .WithOne(m => m.ChatSession)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 