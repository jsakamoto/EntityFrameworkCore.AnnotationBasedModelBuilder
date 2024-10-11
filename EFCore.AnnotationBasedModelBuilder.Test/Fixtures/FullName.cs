using Microsoft.EntityFrameworkCore;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures;

[Owned]
public class FullName
{
    public string FirstName { get; set; } = "";

    [Foo]
    public string LastName { get; set; } = "";
}
