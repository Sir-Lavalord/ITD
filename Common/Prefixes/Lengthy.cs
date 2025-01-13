using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Common.Prefixes
{
    public abstract class Lengthy : ComplexPrefix
    {
        public override void UpdateHeldPrefix(Item item, Player player)
        {
            player.GetSnaptrapPlayer().LengthIncrease += 10f;
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.5f;
        }

        public override float RollChance(Item item)
        {
            return 1f;
        }

        public override bool CanRoll(Item item)
        {
            return true;
        }

        public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            int statCrit = 0;
            int statSpeed = 0;
            int statRetract = 0;
            int statLength = 10;
            int statDamage = 0;

            yield return new TooltipLine(Mod, "PrefixDamage", statDamage + "%" + " damage")
            {
                IsModifier = true,
            };

            yield return new TooltipLine(Mod, "PrefixCritChance", statCrit + "%" + " crit chance")
            {
                IsModifier = true,
            };

            yield return new TooltipLine(Mod, "PrefixSpeed", statSpeed + "%" + " speed")
            {
                IsModifier = true,
            };

            yield return new TooltipLine(Mod, "PrefixShootSpeed", statRetract + "%" + " retract rate")
            {
                IsModifier = true,
            };

            yield return new TooltipLine(Mod, "PrefixSize", statLength + "%" + " snaptrap length")
            {
                IsModifier = true,
            };
        }

        public static LocalizedText PowerTooltip { get; private set; }

        public LocalizedText AdditionalTooltip => this.GetLocalization(nameof(AdditionalTooltip));

        public override void SetStaticDefaults()
        {
            PowerTooltip = Mod.GetLocalization($"{LocalizationCategory}.{nameof(PowerTooltip)}");
            _ = AdditionalTooltip;
        }
    }
}

       