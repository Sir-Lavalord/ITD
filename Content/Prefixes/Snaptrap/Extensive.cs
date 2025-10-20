namespace ITD.Content.Prefixes.Snaptrap;

public class Extensive : SnaptrapPrefix
{
    public Extensive() : base(lengthBonus: 0.16f)
    {

    }

    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 1.8f;
    }

    public override float RollChance(Item item)
    {
        return 2.85f;
    }
}

