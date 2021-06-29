using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            var entityTypes = modelBuilder.Model.GetEntityTypes().OfType<EntityType>();

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

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static TBuilderArg[] CreateBuilderArguments<TBuilderArg, TAttribute>(
            EntityType entityType,
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

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static Action<TBuilderArg> BuildAction<TBuilderArg>(
            ModelBuilder modelBuilder,
            EntityType entityType,
            BuildModel<TBuilderArg> build
        )
        {
            if (!IsOwned(entityType))
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

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static void BuildForOwnedType(this ModelBuilder modelBuilder, EntityType owned, Action<OwnedNavigationBuilder> buildAction)
        {
            var ownershipKey = owned.GetForeignKeys().FirstOrDefault(k => k.IsOwnership);
            var owner = ownershipKey?.PrincipalEntityType;
            var definingNavigationName = ownershipKey?.PrincipalToDependent.Name;
            if (owner == null) throw new Exception("Owner type could not determind.");
            if (definingNavigationName == null) throw new Exception("Defining navigation name could not determind.");
            if (!IsOwned(owner))
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

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static bool IsOwned(EntityType entityType)
        {
            return entityType.GetForeignKeys().Any(fk => fk.IsOwnership);
        }
    }
}
