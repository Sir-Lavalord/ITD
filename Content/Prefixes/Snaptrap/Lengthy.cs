using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Lengthy : SnaptrapPrefix
    {
        public Lengthy() : base(lengthBonus: 0.10f)
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

