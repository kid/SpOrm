namespace Framework.Data.ResultSetMapping
{
    using System;
    using System.Collections.Generic;

    public abstract class NonScalarQueryResult : IQueryResultDescription
    {
        private readonly string alias;
        private readonly IDictionary<string, string[]> propertyMappings;

        protected NonScalarQueryResult(string alias, IDictionary<string, string[]> propertyMappings)
        {
            if (alias == null) throw new ArgumentNullException("alias");

            this.alias = alias;
            this.propertyMappings = propertyMappings ?? new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get { return alias; } }

        /// <summary>
        /// Gets the property mappings.
        /// </summary>
        /// <value>The property mappings.</value>
        public IDictionary<string, string[]> PropertyMappings { get { return propertyMappings; } }
    }
}
