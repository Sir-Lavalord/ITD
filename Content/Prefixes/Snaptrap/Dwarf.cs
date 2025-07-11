using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Prefixes
{
    public class Dwarf : SnaptrapPrefix
    {
        public Dwarf() : base(shootSpeedBonus: 0.08f, retractRateBonus: 0.08f, damageBonus: -12)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.8f;
        }

        public override float RollChance(Item item)
        {
            return 2.42f;
        }
    }
}

