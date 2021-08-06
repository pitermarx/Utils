using System;
using System.Collections.Generic;
using System.Linq;

namespace pitermarx.Utils
{
    public static class EnumUtils
    {
        public static T ParseEnum<T>(string stringValue) where T : struct
        {
            return (T)Enum.Parse(typeof(T), stringValue, true);
        }

        public static IEnumerable<T> ToList<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}