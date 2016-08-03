using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace TeamFlash
{
    public static class NameValueCollectionExtensions
    {
        public static T MapTo<T>(this NameValueCollection nameValues) where T : new()
        {
            var allKeys = nameValues.AllKeys.Select(p => p.ToUpper()).ToList();
            var properties = GetAllProperties(typeof (T));
            var arguments = new T();

            foreach (var propertyInfo in properties)
            {
                if (!allKeys.Contains(propertyInfo.Name.ToUpper())) continue;

                var argValue = nameValues[propertyInfo.Name];
                propertyInfo.SetValue(arguments, Convert.ChangeType(argValue, propertyInfo.PropertyType), null);
            }

            return arguments;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(Type t)
        {
            if (t == null)
                return Enumerable.Empty<PropertyInfo>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            return t.GetProperties(flags);
        }
    }
}
