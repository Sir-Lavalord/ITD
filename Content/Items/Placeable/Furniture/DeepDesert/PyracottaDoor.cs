namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaDoor : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.GetInstance<Tiles.Furniture.DeepDesert.PyracottaDoor>().ClosedType);
        Item.width = 14;
        Item.height = 28;
    }
}
