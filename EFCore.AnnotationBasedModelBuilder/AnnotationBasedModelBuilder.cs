using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders
{
    public static class AnnotationBasedModelBuilder
    {
        public delegate TBuilderArg[] CreateBuilderArgument<TAttribute, TBuilderArg>(AnnotatedProperty<TAttribute>[] annotatedProperties)
            where TAttribute : Attribute;

        public delegate void BuildModel<TBuilderArg>(EntityTypeBuilder builder1, OwnedNavigationBuilder builder2, TBuilderArg builderArg);

        public static void Build<TAttribute>(ModelBuilder modelBuilder, BuildModel<AnnotatedProperty<TAttribute>> build)
            where TAttribute : Attribute
        {
            Build<TAttribute, AnnotatedProperty<TAttribute>>(modelBuilder, _ => _, build);
        }

        public static void Build<TAttribute, TBuilderArg>(
            ModelBuilder modelBuilder,
            CreateBuilderArgument<TAttribute, TBuilderArg> createBuilderArgument,
            BuildModel<TBuilderArg> build
        ) where TAttribute : Attribute
        {
            var entityTypes = modelBuilder.Model.GetEntityTypes();

            Parallel.ForEach(entityTypes, entityType =>
            {
                var builderArgs = CreateBuilderArguments(entityType, createBuilderArgument);
                if (builderArgs.Length == 0) return;

                Action<TBuilderArg> buildModel = BuildAction<TBuilderArg>(modelBuilder, entityType, build);

                lock (modelBuilder)
                {
                    foreach (var arg in builderArgs)
                    {
                        buildModel(arg);
                    }
                }
            });
        }

        private static TBuilderArg[] CreateBuilderArguments<TBuilderArg, TAttribute>(
            IMutableEntityType entityType,
            CreateBuilderArgument<TAttribute, TBuilderArg> createBuilderArgument
        )
            where TAttribute : Attribute
        {
            var annotatedProperties = entityType.ClrType
                .GetProperties()
                .SelectMany(prop => Attribute.GetCustomAttributes(prop, typeof(TAttribute))
                    .Cast<TAttribute>()
                    .Select(attrib => new AnnotatedProperty<TAttribute>(prop.Name, attrib))
                ).ToArray();

            return createBuilderArgument(annotatedProperties);
        }

        private static Action<TBuilderArg> BuildAction<TBuilderArg>(
            ModelBuilder modelBuilder,
            IMutableEntityType entityType,
            BuildModel<TBuilderArg> build
        )
        {
            if (!entityType.IsOwned())
            {
                return (TBuilderArg arg) => build(modelBuilder.Entity(entityType.ClrType), null, arg);
            }
            else
            {
                return (TBuilderArg arg) =>
                {
                    modelBuilder.BuildForOwnedType(entityType, builder => build(null, builder, arg));
                };
            }
        }

        private static void BuildForOwnedType(this ModelBuilder modelBuilder, IEntityType owned, Action<OwnedNavigationBuilder> buildAction)
        {
            var owner = owned.DefiningEntityType ?? owned.GetForeignKeys().FirstOrDefault(k => k.IsOwnership)?.PrincipalEntityType;
            var definingNavigationName = owned.DefiningNavigationName ?? owned.GetForeignKeys().FirstOrDefault(k => k.IsOwnership)?.PrincipalToDependent.Name;
            if (owner == null) throw new Exception("Owner type could not determind.");
            if (definingNavigationName == null) throw new Exception("Defining navigation name could not determind.");
            if (!owner.IsOwned())
            {
                modelBuilder.Entity(owner.ClrType)
                    .OwnsOne(owned.ClrType, definingNavigationName, buildAction);
            }
            else
            {
                modelBuilder.BuildForOwnedType(owner, builder =>
                    builder.OwnsOne(owned.ClrType, definingNavigationName, buildAction));
            }
        }
    }
}
