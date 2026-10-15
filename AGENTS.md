# AGENTS.md

This file provides guidance to AI coding agents when working with code in this repository.

## Overview

NuGet library `Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder`: shared infrastructure that lets other libraries (IndexAttribute, DecimalAttribute for EF Core) apply attribute-driven configuration to an EF Core `ModelBuilder`. The solution (`EFCore.AnnotationBasedModelBuilder.slnx`) has two projects: the library and an xUnit v3 test project (Microsoft.Testing.Platform).

## Commands

```
dotnet build
cd EFCore.AnnotationBasedModelBuilder.Test
dotnet test                                            # all target frameworks (net8/9/10/11), as CI does (MTP mode via global.json)
dotnet test -f net9.0 --filter-method "*Build_Test"    # single test / single TFM
```

Building the library also produces a `.nupkg` into `dist/` (`GeneratePackageOnBuild`). Tests need .NET 8, 9, 10 and 11 SDKs/runtimes. The .NET 11 SDK is required to build `net11.0`.

Run `dotnet test` inside the test project folder. The .NET 11 RC SDK resolves `dotnet test --project <folder>` against the wrong path and fails.

## Test assertions

The tests use the `ChainingAssertion-xUnit.Bin` extension methods (`.Is(...)`, `.IsNotNull()`) with `using Xunit;`.

## Architecture

All logic is in `EFCore.AnnotationBasedModelBuilder/AnnotationBasedModelBuilder.cs`. Typical consumer flow: call `AnnotationBasedModelBuilder.Build<TAttribute[, TBuilderArg]>(modelBuilder, createBuilderArgument, build)` from `OnModelCreating`.

- For each entity type in the model (processed with `Parallel.ForEach`), it reflects over the CLR type's properties collecting `AnnotatedProperty<TAttribute>` (property name + attribute instance).
- The optional `createBuilderArgument` callback turns those into `TBuilderArg[]` (for example grouping properties into one composite index); the single-generic overload uses identity.
- The `build` callback receives `(EntityTypeBuilder, OwnedNavigationBuilder, arg)`. Exactly one of the two builders is non-null: the first for regular entities, the second for owned types.
- Owned types are reached by walking the ownership foreign key back to the owner and calling `OwnsOne(...)` on it. This recurses for nested owned types, so the builder is always obtained from the root entity downward. This is the part that needs care when changing anything.
- `build` invocations are serialized with `lock (modelBuilder)`, because `ModelBuilder` is not thread-safe. Only reflection and `createBuilderArgument` run in parallel, so those callbacks must be thread-safe (the test locks around its shared lists).

The code uses EF Core internal APIs (`EntityType`, `EF1001`, suppressed via `NoWarn` and `SuppressMessage`).

- On a derived entity type, properties inherited from the base entity type are skipped, because the base entity type configures them. EF Core rejects some settings on derived types. For example, `HasKey()` throws "A key cannot be configured on ... because it is a derived type" (IndexAttribute issue #12).

### Binary compatibility trap with EF Core metadata APIs

The library is compiled against EF Core 3.x, but it runs on EF Core 3.0 through 11. The compiled code records the interface that declares each member it calls. Later EF Core versions moved some members to other interfaces. Such code still compiles against the new EF Core, but it throws `MissingMethodException` at runtime. For example, `entityType.BaseType.ClrType` on the `IMutableEntityType` interface fails on EF Core 8 and later, because `ClrType` is no longer declared on `ITypeBase`.

- To read the CLR type, cast to the internal `EntityType` class and use its `ClrType` property, as the existing code does. `IMutableEntityType.BaseType` itself still works.
- Run the tests on every TFM after you touch any EF Core metadata API. A build that passes proves nothing here.

The same trap applies to IndexAttribute and DecimalAttribute, which are also compiled against EF Core 3.x.

## Target frameworks and EF Core versions

Follow these three rules.

1. **The library lists each .NET version from `net10.0` on.** It targets `netstandard2.0` and `netstandard2.1`, plus `net10.0` and `net11.0` one by one. When a new .NET major version ships, add its TFM. These TFMs are there so that nuget.org shows them as supported.
2. **The library keeps the oldest EF Core reference on every TFM.** It references EF Core Relational 3.0.0 (3.1.0 for `netstandard2.0`, because EF Core 3.0 does not support it). This keeps one package usable with EF Core 3.0 through the latest. Never raise it, not even for `net10.0` or later. A newer reference would force users to upgrade EF Core, for example a .NET 10 app that still uses EF Core 8.
3. **The test project uses the latest EF Core of the same major version as each TFM.** It targets `net8.0` to `net11.0`, and each TFM has a conditional `PackageReference` group (for example, `net10.0` uses the latest EF Core 10.0.x). This is where compatibility with each EF Core version is checked, using Sqlite in memory. While a .NET version is still in preview or RC, use its latest prerelease (now `11.0.0-rc.2.26504.105`). When you bump versions, update every group.

## Releasing

Version lives in the library csproj (`<Version>`). Package release notes are extracted from the top entry of `RELEASE-NOTES.txt` (the entry must start with `v.x.y.z`), so add new entries at the top in that format. The README has a supported-versions table that should be kept in sync with EF Core support changes.
