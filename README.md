# Annotation based model builder for EntityFramework Core  
[![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder.svg)](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder/)

## What's this?

This class library, for EntityFramework Core on .NET Core/.NET Framework, is an infrastructure for annotation based model building outside of EntityFramework official team.

This class library is referenced by follow libraries.

- [IndexAttribute for EF Core](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/)
- [DecimalAttribute for EF Core](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.DecimalAttribute/)

## Support Versions

EF Core Version | Suppored This Package Version
----------------|------------------------------
v.3.1           | v.3.1
v.3.0           | v.3.0, v.3.1
v.2.0, 2.1, 2.2 | v.1.0.x

## Release Note

- **v.3.1.0**
    - Supports EntityFramework Core v.3.1.0
    - Revert back to .NET Standard 2.0
- **v.3.0.0** - BREAKING CHANGE: supports EntityFramework Core v.3.0
- **v.1.0.2** - Fix: Doesn't work with owned types on EF Core v.2.1, v.2.2.
- **v.1.0.1** - Fix: Invalid XML Document comment file.
- **v.1.0.0** - 1st release


## License

[MIT License](https://github.com/jsakamoto/EntityFrameworkCore.AnnotationBasedModelBuilder/blob/master/LICENSE)

