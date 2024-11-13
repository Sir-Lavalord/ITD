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
        public static TValue Get<TType, TValue>(string memberName, object instance = null, Type staticClass = null, BindingFlags? flags = null) where TType : MemberInfo
        {
            bool isField = typeof(TType) == typeof(FieldInfo);
            bool isProperty = typeof(TType) == typeof(PropertyInfo);
            if (isField)
            {
                FieldInfo field;
                object value;
                if (staticClass != null)
                {
                    field = staticClass.GetField(memberName, flags ?? DefaultLookup);
                    value = field.GetValue(null);
                }
                else
                {
                    field = instance.GetType().GetField(memberName, flags ?? DefaultLookup);
                    value = field.GetValue(instance);
                }
                return (TValue)value;
            }
            if (isProperty)
            {
                PropertyInfo property;
                object value;
                if (staticClass != null)
                {
                    property = staticClass.GetProperty(memberName, flags ?? DefaultLookup);
                    value = property.GetValue(null);
                }
                else
                {
                    property = instance.GetType().GetProperty(memberName, flags ?? DefaultLookup);
                    value = property.GetValue(instance);
                }
                return (TValue)value;
            }
            // If you do everything right this shouldn't show up at all
            Main.NewText($"[ITD]: Reflect access unsuccessful for {memberName}");
            return default;
        }
    }
}
