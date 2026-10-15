# Annotation based model builder for Entity Framework Core  
[![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder.svg)](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder/) [![unit tests](https://github.com/jsakamoto/EntityFrameworkCore.AnnotationBasedModelBuilder/actions/workflows/unit-tests.yml/badge.svg?branch=master&event=push)](https://github.com/jsakamoto/EntityFrameworkCore.AnnotationBasedModelBuilder/actions/workflows/unit-tests.yml)

## What's this?

This library is a shared base for building Entity Framework Core models from your own annotations (attributes). It is not a part of the official Entity Framework Core project.

The following libraries use it.

- [IndexAttribute for EF Core](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/)
- [DecimalAttribute for EF Core](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.DecimalAttribute/)

## Supported Versions

EF Core version        | Package version
-----------------------|------------------------------
v.6, 7, ...11          | v.5.0.1 or later
v.5.0                  | v.5.0.1 or later, v.3.1
v.3.1                  | v.5.0.1 or later, v.3.1
v.3.0                  | v.5.0.1 or later, v.3.1, v.3.0
v.2.0, 2.1, 2.2        | v.1.0.x

## Release Notes

[Release notes](https://github.com/jsakamoto/EntityFrameworkCore.AnnotationBasedModelBuilder/blob/master/RELEASE-NOTES.txt)

## License

[MIT License](https://github.com/jsakamoto/EntityFrameworkCore.AnnotationBasedModelBuilder/blob/master/LICENSE)

