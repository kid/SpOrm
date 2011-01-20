using System.Reflection;

namespace Framework.Data
{
    public class ColumnMapping
    {
        public string ColumnName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }
}
