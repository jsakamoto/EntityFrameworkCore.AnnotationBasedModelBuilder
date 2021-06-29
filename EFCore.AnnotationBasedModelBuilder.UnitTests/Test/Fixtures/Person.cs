namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures
{
    public class Person
    {
        public int Id { get; set; }

        public FullName Name { get; set; }

        [Foo]
        public int Age { get; set; }
    }
}
