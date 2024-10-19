using ITD.Content.Items.Placeable;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.ID;
using Terraria;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Skies;
using ITD.Systems;
using Terraria.GameContent;

namespace ITD.Content.Tiles.Misc
{
    public class LongBlackMold : ModTile
    {
        public override void SetStaticDefaults()
        {
            ITDSets.ToScrapeableMoss[Type] = ModContent.ItemType<BlackMoldItem>();
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;

            AddMapEntry(Color.Black);

            DustType = DustID.Ambient_DarkBrown;
            HitSound = SoundID.NPCHit9;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.StyleMultiplier = 4;
            TileObjectData.newTile.StyleWrapLimit = 3;

            int[] blackMold = [ModContent.TileType<BlackMold>()];
            TileObjectData.newTile.AnchorValidTiles = blackMold;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorLeft = AnchorData.Empty;
            TileObjectData.addAlternate(3);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newAlternate.AnchorTop = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
            TileObjectData.addAlternate(6);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newAlternate.AnchorTop = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorLeft = AnchorData.Empty;
            TileObjectData.addAlternate(9);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addTile(Type);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D tex = TextureAssets.Tile[Type].Value;
            Tile t = Framing.GetTileSafely(i, j);
            //TileObjectData data = TileObjectData.GetTileData(Framing.GetTileSafely(i, j));
            Vector2 offset = Vector2.Zero;
            switch (t.TileFrameY)
            {
                case 0:
                    offset.Y += 2;
                    break;
                case 18:
                    offset.Y -= 2;
                    break;
                case 36:
                    offset.X -= 2;
                    break;
                case 54:
                    offset.X += 2;
                    break;
                default:
                    break;
            }
            TileHelpers.DrawTileCommon(spriteBatch, i, j, tex, offset);
            return false;
        }
    }
}
