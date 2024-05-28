using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles
{
    public class Bluegrass : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/Bluegrass_Glow");
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.SnowBlock] = true;
            Main.tileMerge[Type][TileID.IceBlock] = true;
            Main.tileMerge[TileID.SnowBlock][Type] = true;
            Main.tileMerge[TileID.IceBlock][Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

            HitSound = SoundID.Item48;
            DustType = DustID.SnowBlock;

            AddMapEntry(new Color(211, 236, 241));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0f;
            g = 0.55f;
            b = 0.6f;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ItemID.SnowBlock);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile thisTile = Framing.GetTileSafely(i, j);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Vector2 offsets = -Main.screenPosition + zero;
            spriteBatch.Draw(glowmask.Value, new Vector2(i*16, j*16) + offsets, new Rectangle(thisTile.TileFrameX, thisTile.TileFrameY, 16, 16), Color.White);
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.rand.NextBool(250))
            {
                Helpers.GrowBluegrass(i - 1, j);
                Helpers.GrowBluegrass(i + 1, j);
                Helpers.GrowBluegrass(i, j - 1);
                Helpers.GrowBluegrass(i, j + 1);
            }
        }
    }
}