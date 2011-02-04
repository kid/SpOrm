namespace Framework.Metadata.Mapping
{
    using System.Reflection;
    using Framework.Metadata.Mapping.Providers;
    using Framework.Metadata.Model;

    public class PropertyBuilder : IPropertyMappingProvider
    {
        private string columnName;
        private MemberInfo memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBuilder"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public PropertyBuilder(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }

        public PropertyBuilder Column(string name)
        {
            columnName = name;
            return this;
        }

        /// <summary>
        /// Gets the property mapping.
        /// </summary>
        /// <value>The property mapping.</value>
        public PropertyMapping PropertyMapping
        {
            get { return new PropertyMapping(columnName); }
        }
    }
}
