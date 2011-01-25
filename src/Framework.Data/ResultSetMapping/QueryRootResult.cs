namespace Framework.Data.ResultSetMapping
{
    using System;

    /// <summary>
    /// Represents a return obtained from the root of the result set.
    /// </summary>
    public class QueryRootResult : NonScalarQueryResult
    {
        private readonly string entityName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRootResult"/> class.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        public QueryRootResult(string entityName) : this(string.Empty, entityName) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRootResult"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="entityName">Name of the entity.</param>
        public QueryRootResult(string alias, string entityName)
            : base(alias, null)
        {
            this.entityName = entityName;
        }

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <value>The name of the entity.</value>
        public string EntityName { get { return entityName; } }
    }
}
