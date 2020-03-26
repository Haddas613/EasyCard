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
        public DbSet<TerminalApiAuthKey> TerminalApiAuthKeys { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserAuditConfiguration());
            modelBuilder.ApplyConfiguration(new TerminalApiAuthKeyConfiguration());
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

        internal class TerminalApiAuthKeyConfiguration : IEntityTypeConfiguration<TerminalApiAuthKey>
        {
            public void Configure(EntityTypeBuilder<TerminalApiAuthKey> builder)
            {
                builder.ToTable("TerminalApiAuthKey");

                builder.HasKey(b => b.TerminalApiAuthKeyID);
                builder.Property(b => b.TerminalApiAuthKeyID).ValueGeneratedOnAdd();

                builder.Property(b => b.TerminalID).IsRequired(true);

                builder.Property(b => b.Created).IsRequired();

                builder.Property(b => b.AuthKey).IsRequired(true).HasMaxLength(512).IsUnicode(false);
            }
        }
    }
}
