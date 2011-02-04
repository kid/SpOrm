namespace Framework.Metadata.Mapping.Providers
{
    using Framework.Metadata.Model;

    public interface IPropertyMappingProvider
    {
        /// <summary>
        /// Gets the property mapping.
        /// </summary>
        /// <value>The property mapping.</value>
        PropertyMapping PropertyMapping { get; }
    }
}
