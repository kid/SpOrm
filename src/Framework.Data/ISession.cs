using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Data
{
    public interface ISession
    {
        /// <summary>
        /// Refreshes the specified entity with values from the database.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entity">The entity.</param>
        void Refresh(Type entityType, object entity);
    }
}
