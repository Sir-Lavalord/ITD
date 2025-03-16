using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Terraria.DataStructures;
using Terraria.Utilities;
using ITD.Content.Prefixes.Snaptrap;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public abstract class ITDSnaptrapItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.GetSnaptrapPlayer().ShootSnaptrap();
        //not all of them (Vocal Zero) use the same tooltip line
        /*        public override void ModifyTooltips(List<TooltipLine> tooltips)
                {
                    float pulseAmount = Main.mouseTextColor / 255f;
                    Color textColor = Color.LightPink * pulseAmount;
                    var line = tooltips.First(x => x.Name == "Tooltip1");
                    string coloredText = string.Format(line.Text, textColor.Hex3());
                    line.Text = coloredText;
                }*/
        public sealed override bool MeleePrefix() => false;
        public sealed override int ChoosePrefix(UnifiedRandom rand)
        {
            WeightedRandom<int> random = new(rand);

            foreach (int snaptrapPrefix in SnaptrapPrefix.SnaptrapPrefixes)
            {
                double weight = PrefixLoader.GetPrefix(snaptrapPrefix).RollChance(Item);

                random.Add(snaptrapPrefix, weight);
            }
            return random.Get();
        }
    }
}