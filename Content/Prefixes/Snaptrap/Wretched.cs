using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Wretched : SnaptrapPrefix
    {
        public Wretched() : base(shootSpeedBonus: -0.04f, lengthBonus: -0.08f, damageBonus: -8)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.2f;
        }

        public override float RollChance(Item item)
        {
            return 3f;
        }
    }
}

