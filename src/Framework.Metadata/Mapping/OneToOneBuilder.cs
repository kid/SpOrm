namespace Framework.Metadata.Mapping
{
    using System.Reflection;
    using Framework.Metadata.Mapping.Providers;

    public class OneToOneBuilder<T> : IOneToOneMappingProvider
    {
        private MemberInfo memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneToOneBuilder&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public OneToOneBuilder(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }
    }
}
