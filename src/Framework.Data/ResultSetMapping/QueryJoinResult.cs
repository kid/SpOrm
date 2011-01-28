namespace Framework.Data.ResultSetMapping
{
    using System.Collections.Generic;

    public class QueryJoinResult : NonScalarQueryResult
    {
        private readonly string ownerAlias;
        private readonly string ownerProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryJoinResult"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="ownerAlias">The owner alias.</param>
        /// <param name="ownerProperty">The owner property.</param>
        public QueryJoinResult(string alias, string ownerAlias, string ownerProperty)
            : this(alias, ownerAlias, ownerProperty, new Dictionary<string, string[]>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryJoinResult"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="ownerAlias">The owner alias.</param>
        /// <param name="ownerProperty">The owner property.</param>
        /// <param name="propertyMappings">The property mappings.</param>
        public QueryJoinResult(string alias, string ownerAlias, string ownerProperty, IDictionary<string, string[]> propertyMappings)
            : base(alias, propertyMappings)
        {
            this.ownerAlias = ownerAlias;
            this.ownerProperty = ownerProperty;
        }

        /// <summary>
        /// Gets the owner alias.
        /// </summary>
        /// <value>The owner alias.</value>
        public string OwnerAlias { get { return ownerAlias; } }

        /// <summary>
        /// Gets the owner property.
        /// </summary>
        /// <value>The owner property.</value>
        public string OwnerProperty { get { return ownerProperty; } }
    }
}
