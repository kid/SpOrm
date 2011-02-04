namespace Framework.Metadata.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Framework.Metadata.Mapping.Providers;

    public class ClassMap<T>
    {
        private IIdentityMappingProvider id;

        private readonly IList<IPropertyMappingProvider> properties = new List<IPropertyMappingProvider>();
        private readonly IList<ICollectionMappingProvider> collections = new List<ICollectionMappingProvider>();
        private readonly IList<IManyToOneMappingProvider> references = new List<IManyToOneMappingProvider>();
        private readonly IList<IOneToOneMappingProvider> oneToOnes = new List<IOneToOneMappingProvider>();

        protected IdentityPart Id(Expression<Func<T, object>> propertySelector)
        {
            var builder = new IdentityPart(propertySelector.GetMemberInfo());
            id = builder;
            return builder;
        }

        protected PropertyBuilder Map(Expression<Func<T, object>> propertySelector)
        {
            var builder = new PropertyBuilder(propertySelector.GetMemberInfo());
            properties.Add(builder);
            return builder;
        }

        protected ManyToOneBuilder<TOther> References<TOther>(Expression<Func<T, TOther>> propertySelector)
        {
            var builder = new ManyToOneBuilder<TOther>(propertySelector.GetMemberInfo());
            references.Add(builder);
            return builder;
        }

        protected OneToOneBuilder<TOther> HasOne<TOther>(Expression<Func<T, TOther>> propertySelector)
        {
            var builder = new OneToOneBuilder<TOther>(propertySelector.GetMemberInfo());
            oneToOnes.Add(builder);
            return builder;
        }

        protected OneToManyBuilder<TChild> HasMany<TChild>(Expression<Func<T, IEnumerable<TChild>>> propertySelector)
        {
            var builder = new OneToManyBuilder<TChild>(propertySelector.GetMemberInfo());
            collections.Add(builder);
            return builder;
        }
    }
}
