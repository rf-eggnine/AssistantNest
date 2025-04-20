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
        userBuilder.Property(nameof(AnUser.NameArray)).HasColumnType("char[]");
        userBuilder.Property(nameof(AnUser.EncryptedPassphraseArray)).HasColumnType("char[]");
        return modelBuilder;
    }
}
