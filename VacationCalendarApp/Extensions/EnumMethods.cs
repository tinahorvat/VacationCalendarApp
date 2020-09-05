using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VacationCalendarApp.Extensions
{
    public static class EnumMethods
    {
        public static string GetDescription(this Enum enumValue)
        {
            return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DescriptionAttribute>()?
                   .Description ?? string.Empty;
        }

        public static Dictionary<string, string> GetNameDescriptionPairs(Type T)
        {
            var keyValuePairs = new Dictionary<string, string>();

            // gets the Type that contains all the info required    
            // to manipulate this type    
            Type enumType = T; 

            // I will get all values and iterate through them    
            var enumValues = enumType.GetEnumValues();

            foreach (var value in enumValues)
            {
                // with our Type object we can get the information about    
                // the members of it    
                MemberInfo memberInfo =
                    enumType.GetMember(value.ToString()).First();

                // we can then attempt to retrieve the    
                // description attribute from the member info    
                var descriptionAttribute =
                    memberInfo.GetCustomAttribute<DescriptionAttribute>();

                // if we find the attribute we can access its values    
                if (descriptionAttribute != null)
                {
                    keyValuePairs.Add(value.ToString(),
                        descriptionAttribute.Description);
                }
                else
                {
                    keyValuePairs.Add(value.ToString(), value.ToString());
                }
            }

            return keyValuePairs;
        }
    }

    public static class EnumService<T>
    {
        private static readonly IEnumerable<string> EnumFieldNames;
        private static readonly IEnumerable<T> EnumValues;

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        static EnumService()
        {
            EnumFieldNames = Enum.GetNames(typeof(T));
            var query = from element in Enum.GetValues(typeof(T)).Cast<T>()
                        select element;
            EnumValues = query.ToList();
        }

        public static bool IsFieldName(string name)
        {
            return EnumFieldNames.Contains(name);
        }

        public static IEnumerable<string> GetFieldNames()
        {
            return EnumFieldNames.ToArray();
        }

        public static IEnumerable<T> GetValues()
        {
            return EnumValues.ToArray();
        }

        private static Dictionary<int, string> dictionary = null;
        public static Dictionary<int, string> ToDictionary()
        {
            VerifyDictionaryExists();
            return dictionary;
        }

        private static void VerifyDictionaryExists()
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<int, string>();
                var query = from element in EnumValues
                            select new
                            {
                                Key = Convert.ToInt32(element),
                                Value = Enum.GetName(typeof(T), element)
                            };

                foreach (var e in query)
                {
                    dictionary[e.Key] = e.Value;
                }
            }
        }

        private static Dictionary<int, string> descDictionary = null;
        public static Dictionary<int, string> GetDescriptionValuePairs()
        {            
            if (descDictionary == null)
            {
                descDictionary = new Dictionary<int, string>();
                var query = from element in EnumValues
                            select new
                            {
                                Key = Convert.ToInt32(element),
                                Value = (element as Enum).GetDescription()
                            };

                foreach (var e in query)
                {
                    descDictionary[e.Key] = e.Value;
                }
            }
            return descDictionary;
        }
    }
}
