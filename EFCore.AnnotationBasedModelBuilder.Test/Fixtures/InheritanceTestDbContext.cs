using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures;

public enum InheritanceMapping
{
    TablePerHierarchy,
    TablePerType,
    TablePerConcreteType
}

public class InheritanceTestDbContext : DbContext
{
    private readonly Action<ModelBuilder> _OnModelCreatingCallBack;

    public InheritanceMapping Mapping { get; }

    public DbSet<Item> Items => this.Set<Item>();

    public DbSet<Car> Cars => this.Set<Car>();

    public DbSet<Fruit> Fruits => this.Set<Fruit>();

    public InheritanceTestDbContext(DbContextOptions options, InheritanceMapping mapping, Action<ModelBuilder> onModelCreating)
        : base(options)
    {
        this.Mapping = mapping;
        this._OnModelCreatingCallBack = onModelCreating;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // The model is cached per context type by default, but it differs per mapping strategy here.
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, MappingModelCacheKeyFactory>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        if (this.Mapping == InheritanceMapping.TablePerType) modelBuilder.Entity<Item>().UseTptMappingStrategy();
        if (this.Mapping == InheritanceMapping.TablePerConcreteType) modelBuilder.Entity<Item>().UseTpcMappingStrategy();
        this._OnModelCreatingCallBack.Invoke(modelBuilder);
    }

    private class MappingModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) => (context.GetType(), ((InheritanceTestDbContext)context).Mapping, designTime);
    }
}
