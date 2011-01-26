using System;

namespace Framework.Data
{
    public interface ISessionLevelCache
    {
        /// <summary>
        /// Tries to find the entity corresponding to the specified type and key.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        object TryToFind(Type entityType, object key);

        /// <summary>
        /// Stores the entity with the specified key.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="entity">The entity.</param>
        void Store(Type entityType, object key, object entity);
    }
}
