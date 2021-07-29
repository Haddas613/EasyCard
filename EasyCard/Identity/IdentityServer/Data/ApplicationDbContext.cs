using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer.Data.Entities;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<UserAudit> UserAudits { get; set; }

        public DbSet<UserPasswordSnapshot> UserPasswordSnapshots { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserAuditConfiguration());
            modelBuilder.ApplyConfiguration(new UserPasswordSnapshotConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        internal class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
        {
            public void Configure(EntityTypeBuilder<UserAudit> builder)
            {
                builder.ToTable("UserAudit");

                builder.HasKey(b => b.UserAuditID);
                builder.Property(b => b.UserAuditID).ValueGeneratedOnAdd();

                builder.Property(b => b.UserId).IsRequired(false);

                builder.Property(b => b.OperationDate).IsRequired();

                builder.Property(b => b.OperationCode).IsRequired().HasMaxLength(30).IsUnicode(false);

                builder.Property(b => b.OperationDescription).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.Email).IsRequired(true).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);
            }
        }

        internal class UserPasswordSnapshotConfiguration : IEntityTypeConfiguration<UserPasswordSnapshot>
        {
            public void Configure(EntityTypeBuilder<UserPasswordSnapshot> builder)
            {
                builder.ToTable("UserPasswordSnapshot");

                builder.HasKey(b => b.UserPasswordSnapshotID);
                builder.Property(b => b.UserPasswordSnapshotID).ValueGeneratedOnAdd();

                builder.Property(b => b.UserId).IsRequired(true);

                builder.Property(b => b.Created).IsRequired(true);

                builder.Property(b => b.HashedPassword).IsRequired(true).HasMaxLength(512).IsUnicode(false);

                builder.Property(b => b.SecurityStamp).IsRequired(true).HasMaxLength(512).IsUnicode(false);
            }
        }
    }
}
