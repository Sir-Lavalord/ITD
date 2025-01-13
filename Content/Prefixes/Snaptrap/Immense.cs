using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Immense : SnaptrapPrefix
    {
        public Immense() : base(shootSpeedBonus: -0.1f, damageBonus: 15)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.25f;
        }

        public override float RollChance(Item item)
        {
            return 1.97f;
        }
    }
}

