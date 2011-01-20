using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Framework.Data
{
    public class LazyLoadingInterceptor : IInterceptor
    {
        private readonly EntityMapping entityMapping;
        private readonly ISession session;
        private bool needsToBeInitialized = true;

        public LazyLoadingInterceptor(EntityMapping entityMapping, ISession session)
        {
            this.entityMapping = entityMapping;
            this.session = session;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");

            if (invocation.Method.Name.Equals("get_" + entityMapping.PrimaryKey.PropertyInfo.Name) ||
                invocation.Method.Name.Equals("set_" + entityMapping.PrimaryKey.PropertyInfo.Name))
            {
                invocation.Proceed();
                return;
            }

            if (needsToBeInitialized)
            {
                needsToBeInitialized = false;
                session.Refresh(invocation.TargetType, invocation.Proxy);
            }

            invocation.Proceed();
        }
    }
}
