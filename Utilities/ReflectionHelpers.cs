using ITD.Content.Items.Accessories.Defensive.Defense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ITD.Utilities
{
    public static class ReflectionHelpers
    {
        public const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        public static bool TryGetModConfigValue<T>(this Mod mod, string configInternalName, string fieldName, BindingFlags? flags, out T value)
        {
            if (!mod.TryFind(configInternalName, out ModConfig config))
            {
                value = default;
                return false;
            }
            FieldInfo field = config.GetType().GetField(fieldName, flags ?? DefaultLookup);
            value = (T)field.GetValue(config);
            return true;
        }
        /// <summary>
        /// Get any private value through reflection
        /// </summary>
        public static object Get<T>(string memberName, object instance = null, Type staticClass = null, BindingFlags? flags = null) where T : MemberInfo
        {
            bool isField = typeof(T) == typeof(FieldInfo);
            bool isProperty = typeof(T) == typeof(PropertyInfo);
            if (isField)
            {
                FieldInfo field;
                object value;
                if (staticClass != null)
                {
                    field = staticClass.GetField(memberName, flags ?? DefaultLookup);
                    value = field.GetValue(staticClass);
                }
                else
                {
                    field = instance.GetType().GetField(memberName, flags ?? DefaultLookup);
                    value = field.GetValue(instance);
                }
                return value;
            }
            if (isProperty)
            {
                PropertyInfo property;
                object value;
                if (staticClass != null)
                {
                    property = staticClass.GetProperty(memberName, flags ?? DefaultLookup);
                    value = property.GetValue(staticClass);
                }
                else
                {
                    property = instance.GetType().GetProperty(memberName, flags ?? DefaultLookup);
                    value = property.GetValue(instance);
                }
                return value;
            }
            return null;
        }
    }
}
