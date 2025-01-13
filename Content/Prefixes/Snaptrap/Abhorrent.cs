using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Abhorrent : SnaptrapPrefix
    {
        public Abhorrent() : base(lengthBonus: -0.1f)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.5f;
        }

        public override float RollChance(Item item)
        {
            return 2.72f;
        }
    }
}

