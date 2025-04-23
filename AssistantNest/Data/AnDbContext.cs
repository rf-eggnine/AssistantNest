// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Eggnine.Common;
using AssistantNest.Models;
using System.Threading.Tasks;
using System.Threading;
using System;

public class AnDbContext : DbContext
{
    public AnDbContext(DbContextOptions<AnDbContext> options)
        : base(options)
    {
        Users = Set<AnUser>();
    }

    public DbSet<AnUser> Users {get; init;}

    public static ModelBuilder GetModelBuilder()
    {
        ModelBuilder modelBuilder = new();
        EntityTypeBuilder<AnUser> userBuilder = modelBuilder.Entity<AnUser>();
        userBuilder.ToTable("Users");
        userBuilder.HasKey(a => a.Id);
        userBuilder.Property(nameof(AnUser.Id)).HasColumnType("uuid");
        userBuilder.Property(nameof(AnUser.Name)).HasColumnType("varchar");
        userBuilder.Property(nameof(AnUser.AcceptedCookiesAt)).HasColumnType("timestamp");
        userBuilder.Property(nameof(AnUser.EncounteredAt)).HasColumnType("timestamp");
        userBuilder.Property(nameof(AnUser.SignedUpAt)).HasColumnType("timestamp");
        userBuilder.Property(nameof(AnUser.UpdatedAt)).HasColumnType("timestamp");
        userBuilder.Property(nameof(AnUser.EncryptedPassphrase)).HasColumnType("varchar");
        userBuilder.Ignore(nameof(AnUser.Claims));
        userBuilder.Ignore(nameof(AnUser.Identity));
        userBuilder.Ignore(nameof(AnUser.Identities));
        userBuilder.HasMany(u => u.Projects)
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        EntityTypeBuilder<AnProject> projectBuilder = modelBuilder.Entity<AnProject>();
        projectBuilder.ToTable("Projects");
        projectBuilder.HasKey(a => a.Id);
        projectBuilder.Property(nameof(AnProject.Id)).HasColumnType("uuid");
        projectBuilder.Property(nameof(AnProject.Name)).HasColumnType("varchar");
        projectBuilder.Property(nameof(AnProject.OpenAiOrganizationId)).HasColumnType("varchar");
        projectBuilder.Property(nameof(AnProject.Description)).HasColumnType("varchar");
        projectBuilder.Property(nameof(AnProject.EncryptedApiKey)).HasColumnType("varchar");
        projectBuilder.Property(nameof(AnProject.CreatedAt)).HasColumnType("timestamp");
        projectBuilder.Property(nameof(AnProject.UpdatedAt)).HasColumnType("timestamp");
        projectBuilder.Property(nameof(AnProject.DeletedAt)).HasColumnType("timestamp");

        return modelBuilder;
    }
}
