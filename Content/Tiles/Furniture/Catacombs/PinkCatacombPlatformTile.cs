using ITD.Content.Dusts;
using ITD.Utilities;
using Terraria.ObjectData;

namespace ITD.Content.Tiles.Furniture.Catacombs;

//the example mod.com
public class PinkCatacombPlatformTile : ModTile
{
    private Asset<Texture2D> glowmask;
    public override void SetStaticDefaults()
    {
        glowmask = Mod.Assets.Request<Texture2D>("Content/Tiles/Furniture/Catacombs/PinkCatacombPlatformTile_Glow");
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.Platforms[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
        AddMapEntry(new Color(200, 200, 200));

        DustType = ModContent.DustType<GreenCatacombDust>();
        AdjTiles = [TileID.Platforms];
        VanillaFallbackOnModDeletion = TileID.Platforms;

        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 27;
        TileObjectData.newTile.StyleWrapLimit = 27;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        TileHelpers.DrawSlopedGlowMask(i, j, glowmask.Value, Color.White, Vector2.Zero);
    }
    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}