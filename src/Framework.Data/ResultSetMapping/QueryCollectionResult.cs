namespace Framework.Data.ResultSetMapping
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection initialiser.
    /// </summary>
    public class QueryCollectionResult : NonScalarQueryResult
    {
        private readonly string ownerEntityName;
        private readonly string ownerProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCollectionResult"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="ownerEntityName">Name of the owner entity.</param>
        /// <param name="ownerProperty">The owner property.</param>
        /// <param name="propertyMappings">The property mappings.</param>
        public QueryCollectionResult(string alias, string ownerEntityName, string ownerProperty, IDictionary<string, string[]> propertyMappings)
            : base(alias, propertyMappings)
        {
            this.ownerEntityName = ownerEntityName;
            this.ownerProperty = ownerProperty;
        }

        /// <summary>
        /// Gets the name of the owner entity.
        /// </summary>
        /// <value>The name of the owner entity.</value>
        public string OwnerEntityName { get { return ownerEntityName; } }

        /// <summary>
        /// Gets the owner property.
        /// </summary>
        /// <value>The owner property.</value>
        public string OwnerProperty { get { return ownerProperty; } }
    }
}
