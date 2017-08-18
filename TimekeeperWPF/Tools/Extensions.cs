using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Tools
{
    public static class Extensions
    {
        public static string GetTypeName(this object o)
        {
            return o.GetType().IgnoreProxy().Name;
        }

        //EF does some wierd stuff. When using EF Code First, it will create proxy classes at
        //runtime with some bizarre name ending in a string of letters and numbers. Doesn't matter
        //most of the time, but if you use nameof(entity.GetType()), you get the proxy class.
        public static Type IgnoreProxy(this Type entityType)
        {
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.BaseType;
            }
            return entityType;
        }
    }
}
