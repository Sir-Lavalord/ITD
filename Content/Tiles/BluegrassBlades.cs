using ITD.Content.Dusts;
using ITD.Content.Items;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;

namespace ITD.Content.Tiles
{
    // This example shows how to have a tile that is cut by weapons, like vines and grass.
    // This example also shows how to spawn a projectile on death like Beehive and Boulder trap.
    public class BluegrassBlades : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = Request<Texture2D>("ITD/Content/Tiles/BluegrassBlades_Glow");
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            //TileObjectData.newTile.CoordinatePadding = 2;
            //TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.AnchorValidTiles = new[] { TileType<Bluegrass>() };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            //TileObjectData.newTile.StyleMultiplier = 3;
            TileObjectData.addTile(Type);

            TileID.Sets.SwaysInWindBasic[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.IgnoredInHouseScore[Type] = true;
            TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
            int[] stylesThatDropBlueshrooms = new int[] { 2 };
            RegisterItemDrop(ItemType<Blueshroom>(), stylesThatDropBlueshrooms);

            HitSound = SoundID.Grass;
            DustType = DustType<BluegrassBladesDust>();
        }
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 0)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType<BlueshroomSporesDust>());
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            /* doesn't work aaa
            Tile thisTile = Framing.GetTileSafely(i, j);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Vector2 offsets = -Main.screenPosition + zero;
            spriteBatch.Draw(glowmask.Value, new Vector2(i * 16, j * 16) + offsets, new Rectangle(thisTile.TileFrameX, thisTile.TileFrameY, 16, 16), Color.White);
            */
        }
    }
}