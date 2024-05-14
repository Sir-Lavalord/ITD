using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class RhodiumBroadsword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
        }
            public override void AddRecipes()
           {
               Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<RhodiumBar>(), 11);
            recipe.AddTile(TileID.Anvils);
               recipe.Register();
           } 
    }
}