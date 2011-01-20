using System;

namespace Framework.Data
{
    public class OneToOneRelationMapping : ColumnMapping
    {
        public Type ReferenceType { get; set; }
    }
}
