//-----------------------------------------------------------------------
// <copyright file="PropertyMapping.cs" company="UNMS-NVSM">
//     Copyright (c) UNMS-NVSM. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Framework.Metadata.Model
{
    public class PropertyMapping
    {
        private readonly string columnName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapping"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public PropertyMapping(string columnName)
        {
            this.columnName = columnName;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get { return columnName; } }
    }

}