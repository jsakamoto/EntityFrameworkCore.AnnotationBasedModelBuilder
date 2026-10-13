# AGENTS.md

This file provides guidance to AI coding agents when working with code in this repository.

## Overview

NuGet library `Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder`: shared infrastructure that lets other libraries (IndexAttribute, DecimalAttribute for EF Core) apply attribute-driven configuration to an EF Core `ModelBuilder`. The solution (`EFCore.AnnotationBasedModelBuilder.slnx`) has two projects: the library and an xUnit v3 test project (Microsoft.Testing.Platform).

## Commands

```
dotnet build
dotnet test --project EFCore.AnnotationBasedModelBuilder.Test                        # all target frameworks (net8/9/10), as CI does (MTP mode via global.json, needs .NET 10+ SDK)
dotnet test --project EFCore.AnnotationBasedModelBuilder.Test -f net9.0 --filter-method "*Build_Test"   # single test / single TFM
```

Building the library also produces a `.nupkg` into `dist/` (`GeneratePackageOnBuild`). Tests need .NET 8, 9 and 10 SDKs/runtimes.

## Architecture

All logic is in `EFCore.AnnotationBasedModelBuilder/AnnotationBasedModelBuilder.cs`. Typical consumer flow: call `AnnotationBasedModelBuilder.Build<TAttribute[, TBuilderArg]>(modelBuilder, createBuilderArgument, build)` from `OnModelCreating`.

- For each entity type in the model (processed with `Parallel.ForEach`), it reflects over the CLR type's properties collecting `AnnotatedProperty<TAttribute>` (property name + attribute instance).
- The optional `createBuilderArgument` callback turns those into `TBuilderArg[]` (for example grouping properties into one composite index); the single-generic overload uses identity.
- The `build` callback receives `(EntityTypeBuilder, OwnedNavigationBuilder, arg)`. Exactly one of the two builders is non-null: the first for regular entities, the second for owned types.
- Owned types are reached by walking the ownership foreign key back to the owner and calling `OwnsOne(...)` on it. This recurses for nested owned types, so the builder is always obtained from the root entity downward. This is the part that needs care when changing anything.
- `build` invocations are serialized with `lock (modelBuilder)`, because `ModelBuilder` is not thread-safe. Only reflection and `createBuilderArgument` run in parallel, so those callbacks must be thread-safe (the test locks around its shared lists).

The code uses EF Core internal APIs (`EntityType`, `EF1001`, suppressed via `NoWarn` and `SuppressMessage`). The library intentionally targets old EF Core versions (`netstandard2.0` with EF Core Relational 3.1.0, `netstandard2.1` with 3.0.0) so that one package works with EF Core 3.0 through 10. Do not bump those package versions. The test project is where compatibility with EF Core 8/9/10 is verified, using Sqlite in memory.

## Releasing

Version lives in the library csproj (`<Version>`). Package release notes are extracted from the top entry of `RELEASE-NOTES.txt` (the entry must start with `v.x.y.z`), so add new entries at the top in that format. The README has a supported-versions table that should be kept in sync with EF Core support changes.
