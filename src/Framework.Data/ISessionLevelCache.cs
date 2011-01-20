using System;

namespace Framework.Data
{
    public interface ISessionLevelCache
    {
        /// <summary>
        /// Tries to find the entity corresponding to the specified type and key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool TryToFind<TEntity>(object key, out TEntity entity);

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
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="entity">The entity.</param>
        void Store<TEntity>(object key, TEntity entity);
    }
}
