using ITD.Utilities;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class BaldsnapStatueTile : ModTile
{
    private Asset<Texture2D> glowmask;
    public override void SetStaticDefaults()
    {
        glowmask = ModContent.Request<Texture2D>(Texture + "_Glow");
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;

        TileObjectData.newTile.Origin = new Point16(0, 2);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

        TileObjectData.newTile.CoordinateWidth = 18;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;

        TileObjectData.newTile.AnchorInvalidTiles = [
            TileID.MagicalIceBlock,
            TileID.Boulder,
            TileID.BouncyBoulder,
            TileID.LifeCrystalBoulder,
            TileID.RollingCactus
        ];

        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(120, 72, 72), Language.GetText("MapObject.Statue"));
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        TileHelpers.DrawTileCommon(spriteBatch, i, j, glowmask.Value, overrideColor: Color.White);
    }
}
