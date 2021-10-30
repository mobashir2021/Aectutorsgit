using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AECMIS.MVC.Helpers
{
    public static class EnumHelper
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }


        public static T ToEnum<T>(this string value, T defaultValue)
        {
            if (string.IsNullOrEmpty(value) || value == "undefined")
            {
                return defaultValue;
            }
           // T result;
            //Enum.TryParse(value, true, out result);
            //  ? result : defaultValue;

            return (T)Enum.Parse(typeof(T), value, true);

            //return result;
        }

        public static T ToEnum<T>(this int? value, T defaultValue)
        {
            Type type = typeof(T);
            

            if (value == null)
            {
                return defaultValue;
            }

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingNullableType = Nullable.GetUnderlyingType(type);
                return (T)Enum.Parse(underlyingNullableType, Enum.GetName(underlyingNullableType, value));
            }

            return  Enum.GetName(typeof(T), value).ToEnum<T>();            
        }

    }
}