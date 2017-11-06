// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace TimekeeperDAL.Tools
{
    public static class Extensions
    {
        //Reminder: Exensions are:
        //public static [type] [MethodName](this [type2] [variableName])

        public static string GetTypeName(this object o) { return o.GetType().IgnoreProxy().Name; }

        //EF does some wierd stuff. When using EF Code First, it will create proxy classes at
        //runtime with some bizarre name ending in a string of letters and numbers used for lazy loading. 
        //Doesn't matter most of the time, but if you use nameof(entity.GetType()), you get the proxy class.
        public static Type IgnoreProxy(this Type entityType)
        {
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
                entityType = entityType.BaseType;
            return entityType;
        }

        public static string Pluralize(this string s)
        {
            return PluralizationService.CreateService(CultureInfo.CurrentCulture).Pluralize(s);
        }
    }
}
