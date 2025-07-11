using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Prefixes
{
    public class Acute : SnaptrapPrefix
    {
        public Acute() : base(critBonus: 2, damageBonus: 8)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.35f;
        }

        public override float RollChance(Item item)
        {
            return 1.87f;
        }
    }
}

