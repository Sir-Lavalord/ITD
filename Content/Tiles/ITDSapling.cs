using ITD.Content.Dusts;
using ITD.Content.Tiles.BlueshroomGroves;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ITD.Content.Tiles
{
    public abstract class ITDSapling : ModTile
    {
        public int MinGrowHeight {  get; set; }
        public int MaxGrowHeight { get; set; }
        public int GrowsIntoTreeType { get; set; }
        /// <summary>
        /// Used as a consequent in UnifiedRandom.NextBool() to determine how fast a sapling will grow. Less means faster.
        /// </summary>
        public int GrowSlow { get; set; }
        public int FertilizerType { get; set; } = ProjectileID.Fertilizer;
        public Color MapColor { get; set; }

        /// <summary>
        /// Override to set DustType, MapColor, GrowSlow, GrowIntoTreeType, MinGrowHeight, MaxGrowHeight, and FertilizerType
        /// </summary>
        public virtual void SetStaticSaplingDefaults()
        {
            DustType = DustID.GrassBlades;
            MapColor = Color.White;
            GrowsIntoTreeType = TileType<BlueshroomTree>();
            GrowSlow = 20;
        }
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = [16, 18 ];
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorValidTiles = [TileType<Bluegrass>(), TileID.SnowBlock];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.DrawFlipHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.StyleMultiplier = 3;
            TileObjectData.addSubTile(1);
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(Color.Aquamarine, name);

            TileID.Sets.SwaysInWindBasic[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

            SetStaticSaplingDefaults();
            AdjTiles = [TileID.Saplings];
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override void RandomUpdate(int i, int j)
        {
            // A random chance to slow down growth
            if (WorldGen.genRand.NextBool(GrowSlow))
            {
                Grow(i, j);
            }
        }
        public void Grow(Point p) => Grow(p.X, p.Y);
        public void Grow(int i, int j)
        {
            bool growSuccess;

            growSuccess = ITDTree.Grow(i, j, GrowsIntoTreeType, MinGrowHeight, MaxGrowHeight, Type);
            // A flag to check if a player is near the sapling
            bool isPlayerNear = WorldGen.PlayerLOS(i, j);

            //If growing the tree was a sucess and the player is near, show growing effects
            if (growSuccess && isPlayerNear)
                ITDTree.TreeGrowFXCheck(i, j);
        }
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
        {
            if (i % 2 == 1)
                effects = SpriteEffects.FlipHorizontally;
        }
    }
}
