using ITD.Common.Prefixes;
using ITD.Utilities;
using System.Collections.Generic;
using Terraria.Localization;

namespace ITD.Content.Prefixes.Snaptrap;

public abstract class SnaptrapPrefix : ComplexPrefix
{
    public static List<int> SnaptrapPrefixes = null;
    internal int critBonus = 0;
    internal int damageBonus = 0;
    internal float lengthBonus = 0f;
    internal float retractRateBonus = 0f;
    internal float shootSpeedBonus = 0f;
    public static LocalizedText LengthBonusText { get; set; }
    public static LocalizedText RetractRateBonusText { get; set; }
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
        _ = 0;
    }
    public sealed override void SetStaticDefaults()
    {
        if (SnaptrapPrefixes is null)
        {
            SnaptrapPrefixes = [];
            LengthBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.LengthBonusText");
            RetractRateBonusText = Mod.GetLocalization($"{LocalizationCategory}.{nameof(SnaptrapPrefix)}.RetractRateBonusText");
        }

        SnaptrapPrefixes.Add(Type);
    }
    public sealed override void Unload()
    {
        SnaptrapPrefixes = null;
    }
    public sealed override void UpdateHeldPrefix(Item item, Player player)
    {
        // use flat here for flat addition (just straight up addition)
        // multiplication and addition to the actual modifier is in percentages iirc
        // so modifier += 1f; would be 100% more.
        // and modifier *= 2f; would double the damage before addition.
        player.GetSnaptrapPlayer().LengthModifier += lengthBonus;
        player.GetSnaptrapPlayer().RetractVelocityModifier += retractRateBonus;
    }
    public sealed override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
    {
        damageMult = 1f + (damageBonus / 100f);
        shootSpeedMult = 1f + shootSpeedBonus;
        critBonus = this.critBonus;
    }
    public sealed override IEnumerable<TooltipLine> GetTooltipLines(Item item)
    {
        if (retractRateBonus != 0f)
            yield return new TooltipLine(Mod, "PrefixRetract", RetractRateBonusText.Format((retractRateBonus > 0f ? "+" : string.Empty) + (retractRateBonus * 100f)))
            {
                IsModifier = true,
                IsModifierBad = retractRateBonus < 0f
            };

        if (lengthBonus != 0f)
            yield return new TooltipLine(Mod, "PrefixSize", LengthBonusText.Format((lengthBonus > 0f ? "+" : string.Empty) + (lengthBonus * 100f)))
            {
                IsModifier = true,
                IsModifierBad = lengthBonus < 0f
            };
    }
}
