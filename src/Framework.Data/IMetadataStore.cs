using System;

namespace Framework.Data
{
    public interface IMetadataStore
    {
        EntityMapping GetMapping(string entityName);
        EntityMapping GetMapping(Type entityType);
    }
}
