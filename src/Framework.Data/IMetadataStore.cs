using System;

namespace Framework.Data
{
    public interface IMetadataStore
    {
        EntityMapping GetMapping(Type entityType);
    }
}
