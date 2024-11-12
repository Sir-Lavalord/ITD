using System.Collections.Generic;
using Terraria.ModLoader;

namespace ITD.Common.Rarities
{
    public class RarityModifierSystem : ModSystem // from better expert rarity mod
    {
        // [public static properties and fields]

        public static readonly IReadOnlyList<RarityModifier> Modifiers;

        // [private static properties and fields]

        private static readonly List<RarityModifier> modifierInstances;
        private static readonly Dictionary<int, RarityModifier> modifiersByRarityType;

        // [static constructors]

        static RarityModifierSystem()
        {
            Modifiers = (modifierInstances = new()).AsReadOnly();
            modifiersByRarityType = new();
        }

        // [public static methods]

        public static void AddModifier(RarityModifier modifier)
        {
            if (modifierInstances.Contains(modifier)) return;

            modifierInstances.Add(modifier);
        }

        public static bool TryGetModifier(int rarity, out RarityModifier modifier)
            => modifiersByRarityType.TryGetValue(rarity, out modifier);

        // [public methods]

        public override void PostSetupContent()
        {
            foreach (var modifier in modifierInstances)
            {
                if (modifiersByRarityType.ContainsKey(modifier.RarityType))
                    throw new System.Exception("...");

                modifiersByRarityType.Add(modifier.RarityType, modifier);
            }
        }

        public override void Unload()
        {
            modifierInstances.Clear();
            modifiersByRarityType.Clear();
        }
    }
}