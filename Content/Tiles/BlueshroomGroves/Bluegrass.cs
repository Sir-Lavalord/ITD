using ITD.Content.Dusts;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class Bluegrass : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/Bluegrass_Glow");
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

            AddMapEntry(new Color(0, 129, 227));
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
            TileHelpers.DrawSlopedGlowMask(i, j, glowmask.Value, Color.White, Vector2.Zero);
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail)
            {
                Framing.GetTileSafely(i, j).TileType = TileID.SnowBlock;
            }
        }
        public override void RandomUpdate(int i, int j)
        {
            if (Main.rand.NextBool(4))
            {
                Helpers.SpreadGrass(i, j, Type, TileID.SnowBlock);
            }
        }
    }
}