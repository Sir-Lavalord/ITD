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
using ITD.Content.Items.Materials;
using System.Collections.Generic;
using ITD.Content.Items.Placeable;
using System.Linq;
using ITD.Systems;
using Terraria.GameContent;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    // This example shows how to have a tile that is cut by weapons, like vines and grass.
    // This example also shows how to spawn a projectile on death like Beehive and Boulder trap.
    public class BluegrassBlades : ModTile
    {
        private readonly Asset<Texture2D> glowmask = Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BluegrassBlades_Glow");
        private static readonly int[] stylesThatDropBlueshrooms = [2];
        public override void SetStaticDefaults()
        {
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
            TileObjectData.newTile.AnchorValidTiles = [ TileType<Bluegrass>() ];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            //TileObjectData.newTile.StyleMultiplier = 3;
            TileObjectData.addTile(Type);

            TileID.Sets.SwaysInWindBasic[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.IgnoredInHouseScore[Type] = true;
            TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

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

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            WeatherSystem.DrawGrassSway(spriteBatch, TextureAssets.Tile[Type].Value, i, j, Lighting.GetColor(i, j));
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            WeatherSystem.DrawGrassSway(spriteBatch, glowmask.Value, i, j, Color.White * BlueshroomTree.opac);
        }
        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (Main.rand.NextBool(8))
            {
                yield return new Item(ItemType<BluegrassSeeds>());
            }
            int style = tile.TileFrameX / 18;
            if (stylesThatDropBlueshrooms.Contains(style))
            {
                yield return new Item(ItemType<Blueshroom>());
            }
        }
    }
}