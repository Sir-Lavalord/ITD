using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class AlloySword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Melee;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 33;
            Item.useAnimation = 20;
            Item.useStyle = 1;
            Item.knockBack = 2.1f;
            Item.value = 10000;
            Item.rare = 2;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
        }

        /*   public override void AddRecipes()
           {
               Recipe recipe = CreateRecipe();
               recipe.AddIngredient(ItemID.DirtBlock, 10);
               recipe.AddTile(TileID.WorkBenches);
               recipe.Register();
           }  */
    }
}