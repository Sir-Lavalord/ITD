using System;
using System.Reflection;
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
        /// Get any value through reflection
        /// </summary>
        public static TValue Get<TMemberType, TValue>(string memberName, object instance = null, Type staticClass = null, BindingFlags? flags = null) where TMemberType : MemberInfo
        {
            bool isField = typeof(TMemberType) == typeof(FieldInfo);
            bool isProperty = typeof(TMemberType) == typeof(PropertyInfo);

            Type targetType = staticClass ?? instance?.GetType();

            if (isField)
            {
                FieldInfo field;
                object value;

                field = targetType.GetField(memberName, flags ?? DefaultLookup);
                value = field.GetValue(staticClass != null ? null : instance);

                return (TValue)value;
            }
            if (isProperty)
            {
                PropertyInfo property;
                object value;

                property = targetType.GetProperty(memberName, flags ?? DefaultLookup);
                value = property.GetValue(staticClass != null ? null : instance, null);

                return (TValue)value;
            }
            // If you do everything right this shouldn't show up at all
            Main.NewText($"[ITD]: Reflect access unsuccessful for {memberName}");
            return default;
        }
        public static void Set<TMemberType>(string memberName, object value, object instance = null, Type staticClass = null, BindingFlags? flags = null) where TMemberType : MemberInfo
        {
            bool isField = typeof(TMemberType) == typeof(FieldInfo);
            bool isProperty = typeof(TMemberType) == typeof(PropertyInfo);

            Type targetType = staticClass ?? instance?.GetType();

            if (isField)
            {
                FieldInfo field = targetType.GetField(memberName, flags ?? DefaultLookup);
                field.SetValue(staticClass != null ? null : instance, value);
                return;
            }
            if (isProperty)
            {
                PropertyInfo property = targetType.GetProperty(memberName, flags ?? DefaultLookup);
                property.SetValue(staticClass != null ? null : instance, value);
                return;
            }
        }
        public static object CallMethod(string methodName, object instance = null, Type staticClass = null, BindingFlags? flags = null, params object[] args)
        {
            bool hasParams = args.Length > 0;

            Type targetType = (staticClass ?? instance?.GetType()) ?? throw new ArgumentException("Can't reflect as no type or instance is provided");

            MethodInfo method = targetType.GetMethod(methodName, flags ?? DefaultLookup, null, hasParams ? Array.ConvertAll(args, obj => obj.GetType()) : Type.EmptyTypes, null);

            if (method == null)
            {
                // If you do everything right this shouldn't show up at all
                Main.NewText($"[ITD]: Reflect access unsuccessful for {methodName}");
                return null;
            }

            object result = method.Invoke(staticClass != null ? null : instance, args);

            if (method.ReturnType == typeof(void))
                return null;

            return result;
        }
    }
}
