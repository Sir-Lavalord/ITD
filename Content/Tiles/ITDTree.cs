using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using ITD.Content.Tiles.BlueshroomGroves;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using Terraria.DataStructures;
using ITD.Systems;
using System.Reflection;
using Terraria.GameContent;
using System.Linq;

namespace ITD.Content.Tiles
{
    public struct TreeShakeSettings(int[] spawnNPC = null, int[] spawnGoldNPC = null, int[] spawnButterflies = null, int[] spawnGoldButterflies = null, int[] spawnFruit = null)
    {
        public int[] SpawnNPC = spawnNPC;
        public int[] SpawnGoldNPC = spawnGoldNPC;
        public int[] SpawnButterfiles = spawnButterflies;
        public int[] SpawnGoldButterflies = spawnGoldButterflies;
        public int[] SpawnFruit = spawnFruit;
        public static TreeShakeSettings Common => new();
    }
    public abstract class ITDTree : ModTile
    {
        // holy, this code sucks. if you have any sanity left please don't read this it will instantly suck it out of you
        public Color MapColor { get; set; }
        public int WoodType {  get; set; }
        /// <summary>
        /// Leave null for no acorn drops
        /// </summary>
        public int? DropAcorns { get; set; } = null;
        public virtual TreeShakeSettings TreeShakeSettings => TreeShakeSettings.Common;
        public static bool IsTopTile(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsTopTile(t);
        }
        public static bool IsTopTile(Tile t)
        {
            return t.TileFrameX == 180 && t.TileFrameY >= 0;
        }
        public static bool IsLeftBranch(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsLeftBranch(t);
        }
        public static bool IsLeftBranch(Tile t) => t.TileFrameX == 18 && t.TileFrameY <= 36;
        public static bool IsLeftBranchNormal(int i, int j) => IsLeftBranchNormal(Framing.GetTileSafely(i, j));
        public static bool IsLeftBranchNormal(Tile t) => t.TileFrameX == 198;
        public static bool IsRightBranch(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsRightBranch(t);
        }
        public static bool IsRightBranch(Tile t)
        {
            return t.TileFrameX == 90 && t.TileFrameY <= 36;
        }
        public static bool IsRightBranchNormal(int i, int j) => IsRightBranchNormal(Framing.GetTileSafely(i, j));
        public static bool IsRightBranchNormal(Tile t) => t.TileFrameX == 216;
        public static bool IsBranch(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsBranch(t);
        }
        public static bool IsBranch(Tile t)
        {
            return IsLeftBranch(t) || IsRightBranch(t);
        }
        public static bool IsAnyLeftBranch(int i, int j) => IsAnyLeftBranch(Framing.GetTileSafely(i, j));
        public static bool IsAnyLeftBranch(Tile t) => IsLeftBranch(t) || IsLeftBranchNormal(t);
        public static bool IsAnyRightBranch(int i, int j) => IsAnyRightBranch(Framing.GetTileSafely(i, j));
        public static bool IsAnyRightBranch(Tile t) => IsRightBranch(t) || IsRightBranchNormal(t);
        public static bool IsLeftRoot(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsLeftRoot(t);
        }
        public static bool IsLeftRoot(Tile t)
        {
            return t.TileFrameX == 18 && t.TileFrameY >= 54;
        }
        public static bool IsRightRoot(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsRightRoot(t);
        }
        public static bool IsRightRoot(Tile t)
        {
            return t.TileFrameX == 90 && t.TileFrameY >= 54;
        }
        public static bool IsRoot(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsRoot(t);
        }
        public static bool IsRoot(Tile t)
        {
            return IsLeftRoot(t) || IsRightRoot(t);
        }
        public static bool IsCenterRoot(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsCenterRoot(t);
        }
        public static bool IsCenterRoot(Tile t)
        {
            return (t.TileFrameX == 0 && t.TileFrameY >= 54) ||
                (t.TileFrameX >= 36 && t.TileFrameX < 90 && t.TileFrameY >= 54) ||
                (t.TileFrameX >= 108 && t.TileFrameY >= 54);
        }
        public static bool IsAnyRoot(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsAnyRoot(t);
        }
        public static bool IsAnyRoot(Tile t)
        {
            return IsCenterRoot(t) || IsRoot(t);
        }
        public static bool IsCenterTrunk(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsCenterTrunk(t);
        }
        public static bool IsCenterTrunk(Tile t)
        {
            return (t.TileFrameX == 0) || (t.TileFrameX >= 36 && t.TileFrameX < 90) || (t.TileFrameX >= 108);
        }
        private enum SideGrowth
        {
            Left,
            Right,
            Both,
            None
        }
        /// <summary>
        /// Override to set dust type, map color, wood type, acorn type, and tree shake settings.
        /// </summary>
        public virtual void SetStaticTreeDefaults()
        {
            DustType = DustID.WoodFurniture;
            MapColor = Color.White;
            WoodType = ItemID.Wood;
            DropAcorns = ItemID.Acorn;
        }
        public override void SetStaticDefaults()
        {
            SetStaticTreeDefaults();
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;
            TileID.Sets.IsATreeTrunk[Type] = true;
            TileID.Sets.IsShakeable[Type] = true;
            TileID.Sets.GetsDestroyedForMeteors[Type] = true;
            TileID.Sets.GetsCheckedForLeaves[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
            LocalizedText name = CreateMapEntryName();
            HitSound = SoundID.Dig;
            AddMapEntry(MapColor, name);
            RegisterItemDrop(WoodType);
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = (fail ? 1 : 3);
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            resetFrame = false;
            noBreak = true;
            return false;
        }
        /// <summary>
        /// Override to set treetop texture.
        /// </summary>
        /// <returns>Treetop Texture2D.</returns>
        public virtual Texture2D GetTreeTops()
        {
            return ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomTops").Value;
        }
        public virtual Texture2D GetBranches()
        {
            return ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomBranches").Value;
        }
        /// <summary>
        /// Draw stuff on top of the treetops.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="origin"></param>
        /// <param name="color"></param>
        /// <param name="sourceRect"></param>
        public virtual void PostDrawTreeTops(int i, int j, SpriteBatch spriteBatch, Rectangle sourceRect, Vector2 offset, Vector2 origin, Color color)
        {

        }
        public virtual void PostDrawBranch(SpriteBatch spriteBatch, Vector2 position, Vector2 origin, Color color, Rectangle sourceRect, bool isLeftBranch)
        {

        }
        private static void FrameToPoint(Tile t, short x, short y)
        {
            int verticalRandom = Main.rand.Next(3) * 18;
            t.TileFrameX = x;
            t.TileFrameY = (short)(y + verticalRandom);
        }
        /// <summary>
        /// Try to grow a tree of a type at the given coordinates
        /// </summary>
        public static bool Grow(int i, int j, int type = 0, int minHeight = 5, int maxHeight = 10, int? saplingType = null)
        {
            // default tree type to avoid trying to grow a dirt tree (though i'm curious abt what that would do)
            if (type == 0)
            {
                type = ModContent.TileType<BlueshroomTree>();
            }
            int height = Main.rand.Next(minHeight, maxHeight + 1);
            // if the ceiling's too low for the chosen height, keep trying until it's a good height. if you still can't place a tree in that case, just return false
            while (!TileHelpers.AptForTree(i, j, height, saplingType))
            {
                height--;
                if (height < minHeight)
                    return false;
            }
            // if this tree is grown from a sapling (which would be at (i, j)), remove the sapling before growing the tree to avoid sapling drops
            if (saplingType != null)
            {
                Framing.GetTileSafely(i, j).HasTile = false;
                int otherOffset = 1;
                // is this the bottom part or the top part of a sapling
                if (TileHelpers.SolidTile(i, j + 1))
                {
                    otherOffset = -1;
                }
                Framing.GetTileSafely(i, j + otherOffset).HasTile = false;
            }
            // actually grow the tree
            for (int k = 0; k < height; k++)
            {
                // new tiles and tiles that are framed need to be synced
                bool mp = Main.netMode != NetmodeID.SinglePlayer;

                WorldGen.PlaceTile(i, j - k, type);
                Tile tile = Framing.GetTileSafely(i, j - k);
                FrameToPoint(tile, 0, 0);
                if (mp) NetMessage.SendTileSquare(-1, i, j - k, TileChangeType.None);
                if (k == 0)
                {
                    SideGrowth rootGrowth = SideGrowth.Both;
                    // Placing roots
                    if (Main.rand.NextBool())
                        rootGrowth = Main.rand.NextBool() ? SideGrowth.Left : SideGrowth.Right;
                    switch (rootGrowth)
                    {
                        case SideGrowth.Left:
                            WorldGen.PlaceTile(i - 1, j, type);
                            Tile leftRoot = Framing.GetTileSafely(i - 1, j);
                            FrameToPoint(leftRoot, 18, 54);
                            FrameToPoint(tile, 36, 54);
                            if (mp) NetMessage.SendTileSquare(-1, i - 1, j, TileChangeType.None);
                            break;
                        case SideGrowth.Right:
                            WorldGen.PlaceTile(i + 1, j, type);
                            Tile rightRoot = Framing.GetTileSafely(i + 1, j);
                            FrameToPoint(rightRoot, 90, 54);
                            FrameToPoint(tile, 72, 54);
                            if (mp) NetMessage.SendTileSquare(-1, i + 1, j, TileChangeType.None);
                            break;
                        default:
                            WorldGen.PlaceTile(i - 1, j, type);
                            WorldGen.PlaceTile(i + 1, j, type);
                            Tile leftRoot1 = Framing.GetTileSafely(i - 1, j);
                            Tile rightRoot1 = Framing.GetTileSafely(i + 1, j);
                            FrameToPoint(leftRoot1, 18, 54);
                            FrameToPoint(rightRoot1, 90, 54);
                            FrameToPoint(tile, 54, 54);
                            if (mp)
                            {
                                NetMessage.SendTileSquare(-1, i - 1, j, TileChangeType.None);
                                NetMessage.SendTileSquare(-1, i + 1, j, TileChangeType.None);
                            }
                            break;
                    }
                }
                // If not root nor treetop, try to grow a branch
                else if (k < height - 1)
                {
                    SideGrowth branchGrowth = SideGrowth.None;
                    if (Main.rand.NextBool())
                        branchGrowth = (SideGrowth)Main.rand.Next(4);
                    short leftBranchFrame = Main.rand.NextBool() ? (short)18 : (short)198;
                    short rightBranchFrame = Main.rand.NextBool() ? (short)90 : (short)216;
                    switch (branchGrowth)
                    {
                        case SideGrowth.Left:
                            WorldGen.PlaceTile(i - 1, j - k, type);
                            Tile leftBranch = Framing.GetTileSafely(i - 1, j - k);
                            FrameToPoint(leftBranch, leftBranchFrame, 0);
                            FrameToPoint(tile, 36, 0);
                            if (mp) NetMessage.SendTileSquare(-1, i - 1, j - k, TileChangeType.None);
                            break;
                        case SideGrowth.Right:
                            WorldGen.PlaceTile(i + 1, j - k, type);
                            Tile rightBranch = Framing.GetTileSafely(i + 1, j - k);
                            FrameToPoint(rightBranch, rightBranchFrame, 0);
                            FrameToPoint(tile, 72, 0);
                            if (mp) NetMessage.SendTileSquare(-1, i + 1, j - k, TileChangeType.None);
                            break;
                        case SideGrowth.Both:
                            WorldGen.PlaceTile(i - 1, j - k, type);
                            WorldGen.PlaceTile(i + 1, j - k, type);
                            Tile leftBranch1 = Framing.GetTileSafely(i - 1, j - k);
                            Tile rightBranch1 = Framing.GetTileSafely(i + 1, j - k);
                            FrameToPoint(leftBranch1, leftBranchFrame, 0);
                            FrameToPoint(rightBranch1, rightBranchFrame, 0);
                            FrameToPoint(tile, 54, 0);
                            if (mp)
                            {
                                NetMessage.SendTileSquare(-1, i - 1, j - k, TileChangeType.None);
                                NetMessage.SendTileSquare(-1, i + 1, j - k, TileChangeType.None);
                            }
                            break;
                        default:
                            break;
                    }
                }
                // If tree top
                else
                {
                    FrameToPoint(tile, 180, 0);
                    if (mp) NetMessage.SendTileSquare(-1, i, j - k, TileChangeType.None);
                }
            }
            return true;
        }
        /// <summary>
        /// Literally just WorldGen.TreeGrowFXCheck but with adjustments to fit ITDTree
        /// </summary>
        public static void TreeGrowFXCheck(int x, int y)
        {
            int treeHeight = 1;

            for (int num = -1; num > -100; num--)
            {
                Tile tile = Main.tile[x, y + num];
                if (!tile.HasTile || !TileID.Sets.GetsCheckedForLeaves[tile.TileType])
                    break;
                treeHeight++;
            }

            for (int i = 1; i < 5; i++)
            {
                Tile tile2 = Main.tile[x, y + i];
                if (tile2.HasTile && TileID.Sets.GetsCheckedForLeaves[tile2.TileType])
                {
                    treeHeight++;
                    continue;
                }
                break;
            }

            if (treeHeight > 0)
            {
                int gore = ITDSets.LeafGrowFX[Framing.GetTileSafely(x, y).TileType];
                //Main.NewText(gore + " heehoo");

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SpecialFX, -1, -1, null, 1, x, y, treeHeight, gore);

                if (Main.netMode == NetmodeID.SinglePlayer)
                    WorldGen.TreeGrowFX(x, y, treeHeight, gore);
            }
        }
        private static Point GetTreeBottom(int i, int j)
        {
            if (IsCenterRoot(i, j))
                return new Point(i, j);
            if (IsLeftRoot(i, j) || IsAnyLeftBranch(i, j))
            {
                return GetTreeBottom(i + 1, j);
            }
            if (IsRightRoot(i, j) || IsAnyRightBranch(i, j))
            {
                return GetTreeBottom(i - 1, j);
            }
            if (IsCenterTrunk(i, j))
            {
                while (!IsCenterRoot(i, j))
                    j++;
                return new Point(i, j);
            }
            return new Point(i, j);
        }
        private static bool TryGetTreeTop(int i, int j, out Point top)
        {
            if (IsTopTile(i, j))
            {
                top = new(i, j);
                return true;
            }
            if (IsLeftRoot(i, j) || IsAnyLeftBranch(i, j))
            {
                return TryGetTreeTop(i + 1, j, out top);
            }
            if (IsRightRoot(i, j) || IsRightBranch(i, j) || IsRightBranchNormal(i, j))
            {
                return TryGetTreeTop(i - 1, j, out top);
            }
            if (IsCenterTrunk(i, j))
            {
                while (!IsTopTile(i, j))
                {
                    j--;
                    if (!TileID.Sets.IsShakeable[Framing.GetTileSafely(i, j).TileType])
                    {
                        top = new(i, j);
                        return false;
                    }
                }
                top = new(i, j);
                return true;
            }
            top = new(i, j);
            return false;
        }
        public void ShakeTree(int i, int j)
        {
            // adapted from confection rebaked
            // hello kibtenbun

            FieldInfo numTreeShakesR = typeof(WorldGen).GetField("numTreeShakes", BindingFlags.Static | BindingFlags.NonPublic);
            int numTreeShakes = (int)numTreeShakesR.GetValue(null);
            int maxTreeShakes = ReflectionHelpers.Get<FieldInfo, int>("maxTreeShakes", staticClass: typeof(WorldGen), flags: BindingFlags.Static | BindingFlags.NonPublic);
            int[] treeShakeX = ReflectionHelpers.Get<FieldInfo, int[]>("treeShakeX", staticClass: typeof(WorldGen), flags: BindingFlags.Static | BindingFlags.NonPublic);
            int[] treeShakeY = ReflectionHelpers.Get<FieldInfo, int[]>("treeShakeY", staticClass: typeof(WorldGen), flags: BindingFlags.Static | BindingFlags.NonPublic);
            if (numTreeShakes == maxTreeShakes)
                return;
            Point bottom = GetTreeBottom(i, j);
            // check if this tree has been shaken recently (in cache), if it is don't do anything
            for (int k = 0; k < numTreeShakes; k++)
            {
                if (treeShakeX[k] == bottom.X && treeShakeY[k] == bottom.Y)
                    return;
            }
            treeShakeX[numTreeShakes] = bottom.X;
            treeShakeY[numTreeShakes] = bottom.Y;
            numTreeShakesR.SetValue(null, ++numTreeShakes);
            if (!TryGetTreeTop(i, j, out bottom) || Collision.SolidTiles(bottom.X - 2, bottom.X + 2, bottom.Y - 2, bottom.Y + 2))
                return;
            // spawn bomb in fortheworthy
            if (Main.getGoodWorld && WorldGen.genRand.NextBool(17))
            {
                Projectile.NewProjectile(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, Main.rand.Next(-100, 101) * 0.002f, 0f, ProjectileID.Bomb, 0, 0f, Main.myPlayer, 16f, 16f);
            }
            // spawn acorns
            else if (WorldGen.genRand.NextBool(7))
            {
                if (DropAcorns != null)
                    Item.NewItem(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, 16, 16, (int)DropAcorns, WorldGen.genRand.Next(1, 3));
            }
            // spawn rotten egg
            else if (WorldGen.genRand.NextBool(35) && Main.halloween)
            {
                Item.NewItem(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, 16, 16, ItemID.RottenEgg, WorldGen.genRand.Next(1, 3));
            }
            // spawn wood
            else if (WorldGen.genRand.NextBool(12))
            {
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, 16, 16, WoodType, WorldGen.genRand.Next(1, 4));
            }
            // spawn money
            else if (WorldGen.genRand.NextBool(20))
            {
                int type = ItemID.CopperCoin;
                int num2 = WorldGen.genRand.Next(50, 100);
                if (WorldGen.genRand.NextBool(30))
                {
                    type = ItemID.GoldCoin;
                    num2 = 1;
                    if (WorldGen.genRand.NextBool(5))
                    {
                        num2++;
                    }
                    if (WorldGen.genRand.NextBool(10))
                    {
                        num2++;
                    }
                }
                else if (WorldGen.genRand.NextBool(10))
                {
                    type = ItemID.SilverCoin;
                    num2 = WorldGen.genRand.Next(1, 21);
                    if (WorldGen.genRand.NextBool(3))
                    {
                        num2 += WorldGen.genRand.Next(1, 21);
                    }
                    if (WorldGen.genRand.NextBool(4))
                    {
                        num2 += WorldGen.genRand.Next(1, 21);
                    }
                }
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, 16, 16, type, num2);
            }
            // spawn npc
            else if (WorldGen.genRand.NextBool(15))
            {
                int type2 = -1;
                if (TreeShakeSettings.SpawnNPC.Length > 0)
                    type2 = Main.rand.NextFromList(TreeShakeSettings.SpawnNPC);
                if (Player.GetClosestRollLuck(bottom.X, bottom.Y, NPC.goldCritterChance) == 0f)
                {
                    if (TreeShakeSettings.SpawnGoldNPC.Length > 0)
                        type2 = Main.rand.NextFromList(TreeShakeSettings.SpawnGoldNPC);;
                }
                if (type2 > -1)
                    NPC.NewNPC(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, type2);
            }
            else if (WorldGen.genRand.NextBool(50) && !Main.dayTime)
            {
                int type3 = Main.rand.NextFromList(NPCID.FairyCritterPink, NPCID.FairyCritterGreen, NPCID.FairyCritterBlue);
                if (Main.tenthAnniversaryWorld && !Main.rand.NextBool(4))
                {
                    type3 = NPCID.FairyCritterPink;
                }
                NPC.NewNPC(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, type3);
            }
            // spawn butterflies
            else if (WorldGen.genRand.NextBool(20) && !Main.raining && !NPC.TooWindyForButterflies && Main.dayTime)
            {
                int type5 = -1;
                if (TreeShakeSettings.SpawnButterfiles.Length > 0)
                    type5 = Main.rand.NextFromList(TreeShakeSettings.SpawnButterfiles);
                if (Player.GetClosestRollLuck(bottom.X, bottom.Y, NPC.goldCritterChance) == 0f)
                {
                    if (TreeShakeSettings.SpawnGoldButterflies.Length > 0)
                        type5 = Main.rand.NextFromList(TreeShakeSettings.SpawnGoldButterflies);
                }
                if (type5 > -1)
                    NPC.NewNPC(new EntitySource_ShakeTree(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, type5);
            }
            // spawn fruit
            else if (WorldGen.genRand.NextBool(12))
            {
                int secondaryItemStack = -1;
                if (TreeShakeSettings.SpawnFruit.Length > 0)
                    secondaryItemStack = Main.rand.NextFromList(TreeShakeSettings.SpawnFruit);
                if (secondaryItemStack > -1)
                    Item.NewItem(WorldGen.GetItemSource_FromTreeShake(bottom.X, bottom.Y), bottom.X * 16, bottom.Y * 16, 16, 16, secondaryItemStack);
            }
            // leaf gores
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SpecialFX, -1, -1, null, 1, bottom.X, bottom.Y, 1f, ITDSets.LeafGrowFX[Type]);
            }
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                WorldGen.TreeGrowFX(bottom.X, bottom.Y, 1, ITDSets.LeafGrowFX[Type], hitTree: true);
            }
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    ShakeTree(i, j);
                }
                return;
            }
            bool onlyLeftRoot = IsLeftRoot(i, j) && !IsRightRoot(i + 2, j);
            bool onlyRightRoot = IsRightRoot(i, j) && !IsLeftRoot(i - 2, j);
            bool bothRoots = (IsLeftRoot(i, j) || IsRightRoot(i, j)) && (IsLeftRoot(i - 2, j) || IsRightRoot(i + 2, j));
            if (onlyLeftRoot)
            {
                Tile trunk = Framing.GetTileSafely(i + 1, j);
                FrameToPoint(trunk, 0, 54);
            }
            if (onlyRightRoot)
            {
                Tile trunk = Framing.GetTileSafely(i - 1, j);
                FrameToPoint(trunk, 0, 54);
            }
            if (bothRoots)
            {
                int offset = 0;
                if (IsLeftRoot(i, j))
                {
                    offset = 1;
                }
                if (IsRightRoot(i, j))
                {
                    offset = -1;
                }
                Tile trunk = Framing.GetTileSafely(i + offset, j);
                if (offset == 1)
                {
                    FrameToPoint(trunk, 72, 54);
                }
                else
                {
                    FrameToPoint(trunk, 36, 54);
                }
            }
            if (IsCenterRoot(i, j))
            {
                if (IsRightRoot(i + 1, j) && TileHelpers.TileType(i + 1, j, Type))
                {
                    WorldGen.KillTile(i + 1, j);
                }
                if (IsLeftRoot(i - 1, j) && TileHelpers.TileType(i - 1, j, Type))
                {
                    WorldGen.KillTile(i - 1, j);
                }
                if (IsCenterTrunk(i, j - 1) && TileHelpers.TileType(i, j - 1, Type))
                {
                    WorldGen.KillTile(i, j - 1);
                }
            }
            if (IsTopTile(i, j) && DropAcorns != null)
            {
                Item.NewItem(new EntitySource_ShakeTree(i, j), new Point(i, j).ToWorldCoordinates(), (int)DropAcorns, WorldGen.genRand.Next(1, 3));
            }
            if (IsCenterTrunk(i, j))
            {
                if (IsLeftBranch(i - 1, j) || IsLeftBranchNormal(i - 1, j))
                {
                    WorldGen.KillTile(i - 1, j);
                }
                if (IsRightBranch(i + 1, j) || IsRightBranchNormal(i + 1, j))
                {
                    WorldGen.KillTile(i + 1, j);
                }
                if (IsCenterTrunk(i, j - 1) && TileHelpers.TileType(i, j - 1, Type))
                {
                    WorldGen.KillTile(i, j - 1 );
                }
                if (IsCenterTrunk(i, j + 1) && TileHelpers.TileType(i, j + 1, Type))
                {
                    Tile t = Framing.GetTileSafely(i, j + 1);
                    if (IsCenterRoot(i, j + 1))
                    {
                        SideGrowth roots = SideGrowth.None;
                        if (IsRightRoot(i + 1, j + 1) && TileHelpers.TileType(i + 1, j + 1, Type))
                        {
                            roots = SideGrowth.Right;
                        }
                        if (IsLeftRoot(i - 1, j + 1) && TileHelpers.TileType(i - 1, j + 1, Type))
                        {
                            roots = roots == SideGrowth.Right ? SideGrowth.Both : SideGrowth.Left;
                        }
                        switch (roots)
                        {
                            case SideGrowth.Left:
                                FrameToPoint(t, 126, 54);
                                break;
                            case SideGrowth.Right:
                                FrameToPoint(t, 144, 54);
                                break;
                            case SideGrowth.Both:
                                FrameToPoint(t, 162, 54);
                                break;
                            default:
                                FrameToPoint(t, 108, 54);
                                break;
                        }
                        return;
                    }
                    SideGrowth branches = SideGrowth.None;
                    if ((IsRightBranch(i + 1, j + 1) || IsRightBranchNormal(i + 1, j + 1)) && TileHelpers.TileType(i + 1, j + 1, Type))
                    {
                        branches = SideGrowth.Right;
                    }
                    if ((IsLeftBranch(i - 1, j + 1) || IsLeftBranchNormal(i - 1, j + 1)) && TileHelpers.TileType(i - 1, j + 1, Type))
                    {
                        branches = branches == SideGrowth.Right ? SideGrowth.Both : SideGrowth.Left;
                    }
                    switch (branches)
                    {
                        case SideGrowth.Left:
                            FrameToPoint(t, 126, 0);
                            break;
                        case SideGrowth.Right:
                            FrameToPoint(t, 144, 0);
                            break;
                        case SideGrowth.Both:
                            FrameToPoint(t, 162, 0);
                            break;
                        default:
                            FrameToPoint(t, 108, 0);
                            break;
                    }
                }
            }
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Color color = Lighting.GetColor(i, j);
            Tile tile = Framing.GetTileSafely(i, j);
            Texture2D tex = TextureAssets.Tile[Type].Value;
            if (IsLeftBranch(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new(0, 42*frame, 40, 40);
                Vector2 position = TileHelpers.TileExtraPos(i, j, new Vector2(15f, 7f));
                Vector2 origin = new(39, GetBranches().Height / 2f / 3f);
                spriteBatch.Draw(GetBranches(), position, sourceRect, color, 0f, origin, 1f, SpriteEffects.None, 0f);
                PostDrawBranch(spriteBatch, position, origin, color, sourceRect, true);
                return false;
            }
            if (IsRightBranch(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new(42, 42 * frame, 40, 40);
                Vector2 position = TileHelpers.TileExtraPos(i, j, new Vector2(42f, 7f));
                Vector2 origin = new(42, GetBranches().Height / 2f / 3f);
                //WeatherSystem.DrawTreeSway(i, j, spriteBatch, GetBranches(), sourceRect, new Vector2(42f, 7f), origin);
                spriteBatch.Draw(GetBranches(), position, sourceRect, color, 0f, origin, 1f, SpriteEffects.None, 0f);
                PostDrawBranch(spriteBatch, position, origin, color, sourceRect, false);
                return false;
            }
            if (IsTopTile(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new(82*frame, 0, 80, 80);
                Vector2 origin = new(GetTreeTops().Width / 2f / 3f + 4, GetTreeTops().Height - 2);
                Vector2 offset = new(13f, 0f);
                WeatherSystem.DrawTreeSway(i, j, spriteBatch, GetTreeTops(), sourceRect, offset, origin);
                PostDrawTreeTops(i, j, spriteBatch, sourceRect, offset, origin, color);
            }
            // unlike regular tree spritesheets, ITDTree doesn't have extra pixels at the bottom of the root parts for it to appear as if it's grounded.
            // we will artificially extend the appearance of roots so they appear grounded
            if (IsAnyRoot(tile))
            {
                // this is a one (well, two) pixel slice of the root sprite.
                Rectangle tileRect = new(tile.TileFrameX, tile.TileFrameY + 14, 16, 2);
                TileHelpers.DrawTileCommon(spriteBatch, i, j, tex, new Vector2(0f, 16f), tileRect);
            }
            return true;
        }
    }
}
