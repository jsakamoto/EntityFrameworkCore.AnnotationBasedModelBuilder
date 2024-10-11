using Microsoft.EntityFrameworkCore;
using Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures;
using Xunit;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test;

public class AnnotationBasedModelBuilderTest
{
    [Fact]
    public void Build_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var builderArguments = new List<FooBuilderArgument>();
        var buildLog = new List<string>();
        using var dbContext = new TestDbContext(options, modelBuilder =>
        {
            AnnotationBasedModelBuilder.Build<FooAttribute, FooBuilderArgument>(
                modelBuilder,
                (props) =>
                {
                    var args = props.Select(prop => new FooBuilderArgument(prop)).ToArray();
                    lock (builderArguments) builderArguments.AddRange(args);
                    return args;
                },
                (b1, b2, arg) =>
                {
                    lock (buildLog) buildLog.Add($"{(b1 == null ? "(null)" : "b1")};{(b2 == null ? "(null)" : "b2")};{arg}");
                });
        });

        dbContext.Model.IsNotNull();

        builderArguments
            .Select(arg => arg.ToString())
            .OrderBy(text => text)
            .Is("Age", "LastName");

        buildLog
            .OrderBy(text => text)
            .Is("(null);b2;LastName", "b1;(null);Age");
    }
}
