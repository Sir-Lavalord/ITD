using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using ITD.Content.Tiles.BlueshroomGroves;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using Terraria.DataStructures;

namespace ITD.Content.Tiles
{
    public abstract class ITDTree : ModTile
    {
        // holy, this code sucks. if you have any sanity left please don't read this it will instantly suck it out of you
        public Color MapColor { get; set; }
        public int WoodType {  get; set; }
        /// <summary>
        /// Leave null for no acorn drops
        /// </summary>
        public int? DropAcorns { get; set; } = null;
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
        public static bool IsCenterRootWithSideRoots(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return IsCenterRootWithSideRoots(t);
        }
        public static bool IsCenterRootWithSideRoots(Tile t)
        {
            return (t.TileFrameX == 0 && t.TileFrameY >= 54) || (t.TileFrameX >= 36 && t.TileFrameX < 90 && t.TileFrameY >= 54) || (t.TileFrameX >= 126 && t.TileFrameY >= 54);
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
        /// Override to set dust type, map color, wood type, and acorn type.
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
            HitSound = SoundID.Dig;
            TileID.Sets.IsATreeTrunk[Type] = true;
            LocalizedText name = CreateMapEntryName();
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
        public virtual void PostDrawTreeTops(SpriteBatch spriteBatch, Vector2 position, Vector2 origin, Color color, Rectangle sourceRect)
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
        /// Grow a tree of a type at the given coordinates
        /// </summary>
        public static bool Grow(int i, int j, int type = 0, int minHeight = 5, int maxHeight = 10, int? saplingType = null)
        {
            if (type == 0)
            {
                type = ModContent.TileType<BlueshroomTree>();
            }
            int height = Main.rand.Next(minHeight, maxHeight + 1);
            if (Helpers.AptForTree(i, j, height, saplingType))
            {
                for (int k = 0; k < height; k++)
                {
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
            return false;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail) return;
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
            if (IsCenterRootWithSideRoots(i, j))
            {
                if (IsRightRoot(i + 1, j) && Helpers.TileType(i + 1, j, Type))
                {
                    WorldGen.KillTile(i + 1, j);
                }
                if (IsLeftRoot(i - 1, j) && Helpers.TileType(i - 1, j, Type))
                {
                    WorldGen.KillTile(i - 1, j);
                }
                if (IsCenterTrunk(i, j - 1) && Helpers.TileType(i, j - 1, Type))
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
                if (IsCenterTrunk(i, j - 1) && Helpers.TileType(i, j - 1, Type))
                {
                    WorldGen.KillTile(i, j - 1 );
                }
                if (IsCenterTrunk(i, j + 1) && Helpers.TileType(i, j + 1, Type))
                {
                    Tile t = Framing.GetTileSafely(i, j + 1);
                    if (IsCenterRootWithSideRoots(i, j + 1))
                    {
                        SideGrowth roots = SideGrowth.None;
                        if (IsRightRoot(i + 1, j + 1) && Helpers.TileType(i + 1, j + 1, Type))
                        {
                            roots = SideGrowth.Right;
                        }
                        if (IsLeftRoot(i - 1, j + 1) && Helpers.TileType(i - 1, j + 1, Type))
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
                    if ((IsRightBranch(i + 1, j + 1) || IsRightBranchNormal(i + 1, j + 1)) && Helpers.TileType(i + 1, j + 1, Type))
                    {
                        branches = SideGrowth.Right;
                    }
                    if ((IsLeftBranch(i - 1, j + 1) || IsLeftBranchNormal(i - 1, j + 1)) && Helpers.TileType(i - 1, j + 1, Type))
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
            if (IsLeftBranch(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new Rectangle(0, 42*frame, 40, 40);
                Vector2 position = Helpers.TileExtraPos(i, j, new Vector2(15f, 7f));
                Vector2 origin = new Vector2(39, GetBranches().Height / 2f / 3f);
                spriteBatch.Draw(GetBranches(), position, sourceRect, color, 0f, origin, 1f, SpriteEffects.None, 0f);
                PostDrawBranch(spriteBatch, position, origin, color, sourceRect, true);
                return false;
            }
            if (IsRightBranch(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new Rectangle(42, 42 * frame, 40, 40);
                Vector2 position = Helpers.TileExtraPos(i, j, new Vector2(42f, 7f));
                Vector2 origin = new Vector2(42, GetBranches().Height / 2f / 3f);
                spriteBatch.Draw(GetBranches(), position, sourceRect, color, 0f, origin, 1f, SpriteEffects.None, 0f);
                PostDrawBranch(spriteBatch, position, origin, color, sourceRect, false);
                return false;
            }
            if (IsTopTile(tile))
            {
                int frame = tile.TileFrameY / 18;
                Rectangle sourceRect = new Rectangle(82*frame, 0, 80, 80);
                Vector2 position = Helpers.TileExtraPos(i, j, new Vector2(9f, 0f));
                Vector2 origin = new Vector2(GetTreeTops().Width / 2f / 3f, GetTreeTops().Height - 2);
                spriteBatch.Draw(GetTreeTops(), position, sourceRect, color, 0f, origin, 1f, SpriteEffects.None, 0f);
                PostDrawTreeTops(spriteBatch, position, origin, color, sourceRect);
            }
            return true;
        }
    }
}
