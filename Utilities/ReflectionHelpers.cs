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
        public static bool TryGetModConfigValue<T>(this Mod mod, string configInternalName, string fieldName, BindingFlags? flags, out T value)
        {
            if (!mod.TryFind(configInternalName, out ModConfig config))
            {
                value = default;
                return false;
            }
            // default is equivalent to DefaultLookup
            FieldInfo field = config.GetType().GetField(fieldName, flags ?? BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            value = (T)field.GetValue(config);
            return true;
        }
    }
}
