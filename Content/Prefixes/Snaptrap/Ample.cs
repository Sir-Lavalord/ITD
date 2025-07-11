using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Prefixes
{
    public class Ample : SnaptrapPrefix
    {
        public Ample() : base(shootSpeedBonus: -0.12f, retractRateBonus: -0.12f, damageBonus: 12)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.9f;
        }

        public override float RollChance(Item item)
        {
            return 2.32f;
        }
    }
}

