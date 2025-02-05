using ITD.Content.Items.Placeable;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Dyes
{
    public class CosmicDye : ModItem
    {
        public override void Load()
        {
        }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;

            if (Main.dedServ)
                return;
            GameShaders.Armor.BindShader(Type, ITD.ITDArmorShaders["CosmicDye"]);
        }
        public override void SetDefaults()
        {
            int temp = Item.dye;
            Item.CloneDefaults(ItemID.BlackDye);
            Item.dye = temp;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.BlackDye, 1)
                .AddIngredient(ModContent.ItemType<StarlitOre>(), 4)
                .AddTile(TileID.DyeVat)
                .Register();
        }
    }
}
