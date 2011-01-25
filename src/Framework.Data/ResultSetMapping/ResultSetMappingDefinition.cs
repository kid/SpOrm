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
        public void AddQueryReturn(IQueryResultDescription queryReturn)
        {
            if (queryReturn == null) throw new ArgumentNullException("queryReturn");

            queryReturns.Add(queryReturn);
        }

        /// <summary>
        /// Gets the query returns.
        /// </summary>
        /// <value>The query returns.</value>
        public IEnumerable<IQueryResultDescription> QueryReturns { get { return queryReturns; } }
    }
}
