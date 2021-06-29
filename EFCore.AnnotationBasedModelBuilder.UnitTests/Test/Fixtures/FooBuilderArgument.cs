namespace Toolbelt.EntityFrameworkCore.Metadata.Builders.Test.Fixtures
{
    public class FooBuilderArgument
    {
        private readonly AnnotatedProperty<FooAttribute> _Prop;

        public FooBuilderArgument(AnnotatedProperty<FooAttribute> prop)
        {
            this._Prop = prop;
        }

        public override string ToString() => $"{_Prop.Name}";
    }
}
