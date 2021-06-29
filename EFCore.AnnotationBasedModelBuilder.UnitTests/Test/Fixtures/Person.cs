namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures
{
    public class Person
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = "";

        [Foo]
        public string LastName { get; set; } = "";
    }
}
