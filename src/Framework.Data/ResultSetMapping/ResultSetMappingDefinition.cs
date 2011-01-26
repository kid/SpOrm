namespace Framework.Data.ResultSetMapping
{
    using System;
    using System.Collections.Generic;

    public class ResultSetMappingDefinition
    {
        private readonly List<IQueryResultDescription> queryReturns = new List<IQueryResultDescription>();

        /// <summary>
        /// Adds the query return.
        /// </summary>
        /// <param name="queryReturn">The query return.</param>
        public ResultSetMappingDefinition AddQueryReturn(IQueryResultDescription queryReturn)
        {
            if (queryReturn == null) throw new ArgumentNullException("queryReturn");

            queryReturns.Add(queryReturn);

            return this;
        }

        /// <summary>
        /// Gets the query returns.
        /// </summary>
        /// <value>The query returns.</value>
        public ICollection<IQueryResultDescription> QueryReturns { get { return queryReturns; } }
    }
}
