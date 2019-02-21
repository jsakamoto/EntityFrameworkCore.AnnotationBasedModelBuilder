using System;

namespace Toolbelt.EntityFrameworkCore.Metadata.Builders
{
    public class AnnotatedProperty<TAttribute> where TAttribute : Attribute
    {
        public string Name { get; }

        public TAttribute Attribute { get; }

        public AnnotatedProperty(string name, TAttribute attribute)
        {
            Name = name;
            Attribute = attribute;
        }
    }
}
