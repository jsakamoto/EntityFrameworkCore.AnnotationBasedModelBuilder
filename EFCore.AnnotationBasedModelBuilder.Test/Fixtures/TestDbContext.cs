using System;
using Microsoft.EntityFrameworkCore;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures
{
    public class TestDbContext : DbContext
    {
        private readonly Action<ModelBuilder> _OnModelCreatingCallBack;

        public DbSet<Person> People => this.Set<Person>();

        public TestDbContext(DbContextOptions options, Action<ModelBuilder> onModelCreating)
            : base(options)
        {
            this._OnModelCreatingCallBack = onModelCreating;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this._OnModelCreatingCallBack.Invoke(modelBuilder);
        }
    }
}
