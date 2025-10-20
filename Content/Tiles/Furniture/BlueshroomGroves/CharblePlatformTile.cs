using ITD.Utilities;

namespace ITD.Content.Tiles.Furniture.BlueshroomGroves;

public class CharblePlatformTile : ModTile
{
    public override void SetStaticDefaults()
    {
        this.DefaultToPlatform();
        DustType = DustID.Marble;
    }
}
