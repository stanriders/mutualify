﻿using Microsoft.EntityFrameworkCore;
using Mutualify.Database.Models;

namespace Mutualify.Database;

public sealed class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Token> Tokens { get; set; } = null!;

    public DbSet<Relation> Relations { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    private DatabaseContext() { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Relation>().HasIndex(x=> x.ToId);
        modelBuilder.Entity<Relation>().HasKey(x => new { x.FromId, x.ToId });

        modelBuilder.Entity<User>().HasIndex(x => x.Rank);

        base.OnModelCreating(modelBuilder);
    }
}
