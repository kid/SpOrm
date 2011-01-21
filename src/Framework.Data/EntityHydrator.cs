using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.DynamicProxy;

namespace Framework.Data
{
    public class EntityHydrator
    {
        private readonly ProxyGenerator proxyGenerator = new ProxyGenerator();
        private readonly IMetadataStore metadataStore;
        private readonly ISession session;
        private readonly ISessionLevelCache sessionLevelCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityHydrator"/> class.
        /// </summary>
        /// <param name="metadataStore">The metadata store.</param>
        /// <param name="session">The session.</param>
        /// <param name="sessionLevelCache">The session level cache.</param>
        public EntityHydrator(IMetadataStore metadataStore, ISession session, ISessionLevelCache sessionLevelCache)
        {
            this.metadataStore = metadataStore;
            this.session = session;
            this.sessionLevelCache = sessionLevelCache;
        }

        /// <summary>
        /// Hydrates the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public TEntity HydrateEntity<TEntity>(IDbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            var values = new Dictionary<string, object>();

            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return default(TEntity);
                }

                for (int index = 0; index < reader.FieldCount; index++)
                {
                    values.Add(reader.GetName(index), reader.GetValue(index));
                }
            }

            return CreateEntityFromValues<TEntity>(values);
        }

        /// <summary>
        /// Creates the entity from the specified values, or return one from the session level cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private TEntity CreateEntityFromValues<TEntity>(IDictionary<string, object> values)
        {
            var entityMapping = metadataStore.GetMapping(typeof(TEntity));

            var primaryKeyValue = values[entityMapping.PrimaryKey.ColumnName];

            TEntity entity;
            if (sessionLevelCache.TryToFind<TEntity>(primaryKeyValue, out entity))
            {
                return entity;
            }

            entity = CreateEmptyEntity<TEntity>();

            entityMapping.PrimaryKey.PropertyInfo.SetValue(entity, primaryKeyValue, null);
            SetRegularColumns(entityMapping, entity, values);
            SetOneToOneRelations(entityMapping, entity, values);

            sessionLevelCache.Store<TEntity>(primaryKeyValue, entity);

            return entity;
        }

        /// <summary>
        /// Creates an empty entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        protected virtual TEntity CreateEmptyEntity<TEntity>()
        {
            return Activator.CreateInstance<TEntity>();
        }

        /// <summary>
        /// Creates a lazy loading proxy.
        /// </summary>
        /// <param name="entityMapping">The entity mapping.</param>
        /// <param name="oneToOneMapping">The one to one mapping.</param>
        /// <param name="foreignKeyValue">The foreign key value.</param>
        /// <returns></returns>
        protected virtual object CreateLazyLoadingProxy(
            EntityMapping entityMapping,
            OneToOneRelationMapping oneToOneMapping,
            object foreignKeyValue)
        {
            if (oneToOneMapping == null) throw new ArgumentNullException("oneToOneMapping");

            var proxy = proxyGenerator.CreateClassProxy(
                oneToOneMapping.ReferenceType,
                new[] { new LazyLoadingInterceptor(entityMapping, session) }
            );

            var referencePrimaryKey = metadataStore.GetMapping(oneToOneMapping.ReferenceType).PrimaryKey;
            referencePrimaryKey.PropertyInfo.SetValue(proxy, foreignKeyValue, null);

            return proxy;
        }

        /// <summary>
        /// Sets the regular columns values.
        /// </summary>
        /// <param name="entityMapping">The entity mapping.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="row">The row.</param>
        private static void SetRegularColumns(EntityMapping entityMapping, object entity, IDictionary<string, object> row)
        {
            foreach (var columnMapping in entityMapping.Columns.Where(_ => _.PropertyInfo.CanWrite))
            {
                columnMapping.PropertyInfo.SetValue(entity, row[columnMapping.ColumnName], null);
            }
        }

        private void SetOneToOneRelations(EntityMapping entityMapping, object entity, IDictionary<string, object> values)
        {
            foreach (var oneToOneMapping in entityMapping.OneToOneRelations.Where(_ => _.PropertyInfo.CanWrite))
            {
                object foreignKeyValue = values[oneToOneMapping.ColumnName];

                if (foreignKeyValue is DBNull)
                {
                    oneToOneMapping.PropertyInfo.SetValue(entity, null, null);
                }
                else
                {
                    var referencedEntity =
                        sessionLevelCache.TryToFind(entityMapping.EntityType, foreignKeyValue) ??
                        CreateLazyLoadingProxy(entityMapping, oneToOneMapping, foreignKeyValue);

                    oneToOneMapping.PropertyInfo.SetValue(entity, referencedEntity, null);
                }
            }
        }
    }
}
