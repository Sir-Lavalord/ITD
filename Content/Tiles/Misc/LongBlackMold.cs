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

            // ok, i'm going to leave this here but the problem is that the alternates for left, right, and down placements won't place at all.
            // if an Item.createTile is set to this, the correct alternate shows up according to its anchor, but it can't be placed
            // help

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.StyleMultiplier = 4;
            TileObjectData.newTile.StyleWrapLimit = 3;

            int[] blackMold = [ModContent.TileType<BlackMold>()];
            TileObjectData.newTile.AnchorValidTiles = blackMold;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addAlternate(3);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addAlternate(6);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addAlternate(9);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addTile(Type);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
