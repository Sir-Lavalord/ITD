using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Prefixes
{
    public class Feral : SnaptrapPrefix
    {
        public Feral() : base(damageBonus: 10)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.5f;
        }

        public override float RollChance(Item item)
        {
            return 1.72f;
        }
    }
}

