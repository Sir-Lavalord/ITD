using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Tiles
{
    public abstract class ITDChandelier : ModTile
    {
        public virtual bool LavaDeath => true;
        public virtual Color? MapColor => null;
        public virtual bool ExtraBottomPixel => false;
        public virtual Vector3 GetLightColor(int i, int j) => Color.White.ToVector3();
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.MultiTileSway[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = default;
            TileObjectData.newTile.CoordinateHeights =
            [
                16,
                16,
                ExtraBottomPixel ? 18 : 16,
            ];
            TileObjectData.newTile.Origin = new Point16(1, 0);
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(MapColor ?? Color.LightGoldenrodYellow, Language.GetText("MapObject.Chandelier"));
            AdjTiles = [TileID.Chandeliers];

            SetStaticDefaultsSafe();
        }
        public virtual void SetStaticDefaultsSafe()
        {

        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (Main.tile[i, j].TileFrameX < 54)
            {
                Vector3 lightColor = GetLightColor(i, j);
                r = lightColor.X;
                g = lightColor.Y;
                b = lightColor.Z;
                return;
            }
            r = g = b = 0f;
        }
        public sealed override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (TileObjectData.IsTopLeft(tile))
            {
                // Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
            }

            // We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
            return false;
        }
    }
}
