using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Barbed : SnaptrapPrefix
    {
        public Barbed() : base(damageBonus: 15, shootSpeedBonus: 0.04f)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.95f;
        }

        public override float RollChance(Item item)
        {
            return 1.32f;
        }
    }
}

