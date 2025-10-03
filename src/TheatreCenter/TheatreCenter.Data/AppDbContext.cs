using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }




        // Сущности базы данных
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Musical> Musicals { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Theatre> Theatres { get; set; }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }

        // Таблицы связей многие-ко-многим
        public DbSet<ActorRole> ActorRoles { get; set; }
        public DbSet<MusicalTheme> MusicalThemes { get; set; }
        public DbSet<AccountTheatreFavorite> AccountTheatreFavorites { get; set; }
        public DbSet<AccountMusicalFavorite> AccountMusicalFavorites { get; set; }
        public DbSet<AccountActorFavorite> AccountActorFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(a => a.PasswordHash)
                    .IsRequired();
                entity.Property(a => a.AccessLevel)
                    .HasConversion<string>() 
                    .IsRequired();
                entity.Property(a => a.UpgradeRequest)
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.HasIndex(a => a.Username).IsUnique();
            });

            // Конфигурация для Actor
            modelBuilder.Entity<Actor>(entity =>
            {
                entity.Property(a => a.Id)
                    .ValueGeneratedOnAdd(); 
                entity.Property(a => a.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(a => a.VoiceType)
                    .HasConversion<string>();
            });

            // Конфигурация для Musical
            modelBuilder.Entity<Musical>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Title)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(m => m.AgeRestriction)
                    .HasConversion<string>();

                entity.HasOne(m => m.Theatre)
                    .WithMany(t => t.Musicals)
                    .HasForeignKey(m => m.TheatreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация для Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RoleType)
                    .HasConversion<string>();

                entity.HasOne(r => r.Musical)
                    .WithMany(m => m.Roles)
                    .HasForeignKey(r => r.MusicalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для Theatre
            modelBuilder.Entity<Theatre>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // Конфигурация для Show
            modelBuilder.Entity<Show>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasOne(s => s.Musical)
                    .WithMany(m => m.Shows)
                    .HasForeignKey(s => s.MusicalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для Theme
            modelBuilder.Entity<Theme>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // Конфигурация для ActorRole (актер ↔ роль)
            modelBuilder.Entity<ActorRole>(entity =>
            {
                entity.HasKey(ar => new { ar.ActorId, ar.RoleId });

                entity.HasOne(ar => ar.Actor)
                    .WithMany(a => a.ActorRoles)
                    .HasForeignKey(ar => ar.ActorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ar => ar.Role)
                    .WithMany(r => r.ActorRoles)
                    .HasForeignKey(ar => ar.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для MusicalTheme (мюзикл ↔ тематика)
            modelBuilder.Entity<MusicalTheme>(entity =>
            {
                entity.HasKey(mt => new { mt.MusicalId, mt.ThemeId });

                entity.HasOne(mt => mt.Musical)
                    .WithMany(m => m.MusicalThemes)
                    .HasForeignKey(mt => mt.MusicalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mt => mt.Theme)
                    .WithMany(t => t.MusicalThemes)
                    .HasForeignKey(mt => mt.ThemeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для избранного (аккаунт ↔ театр)
            modelBuilder.Entity<AccountTheatreFavorite>(entity =>
            {
                entity.HasKey(at => new { at.AccountId, at.TheatreId });

                entity.HasOne(at => at.Account)
                    .WithMany(a => a.FavoriteTheatres)
                    .HasForeignKey(at => at.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(at => at.Theatre)
                    .WithMany()
                    .HasForeignKey(at => at.TheatreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для избранного (аккаунт ↔ мюзикл)
            modelBuilder.Entity<AccountMusicalFavorite>(entity =>
            {
                entity.HasKey(am => new { am.AccountId, am.MusicalId });

                entity.HasOne(am => am.Account)
                    .WithMany(a => a.FavoriteMusicals)
                    .HasForeignKey(am => am.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(am => am.Musical)
                    .WithMany()
                    .HasForeignKey(am => am.MusicalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для избранного (аккаунт ↔ актер)
            modelBuilder.Entity<AccountActorFavorite>(entity =>
            {
                entity.HasKey(aa => new { aa.AccountId, aa.ActorId });

                entity.HasOne(aa => aa.Account)
                    .WithMany(a => a.FavoriteActors)
                    .HasForeignKey(aa => aa.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(aa => aa.Actor)
                    .WithMany()
                    .HasForeignKey(aa => aa.ActorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для CastMember (актер ↔ роль ↔ показ)
            modelBuilder.Entity<CastMember>(entity =>
            {
                entity.HasKey(cm => cm.Id);

                entity.Property(cm => cm.Comment)
                    .HasMaxLength(200);

                entity.HasOne(cm => cm.Show)
                    .WithMany(s => s.CastMembers)
                    .HasForeignKey(cm => cm.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cm => cm.Role)
                    .WithMany(r => r.CastMembers)
                    .HasForeignKey(cm => cm.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cm => cm.Actor)
                    .WithMany(a => a.CastMembers)
                    .HasForeignKey(cm => cm.ActorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //// Конфигурация для связи Actor ↔ Show через CastMember
            //modelBuilder.Entity<Actor>()
            //    .HasMany(a => a.CastMembers)
            //    .WithOne(cm => cm.Actor)
            //    .HasForeignKey(cm => cm.ActorId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Конфигурация для связи Role ↔ Show через CastMember
            //modelBuilder.Entity<Role>()
            //    .HasMany(r => r.CastMembers)
            //    .WithOne(cm => cm.Role)
            //    .HasForeignKey(cm => cm.RoleId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Оптимизация для строковых перечислений
            modelBuilder.Entity<Actor>()
                .Property(a => a.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

            modelBuilder.Entity<Role>()
                .Property(r => r.RoleType)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Musical>()
                .Property(m => m.AgeRestriction)
                .HasConversion<string>()
                .HasMaxLength(10);

            // Индексы для улучшения производительности
            modelBuilder.Entity<Show>()
                .HasIndex(s => s.Date);

            modelBuilder.Entity<Show>()
                .HasIndex(s => s.MusicalId);

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.MusicalId);

            modelBuilder.Entity<Musical>()
                .HasIndex(m => m.TheatreId);

            modelBuilder.Entity<CastMember>()
                .HasIndex(cm => cm.ShowId);

            modelBuilder.Entity<CastMember>()
                .HasIndex(cm => cm.ActorId);

            modelBuilder.Entity<CastMember>()
                .HasIndex(cm => cm.RoleId);

            // Уникальные ограничения
            modelBuilder.Entity<Theme>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<Theatre>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}