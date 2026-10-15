namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures;

public abstract class ItemBase
{
    public int Id { get; set; }

    [Foo]
    public string Code { get; set; } = "";
}

public class Item : ItemBase
{
}

public class Car : Item
{
    [Foo]
    public string Make { get; set; } = "";
}

public class Fruit : Item
{
}
