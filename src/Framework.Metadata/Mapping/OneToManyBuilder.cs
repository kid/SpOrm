namespace Framework.Metadata.Mapping
{
    using System.Reflection;
    using Framework.Metadata.Mapping.Providers;

    public class OneToManyBuilder<T> : ICollectionMappingProvider
    {
        private MemberInfo memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneToManyBuilder&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public OneToManyBuilder(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }
    }
}
