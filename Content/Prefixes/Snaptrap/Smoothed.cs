using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Prefixes
{
    public class Smoothed : SnaptrapPrefix
    {
        public Smoothed() : base(damageBonus: -15)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.25f;
        }

        public override float RollChance(Item item)
        {
            return 2.95f;
        }
    }
}

