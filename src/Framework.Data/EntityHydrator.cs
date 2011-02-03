using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.DynamicProxy;
using Framework.Data.ResultSetMapping;

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
            var resultSetDefinition = new ResultSetMappingDefinition().AddQueryReturn(new QueryRootResult(typeof(TEntity).Name));
            return (TEntity)HydrateEntity(command, resultSetDefinition);
        }

        /// <summary>
        /// Hydrates the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> HydrateEntities<TEntity>(IDbCommand command)
        {
            var resultSetDefinition = new ResultSetMappingDefinition().AddQueryReturn(new QueryRootResult(typeof(TEntity).Name));
            return HydrateEntities(command, resultSetDefinition).Cast<TEntity>();
        }

        public IEnumerable<TEntity> HydrateEntities<TEntity>(IDbCommand command, ResultSetMappingDefinition resultSetMapping)
        {
            return HydrateEntities(command, resultSetMapping).Cast<TEntity>();
        }

        public object HydrateEntity(IDbCommand command, ResultSetMappingDefinition resultSetMapping)
        {
            return HydrateEntities(command, resultSetMapping).Cast<object>().First();
        }

        public IEnumerable HydrateEntities(IDbCommand command, ResultSetMappingDefinition resultSetMapping)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (resultSetMapping == null) throw new ArgumentNullException("resultSetMapping");

            var rows = new List<Dictionary<string, object>>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var values = new Dictionary<string, object>();
                    for (int index = 0; index < reader.FieldCount; index++)
                    {
                        values.Add(reader.GetName(index), reader.GetValue(index));
                    }
                    rows.Add(values);
                }
            }

            var results = resultSetMapping.QueryReturns.Reverse().Select<IQueryResultDescription, object>(
                queryResultDescription =>
                {
                    var rootResultDescription = queryResultDescription as QueryRootResult;
                    if (rootResultDescription != null) return Process(rootResultDescription, rows);
                    var joinResultMapping = queryResultDescription as QueryJoinResult;
                    if (joinResultMapping != null) return Process(resultSetMapping, joinResultMapping, rows);
                    throw new NotSupportedException();
                });
            return results.Reverse();
        }

        private object Process(ResultSetMappingDefinition resultSetMapping, QueryJoinResult joinResultMapping, List<Dictionary<string, object>> rows)
        {
            var parentEntityMapping = metadataStore.GetMapping(
                resultSetMapping.QueryReturns.OfType<QueryRootResult>().First(_ => joinResultMapping.OwnerAlias == _.Alias).EntityName
            );

            var entityMapping = metadataStore.GetMapping(
                parentEntityMapping.OneToOneRelations.FirstOrDefault(_ => _.ColumnName == joinResultMapping.OwnerProperty).ReferenceType
            );

            if (entityMapping != null)
            {
                var prefix = joinResultMapping.Alias + "_";
                var rowValues = rows.First()
                    .Where(_ => _.Key.StartsWith(prefix, StringComparison.Ordinal))
                    .ToDictionary(_ => _.Key.Substring(prefix.Length), _ => _.Value);

                return CreateEntityFromValues(entityMapping, rowValues);
            }
            throw new NotSupportedException();
        }

        private object Process(QueryRootResult rootResultMapping, IList<Dictionary<string, object>> rows)
        {
            var rowValues = rows.First();
            if (!string.IsNullOrEmpty(rootResultMapping.Alias))
            {
                var prefix = rootResultMapping.Alias + "_";
                rowValues = rowValues
                    .Where(_ => _.Key.StartsWith(prefix, StringComparison.Ordinal))
                    .ToDictionary(_ => _.Key.Substring(prefix.Length), _ => _.Value);
            }
            return CreateEntityFromValues(metadataStore.GetMapping(rootResultMapping.EntityName), rowValues);
        }

        /// <summary>
        /// Creates the entity from the specified values, or return one from the session level cache.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private object CreateEntityFromValues(EntityMapping entityMapping, IDictionary<string, object> values)
        {
            if (entityMapping == null) throw new ArgumentNullException("entityMapping");

            var primaryKeyValue = values[entityMapping.PrimaryKey.ColumnName];

            object entity = sessionLevelCache.TryToFind(entityMapping.EntityType, primaryKeyValue);
            if (entity != null)
            {
                return entity;
            }

            entity = CreateEmptyEntity(entityMapping.EntityType);

            entityMapping.PrimaryKey.PropertyInfo.SetValue(entity, primaryKeyValue, null);
            SetRegularColumns(entityMapping, entity, values);
            SetOneToOneRelations(entityMapping, entity, values);

            sessionLevelCache.Store(entityMapping.EntityType, primaryKeyValue, entity);

            return entity;
        }

        /// <summary>
        /// Creates an empty entity.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        protected virtual object CreateEmptyEntity(Type entityType)
        {
            return Activator.CreateInstance(entityType);
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
                var value = row[columnMapping.ColumnName];
                if (value is DBNull) value = null;
                columnMapping.PropertyInfo.SetValue(entity, value, null);
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
                        sessionLevelCache.TryToFind(oneToOneMapping.ReferenceType, foreignKeyValue) ??
                        CreateLazyLoadingProxy(entityMapping, oneToOneMapping, foreignKeyValue);

                    oneToOneMapping.PropertyInfo.SetValue(entity, referencedEntity, null);
                }
            }
        }
    }
}
