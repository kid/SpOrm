namespace Framework.Metadata.Mapping
{
    using System.Reflection;
    using Framework.Metadata.Mapping.Providers;

    public class IdentityPart : IIdentityMappingProvider
    {
        private MemberInfo memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityPart"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public IdentityPart(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }
    }
}
