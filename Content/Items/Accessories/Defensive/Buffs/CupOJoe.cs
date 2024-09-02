using ITD.Utilities.Placeholders;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Defensive.Buffs
{
    public class CupOJoe : ModItem
    {
        public override string Texture => Placeholder.PHBottle;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.White;

            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }
    }
}
