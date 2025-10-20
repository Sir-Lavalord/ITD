namespace ITD.Content.Prefixes.Snaptrap;

public class Tapered : SnaptrapPrefix
{
    public Tapered() : base(critBonus: 2, shootSpeedBonus: 0.08f, damageBonus: 15)
    {

    }

    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 4.25f;
    }

    public override float RollChance(Item item)
    {
        return 1.02f;
    }
}

