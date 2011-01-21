using System;
using System.Collections.Generic;
using System.Globalization;

namespace Framework.Data
{
    public class EntityMapping
    {
        private readonly IDictionary<string, ColumnMapping> columns = new Dictionary<string, ColumnMapping>();
        private readonly IDictionary<string, OneToOneRelationMapping> oneToOneRelations = new Dictionary<string, OneToOneRelationMapping>();

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>The primary key.</value>
        public PrimaryKeyMapping PrimaryKey { get; set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ColumnMapping> Columns { get { return columns.Values; } }

        /// <summary>
        /// Gets the one to one relations.
        /// </summary>
        /// <value>The one to one relations.</value>
        public IEnumerable<OneToOneRelationMapping> OneToOneRelations { get { return oneToOneRelations.Values; } }

        public Type EntityType { get; set; }

        public void AddColumn(ColumnMapping columnMapping)
        {
            if (columnMapping == null) throw new ArgumentNullException("columnMapping");

            if (columns.ContainsKey(columnMapping.ColumnName))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, "This entity mapping already contains a column named {0}.", columnMapping.ColumnName)
                );
            }
            columns.Add(columnMapping.ColumnName, columnMapping);
        }
    }
}
