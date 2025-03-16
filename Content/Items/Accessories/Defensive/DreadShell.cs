using ITD.Players;
using ITD.Content.Projectiles;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Defensive
{
    public class DreadShell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.value = Item.sellPrice(50000);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            modPlayer.blockChance += 0.1f;
            modPlayer.dreadBlock = true;
        }
    }
}