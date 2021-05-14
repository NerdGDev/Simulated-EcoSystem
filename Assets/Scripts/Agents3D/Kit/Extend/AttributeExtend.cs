using System;
using System.Linq;

namespace Kit
{
    public static class AttributeExtend
    {
        /// <summary>Gets the attribute value from giving class</summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="valueSelector">The value selector.</param>
        /// <returns></returns>
        /// <example>
        /// string name = typeof(MyClass).GetAttributeValue((MyAttribute attr) => attr.name);
        /// string name = typeof(MyClass).GetAttributeValue<MyAttribute,string>((MyAttribute attr) => attr.name);
        /// </example>
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type,Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var attr = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            return (attr == null) ? default(TValue) : valueSelector(attr);
        }
    }
}
