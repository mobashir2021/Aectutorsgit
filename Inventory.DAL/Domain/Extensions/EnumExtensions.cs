using System;
using System.ComponentModel;

namespace AECMIS.DAL.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription<TEnum>(this TEnum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            if (fi != null)
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                    return attributes[0].Description;
            }

            return value.ToString();
        }

        public static T ToEnum<T>(this int enumVal)
        {
            return (T)Enum.Parse(typeof(T), enumVal.ToString());
        }
    }
}
