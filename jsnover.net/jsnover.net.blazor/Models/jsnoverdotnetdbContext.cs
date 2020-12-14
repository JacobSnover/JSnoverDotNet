using System;
using jsnover.net.blazor.DataTransferObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace jsnover.net.blazor.Models
{
    public partial class jsnoverdotnetdbContext : DbContext
    {
        public jsnoverdotnetdbContext()
        {
        }

        public jsnoverdotnetdbContext(DbContextOptions<jsnoverdotnetdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Commentors> Commentors { get; set; }
        public virtual DbSet<ContactRequest> ContactRequest { get; set; }
        public virtual DbSet<Owners> Owners { get; set; }
        public virtual DbSet<Photos> Photos { get; set; }
        public virtual DbSet<Subscribers> Subscribers { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<VisitorCounter> VisitorCounter { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ContextOptions.connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasDatabaseName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasDatabaseName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasDatabaseName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.EditDate).HasColumnType("date");

                entity.Property(e => e.SubmitDate).HasColumnType("date");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Topic).HasMaxLength(250);
            });

            modelBuilder.Entity<Commentors>(entity =>
            {
                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.DatePosted).HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(50);

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.Commentors)
                    .HasForeignKey(d => d.BlogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commentors_Blogs");
            });

            modelBuilder.Entity<ContactRequest>(entity =>
            {
                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(100)
                    .IsFixedLength();

                entity.Property(e => e.DatePosted).HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            modelBuilder.Entity<Owners>(entity =>
            {
                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Photos>(entity =>
            {
                entity.Property(e => e.Link)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.Photos)
                    .HasForeignKey(d => d.BlogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Photos_Blog");
            });

            modelBuilder.Entity<Subscribers>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.SubscribeDate).HasColumnType("date");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.Tag)
                    .HasForeignKey(d => d.BlogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tag_Blog");
            });

            modelBuilder.Entity<VisitorCounter>(entity =>
            {
                entity.Property(e => e.VisitorCount).HasColumnName("visitorCount");

                entity.Property(e => e.VisitorCountHundreds).HasColumnName("visitorCountHundreds");

                entity.Property(e => e.VisitorDate)
                    .HasColumnName("visitorDate")
                    .HasColumnType("date");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
