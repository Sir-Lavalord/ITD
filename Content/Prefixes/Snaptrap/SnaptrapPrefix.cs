using ITD.Common.Prefixes;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Prefixes.Snaptrap
{
    public abstract class SnaptrapPrefix : ComplexPrefix
    {
        public static List<int> SnaptrapPrefixes = null;
        internal int critBonus = 0;
        internal int damageBonus = 0;
        internal float lengthBonus = 0f;
        internal float retractRateBonus = 0f;
        internal float shootSpeedBonus = 0f;
        public static LocalizedText CritBonusText { get; set; }
        public static LocalizedText DamageBonusText {  get; set; }
        public static LocalizedText LengthBonusText { get; set; }
        public static LocalizedText RetractRateBonusText { get; set; }
        public static LocalizedText ShootSpeedBonusText { get; set; }
        public sealed override PrefixCategory Category => PrefixCategory.Custom;
        // don't see constructors for modtypes often do you?
        // well, the way most things are initialized in order for tmod to load them into the game is often via Activator.CreateInstance<T> or similar,
        // which actually calls any public constructors with default values (i.e. an "uninitialized" object)
        // we can abuse this by actually defining a constructor for the modtype in order for stuff to be set on creation.
        // inheriting types will be able to inherit this constructor and set any values we want.
        public SnaptrapPrefix(int critBonus = 0, int damageBonus = 0, float lengthBonus = 0f, float shootSpeedBonus = 0f, float retractRateBonus = 0f)
        {
            this.critBonus = critBonus;
            this.damageBonus = damageBonus;
            this.lengthBonus = lengthBonus;
            this.shootSpeedBonus = shootSpeedBonus;
            this.retractRateBonus = retractRateBonus;
        }
        public sealed override void SetStaticDefaults()
        {
            if (SnaptrapPrefixes is null)
            {
                SnaptrapPrefixes = [];
                CritBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.CritBonusText");
                DamageBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.DamageBonusText");
                LengthBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.LengthBonusText");
                RetractRateBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.RetractRateBonusText");
                ShootSpeedBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.ShootSpeedBonusText");
            }

            SnaptrapPrefixes.Add(Type);
        }
        public sealed override void Unload()
        {
            SnaptrapPrefixes = null;
        }
        public sealed override void UpdateHeldPrefix(Item item, Player player)
        {
            player.GetSnaptrapPlayer().LengthIncrease += lengthBonus;
            player.GetSnaptrapPlayer().RetractMultiplier += retractRateBonus;
        }
        public sealed override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 1f + (damageBonus / 100f);
            shootSpeedMult = 1f + shootSpeedBonus;
            critBonus = this.critBonus;
        }
        public sealed override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            if (damageBonus != 0)
                yield return new TooltipLine(Mod, "PrefixDamage", DamageBonusText.Format(damageBonus))
                {
                    IsModifier = true,
                    IsModifierBad = damageBonus < 0
                };

            if (critBonus != 0)
                yield return new TooltipLine(Mod, "PrefixCritChance", CritBonusText.Format(critBonus))
                {
                    IsModifier = true,
                    IsModifierBad = critBonus < 0
                };

            if (shootSpeedBonus != 0f)
                yield return new TooltipLine(Mod, "PrefixSpeed", ShootSpeedBonusText.Format(shootSpeedBonus))
                {
                    IsModifier = true,
                    IsModifierBad = shootSpeedBonus < 0f
                };

            if (retractRateBonus != 0f)
                yield return new TooltipLine(Mod, "PrefixShootSpeed", RetractRateBonusText.Format(retractRateBonus * 100f))
                {
                    IsModifier = true,
                    IsModifierBad = retractRateBonus < 0f
                };

            if (lengthBonus != 0f)
                yield return new TooltipLine(Mod, "PrefixSize", LengthBonusText.Format(lengthBonus * 100f))
                {
                    IsModifier = true,
                    IsModifierBad = lengthBonus < 0f
                };
        }
    }
}
