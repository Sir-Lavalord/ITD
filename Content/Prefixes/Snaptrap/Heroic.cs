namespace ITD.Content.Prefixes.Snaptrap;

/// <summary>
/// Equivalent of Legendary for snaptraps.
/// </summary>
public class Heroic : SnaptrapPrefix
{
    public Heroic() : base(critBonus: 4, shootSpeedBonus: 0.1f, retractRateBonus: 0.1f, lengthBonus: 0.1f, damageBonus: 15)
    {

    }

    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 2.95f;
    }

    public override float RollChance(Item item)
    {
        return 0.1f;
    }
}

