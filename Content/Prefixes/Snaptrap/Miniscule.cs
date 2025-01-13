using ITD.Content.Prefixes.Snaptrap;
using Terraria;

namespace ITD.Content.Prefixes
{
    public class Miniscule : SnaptrapPrefix
    {
        public Miniscule() : base(shootSpeedBonus: 0.16f, retractRateBonus: 0.16f, damageBonus: -20)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.6f;
        }

        public override float RollChance(Item item)
        {
            return 1.62f;
        }
    }
}

