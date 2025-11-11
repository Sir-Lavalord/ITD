using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles;

public abstract class ITDLantern : ModTile
{
    public virtual bool LavaDeath => true;
    public virtual Color? MapColor => null;
    public virtual bool ExtraBottomPixel => false;
    public virtual Vector3 GetLightColor(int i, int j) => Color.White.ToVector3();
    public override void SetStaticDefaults()
    {
        bool lavaDeath = LavaDeath;

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = lavaDeath;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.CoordinateHeights = [16, ExtraBottomPixel ? 18 : 16];
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorBottom = default;
        TileObjectData.newTile.LavaDeath = lavaDeath;
        TileObjectData.newTile.LavaPlacement = lavaDeath ? LiquidPlacement.NotAllowed : LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        AddMapEntry(MapColor ?? Color.LightGoldenrodYellow, Language.GetText("MapObject.Lantern"));
        AdjTiles = [TileID.Torches, TileID.HangingLanterns];

        SetStaticDefaultsSafe();
    }
    public virtual void SetStaticDefaultsSafe()
    {

    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].TileFrameX < 18)
        {
            Vector3 lightColor = GetLightColor(i, j);
            r = lightColor.X;
            g = lightColor.Y;
            b = lightColor.Z;
            return;
        }
        r = g = b = 0f;
    }
    public override void HitWire(int i, int j)
    {
        TileHelpers.CommonWiringLight(Type, i, j);
    }
}
