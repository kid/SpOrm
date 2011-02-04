namespace Framework.Metadata.Mapping
{
    using System.Reflection;
    using Framework.Metadata.Mapping.Providers;

    public class ManyToOneBuilder<T> : IManyToOneMappingProvider
    {
        private MemberInfo memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToOneBuilder&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public ManyToOneBuilder(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }
    }
}
