using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Web_Chat.Models
{
    public class DBContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public virtual DbSet<ImageFile> ImagesFiles { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Members)
                .WithMany(u => u.Chats)
                .UsingEntity<Dictionary<string, object>>(
                    "UserChat",
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j => j.HasOne<Chat>().WithMany().HasForeignKey("ChatId")
                );
        }
    }
}
