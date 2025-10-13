using ITD.Content.Items.Placeable.Furniture.Catacombs;

namespace ITD.Content.Tiles.Furniture.Catacombs;

public class CatacombCarvedEmblemTile : ITDPainting
{
    private readonly int Blue = ModContent.ItemType<BlueCatacombCarvedEmblem>();
    private readonly int Green = ModContent.ItemType<GreenCatacombCarvedEmblem>();
    private readonly int Pink = ModContent.ItemType<PinkCatacombCarvedEmblem>();
    public override void SetStaticPaintingDefaults()
    {
        DustType = DustID.Shadowflame;
        MapColor = new Color(30, 63, 96);
        PaintingItem = [Blue, Green, Pink];
        PaintingSize = [2, 2];
    }

}
