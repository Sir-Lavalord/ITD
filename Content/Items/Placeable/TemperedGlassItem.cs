using ITD.Content.Items.Materials;
using ITD.Content.Tiles.Misc;

namespace ITD.Content.Items.Placeable;

public class TemperedGlassItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<TemperedGlass>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient(ModContent.ItemType<EmberlionMandible>(), 1)
            .AddIngredient(ItemID.Glass, 4)
            .AddTile(TileID.Furnaces)
            .Register();
    }
}
