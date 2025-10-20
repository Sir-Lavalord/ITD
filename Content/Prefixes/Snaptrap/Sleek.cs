namespace ITD.Content.Prefixes.Snaptrap;

public class Sleek : SnaptrapPrefix
{
    public Sleek() : base(critBonus: -1, shootSpeedBonus: 0.16f, retractRateBonus: 0.16f)
    {

    }

    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 1.75f;
    }

    public override float RollChance(Item item)
    {
        return 1.47f;
    }
}

