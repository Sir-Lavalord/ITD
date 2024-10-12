using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace ITD.Utilities
{
    public struct TileFramingData(Tile thisTile, Tile up, Tile down, Tile left, Tile right, Tile upLeft, Tile upRight, Tile downLeft, Tile downRight)
    {
        public Tile ThisTile = thisTile;
        public Tile Up = up;
        public Tile Down = down;
        public Tile Left = left;
        public Tile Right = right;
        public Tile UpLeft = upLeft;
        public Tile UpRight = upRight;
        public Tile DownLeft = downLeft;
        public Tile DownRight = downRight;
        public static TileFramingData Get(int i, int j)
        {
            static Tile GetTile(int i, int j) => Framing.GetTileSafely(i, j);

            Tile thisT = GetTile(i, j);
            Tile upT = GetTile(i, j - 1);
            Tile downT = GetTile(i, j + 1);
            Tile leftT = GetTile(i - 1, j);
            Tile rightT = GetTile(i + 1, j);
            Tile upLeftT = GetTile(i - 1, j - 1);
            Tile upRightT = GetTile(i + 1, j - 1);
            Tile downLeftT = GetTile(i - 1, j + 1);
            Tile downRightT = GetTile(i + 1, j + 1);

            return new TileFramingData(thisT, upT, downT, leftT, rightT, upLeftT, upRightT, downLeftT, downRightT);
        }
    }
    public static class TileHelpers
    {
        public static bool TileType(int i, int j, int t) => Framing.GetTileSafely(i, j).HasTile && Framing.GetTileSafely(i, j).TileType == t;
        public static bool TileType(Tile tile, int t) => tile.HasTile && tile.TileType == t;
        public static bool SolidTile(int i, int j) => Framing.GetTileSafely(i, j).HasTile && Main.tileSolid[Framing.GetTileSafely(i, j).TileType];
        public static bool SolidTopTile(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (Main.tileSolidTop[Framing.GetTileSafely(i, j).TileType] || Main.tileSolid[Framing.GetTileSafely(i, j).TileType]);
        public static bool EdgeTileCross(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (!Framing.GetTileSafely(i + 1, j).HasTile || !Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile);
        public static bool EdgeTileX(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (!Framing.GetTileSafely(i + 1, j + 1).HasTile || !Framing.GetTileSafely(i + 1, j - 1).HasTile || !Framing.GetTileSafely(i - 1, j + 1).HasTile || !Framing.GetTileSafely(i - 1, j - 1).HasTile);
        public static bool EdgeTile(int i, int j) => EdgeTileCross(i, j) || EdgeTileX(i, j);
        public static Vector2 CommonTileOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        public static Vector2 TileExtraPos(int i, int j, Vector2 extraOffset = default) => new Vector2(i, j) * 16 - Main.screenPosition + extraOffset + CommonTileOffset;
        public static bool AptForTree(int i, int j, int height, int? saplingType = null)
        {
            Rectangle rect = new(i - 2, j - height, 5, height);
            // check if tile it's trying to grow on is empty
            if (Framing.GetTileSafely(i, j).HasTile && saplingType == null)
            {
                return false;
            }
            // check for solid ground
            for (int m = rect.Left + 1; m < rect.Right - 1; m++)
            {
                if (!Framing.GetTileSafely(m, rect.Bottom + 1).HasTile)
                {
                    return false;
                }
            }
            // check if space is completely empty
            for (int k = rect.Left; k < rect.Right; k++)
            {
                for (int l = rect.Top; l < rect.Bottom; l++)
                {
                    //Dust.QuickBox(new Vector2(k, l)*16f, new Vector2(k, l)*16f + new Vector2(16, 16), 8, Color.White, null);
                    if (Framing.GetTileSafely(k, l).HasTile)
                    {
                        if (saplingType != null && Framing.GetTileSafely(k, l).TileType == (int)saplingType)
                            return true;
                        return false;
                    }
                }
            }
            return true;
        }
        public static Point? GetTreeTopPosition(int i, int j)
        {
            Tile thisTile = Framing.GetTileSafely(i, j);
            bool isStump = thisTile.TileFrameX == 0 && thisTile.TileFrameY >= 198 || thisTile.TileFrameX == 132 || thisTile.TileFrameX == 110 && thisTile.TileFrameY <= 44 || thisTile.TileFrameX == 154;
            if (isStump)
                return null;
            if (!TileType(i, j - 1, TileID.Trees))
            {
                return new Point(i, j);
            }
            else
            {
                return GetTreeTopPosition(i, j - 1);
            }
        }
        /// <summary>
        /// code from the verdant mod
        /// </summary>
        public static void DrawSlopedGlowMask(int i, int j, Texture2D texture, Color drawColor, Vector2 positionOffset, Rectangle? frameOverride = null)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;

            if (frameOverride is not null)
            {
                frameX = frameOverride.Value.X;
                frameY = frameOverride.Value.Y;
            }

            Point frameSize = frameOverride is not null ? frameOverride.Value.Size().ToPoint() : new Point(16, 16);
            int width = frameSize.X;
            int height = frameSize.Y;

            Vector2 location = new(i * 16, j * 16);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Vector2 offsets = -Main.screenPosition + zero + positionOffset;
            Vector2 drawCoordinates = location + offsets;

            if (tile.Slope == 0 && !tile.IsHalfBlock || Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType]) //second one should be for platforms
                Main.spriteBatch.Draw(texture, drawCoordinates, new Rectangle(frameX, frameY, width, height), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else if (tile.IsHalfBlock)
                Main.spriteBatch.Draw(texture, new Vector2(drawCoordinates.X, drawCoordinates.Y + 8), new Rectangle(frameX, frameY, width, 8), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else
            {
                var slope = tile.Slope;
                Rectangle frame;
                Vector2 drawPos;

                if (slope == SlopeType.SlopeDownLeft || slope == SlopeType.SlopeDownRight)
                {
                    int length;
                    int height2;

                    for (int a = 0; a < 8; ++a)
                    {
                        if (slope == SlopeType.SlopeDownRight)
                        {
                            length = 16 - a * 2 - 2;
                            height2 = 14 - a * 2;
                        }
                        else
                        {
                            length = a * 2;
                            height2 = 14 - length;
                        }

                        frame = new Rectangle(frameX + length, frameY, 2, height2);
                        drawPos = new Vector2(i * 16 + length, j * 16 + a * 2) + offsets;
                        Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                    }

                    frame = new Rectangle(frameX, frameY + 14, width, 2);
                    drawPos = new Vector2(i * 16, j * 16 + 14) + offsets;
                    Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }
                else
                {
                    int length;
                    int height2;

                    for (int a = 0; a < 8; ++a)
                    {
                        if (slope == SlopeType.SlopeUpLeft)
                        {
                            length = a * 2;
                            height2 = 16 - length;
                        }
                        else
                        {
                            length = 16 - a * 2 - 2;
                            height2 = 16 - a * 2;
                        }

                        frame = new Rectangle(frameX + length, frameY + 16 - height2, 2, height2);
                        drawPos = new Vector2(i * 16 + length, j * 16) + offsets;
                        Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                    }

                    drawPos = new Vector2(i * 16, j * 16) + offsets;
                    frame = new Rectangle(frameX, frameY, 16, 2);
                    Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }
            }
        }
        /// <summary>
        /// Attempts to recreate vanilla tile framing behaviour. I'm using this for pegmatite as I need pegmatite and diorite to merge properly and not look ugly.
        /// </summary>
        public static void VanillaTileFraming(int i, int j)
        {
            TileFramingData data = TileFramingData.Get(i, j);
            int t = data.ThisTile.TileType;
            int r = Main.rand.Next(3);
            bool upHas = TileType(data.Up, t);
            bool downHas = TileType(data.Down, t);
            bool leftHas = TileType(data.Left, t);
            bool rightHas = TileType(data.Right, t);
            bool upLeftHas = TileType(data.UpLeft, t);
            bool upRightHas = TileType(data.UpRight, t);
            bool downLeftHas = TileType(data.DownLeft, t);
            bool downRightHas = TileType(data.DownRight, t);
            void FrameToHori(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = (short)(point.X + r * 18);
                tile.TileFrameY = point.Y;
            }
            void FrameToVert(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = point.X;
                tile.TileFrameY = (short)(point.Y + r * 18);
            }
            Point16 noSurroundingHori = new(162, 54);
            Point16 allSurroundingHori = new(18, 18);

            Point16 allSurroundingProtrudeUpHori = new(108, 18);
            Point16 allSurroundingProtrudeDownHori = new(108, 36);
            Point16 allSurroundingProtrudeLeftVert = new(180, 0);
            Point16 allSurroundingProtrudeRightVert = new(198, 0);

            Point16 allSurroundingNotLeftVert = new(0, 0);
            Point16 allSurroundingNotRightVert = new(72, 0);
            Point16 allSurroundingNotUpHori = new(18, 0);
            Point16 allSurroundingNotDownHori = new(18, 36);

            Point16 bothSurroundingUpDownVert = new(90, 0);
            Point16 bothSurroundingLeftRightHori = new(108, 72);

            Point16[] bothSurroundingUpLeft = [new(18, 72), new(54, 72), new(90, 72)];
            Point16[] bothSurroundingUpRight = [new(0, 72), new(36, 72), new(72, 72)];
            Point16[] bothSurroundingDownLeft = [new(18, 54), new(54, 54), new(90, 54)];
            Point16[] bothSurroundingDownRight = [new(0, 54), new(36, 54), new(72, 54)];

            Point16 onlySurroundingDownHori = new(108, 0);
            Point16 onlySurroundingUpHori = new(108, 54);
            Point16 onlySurroundingLeftVert = new(216, 0);
            Point16 onlySurroundingRightVert = new(162, 0);

            // If there are no surrounding tiles of the same type in the four cardinal directions
            if (!upHas && !rightHas && !leftHas && !downHas)
            {
                FrameToHori(data.ThisTile, noSurroundingHori);
                return;
            }
            // If there are surrounding tiles of the same type in the four cardinal directions
            // This needs extra checks for when tiles are "protruding" vs completely surrounded
            if (upHas && rightHas && leftHas && downHas)
            {
                // If protruding up
                if (!upLeftHas && !upRightHas)
                {
                    FrameToHori(data.ThisTile, allSurroundingProtrudeUpHori);
                    return;
                }
                // If protruding down
                if (!downLeftHas && !downRightHas)
                {
                    FrameToHori(data.ThisTile, allSurroundingProtrudeDownHori);
                    return;
                }
                // If protruding left
                if (!upLeftHas && !downLeftHas)
                {
                    FrameToVert(data.ThisTile, allSurroundingProtrudeLeftVert);
                    return;
                }
                // If protruding right
                if (!upRightHas && !downRightHas)
                {
                    FrameToVert(data.ThisTile, allSurroundingProtrudeRightVert);
                    return;
                }
                // If none (completely or almost completely surrounded)
                FrameToHori(data.ThisTile, allSurroundingHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the top
            if (leftHas && rightHas && downHas && !upHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotUpHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the bottom
            if (leftHas && rightHas && upHas && !downHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotDownHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the left
            if (rightHas && upHas && downHas && !leftHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotLeftVert);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the right
            if (leftHas && upHas && downHas && !rightHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotRightVert);
                return;
            }
            // If there are only tiles of the same type above and below
            if (!rightHas && !leftHas && upHas && downHas)
            {
                FrameToVert(data.ThisTile, bothSurroundingUpDownVert);
                return;
            }
            // If there are only tiles of the same type to the left and to the right
            if (!upHas && !downHas && leftHas && rightHas)
            {
                FrameToHori(data.ThisTile, bothSurroundingLeftRightHori);
                return;
            }
            // If there are only tiles of the same type above and to the left
            if (!downHas && !rightHas && upHas && leftHas)
            {
                data.ThisTile.TileFrameX = bothSurroundingUpLeft[r].X;
                data.ThisTile.TileFrameY = bothSurroundingUpLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type above and to the right
            if (!downHas && !leftHas && upHas && rightHas)
            {
                data.ThisTile.TileFrameX = bothSurroundingUpRight[r].X;
                data.ThisTile.TileFrameY = bothSurroundingUpRight[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the left
            if (!upHas && !rightHas && downHas && leftHas)
            {
                data.ThisTile.TileFrameX = bothSurroundingDownLeft[r].X;
                data.ThisTile.TileFrameY = bothSurroundingDownLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the right
            if (!upHas && !leftHas && downHas && rightHas)
            {
                data.ThisTile.TileFrameX = bothSurroundingDownRight[r].X;
                data.ThisTile.TileFrameY = bothSurroundingDownRight[r].Y;
                return;
            }
            // If there's only a surrounding tile above
            if (!downHas && !rightHas && !leftHas && upHas)
            {
                FrameToHori(data.ThisTile, onlySurroundingUpHori);
                return;
            }
            // If there's only a surrounding tile below
            if (!upHas && !rightHas && !leftHas && downHas)
            {
                FrameToHori(data.ThisTile, onlySurroundingDownHori);
                return;
            }
            // If there's only a surrounding tile to the left
            if (!upHas && !downHas && !rightHas && leftHas)
            {
                FrameToVert(data.ThisTile, onlySurroundingLeftVert);
                return;
            }
            // If there's only a surrounding tile to the right
            if (!upHas && !downHas && !leftHas && rightHas)
            {
                FrameToVert(data.ThisTile, onlySurroundingRightVert);
                return;
            }
        }
        public static void VanillaTileMergeWithOtherTopRight(ref Tile tileToCheck, int otherType, Tile up, Tile down, Tile left, Tile right, int yOffset)
        {
            int t = tileToCheck.TileType;
            bool upHas = TileType(up, t);
            bool downHas = TileType(down, t);
            bool leftHas = TileType(left, t);
            bool rightHas = TileType(right, t);
            bool upHasOther = TileType(up, otherType);
            bool downHasOther = TileType(down, otherType);
            bool leftHasOther = TileType(left, otherType);
            bool rightHasOther = TileType(right, otherType);
            //
            Point16 allSurroundingNotUpAndDownIsOther = new(234, 0);
            Point16 allSurroundingNotDownAndUpIsOther = new(234, 18);
            Point16 allSurroundingNotLeftAndRightIsOther = new(234, 36);
            Point16 allSurroundingNotRightAndLeftIsOther = new(234, 54);
            void FrameTo(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = (short)(point.X + r * 18);
                tile.TileFrameY = (short)(point.Y + yOffset);
            }
            if (!upHas && !upHasOther && leftHas && rightHas && downHasOther)
            {
                FrameTo(tileToCheck, allSurroundingNotUpAndDownIsOther);
                return;
            }
            if (!downHas && !downHasOther && leftHas && rightHas && upHasOther)
            {
                FrameTo(tileToCheck, allSurroundingNotDownAndUpIsOther);
                return;
            }
            if (!leftHas && !leftHasOther && upHas && downHas && rightHasOther)
            {
                FrameTo(tileToCheck, allSurroundingNotLeftAndRightIsOther);
                return;
            }
            if (!rightHas && !rightHasOther && upHas && downHas && leftHasOther)
            {
                FrameTo(tileToCheck, allSurroundingNotRightAndLeftIsOther);
                return;
            }
        }
        public static void VanillaTileMergeWithOther(int i, int j, int otherType, int yOffset = 0, int topRightYOffset = 0)
        {
            TileFramingData data = TileFramingData.Get(i, j);
            int r = Main.rand.Next(3);
            int t = data.ThisTile.TileType;
            bool upHas = TileType(data.Up, t);
            bool downHas = TileType(data.Down, t);
            bool leftHas = TileType(data.Left, t);
            bool rightHas = TileType(data.Right, t);
            bool upHasOther = TileType(data.Up, otherType);
            bool downHasOther = TileType(data.Down, otherType);
            bool leftHasOther = TileType(data.Left, otherType);
            bool rightHasOther = TileType(data.Right, otherType);
            bool upLeftHasOther = TileType(data.UpLeft, otherType);
            bool upRightHasOther = TileType(data.UpRight, otherType);
            bool downLeftHasOther = TileType(data.DownLeft, otherType);
            bool downRightHasOther = TileType(data.DownRight, otherType);
            if (!upHasOther && !downHasOther && !leftHasOther && !rightHasOther && !upLeftHasOther && !upRightHasOther && !downLeftHasOther && !downRightHasOther)
            {
                return;
            }
            void FrameToHori(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = (short)(point.X + r * 18);
                tile.TileFrameY = (short)(point.Y + yOffset);
            }
            void FrameToVert(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = point.X;
                tile.TileFrameY = (short)(point.Y + r * 18 + yOffset);
            }
            Point16 allSurroundingIsOtherHori = new(108, 198);
            //
            Point16[] allSurroundingAndDownRightIsOther = [new(0, 90), new(0, 126), new(0, 162)];
            Point16[] allSurroundingAndDownLeftIsOther = [new(18, 90), new(18, 126), new(18, 162)];
            Point16[] allSurroundingAndUpRightIsOther = [new(0, 108), new(0, 144), new(0, 180)];
            Point16[] allSurroundingAndUpLeftIsOther = [new(18, 108), new(18, 144), new(18, 180)];
            //
            Point16[] allSurroundingButUpAndLeftIsOther = [new(36, 90), new(36, 126), new(36, 162)];
            Point16[] allSurroundingButUpAndRightIsOther = [new(54, 90), new(54, 126), new(54, 162)];
            Point16[] allSurroundingButDownAndLeftIsOther = [new(36, 108), new(36, 144), new(36, 180)];
            Point16[] allSurroundingButDownAndRightIsOther = [new(54, 108), new(54, 144), new(54, 180)];
            //
            Point16 allSurroundingNotLeftAndDownIsOtherVert = new(72, 90);
            Point16 allSurroundingNotLeftAndUpIsOtherVert = new(72, 144);
            Point16 allSurroundingNotRightAndDownIsOtherVert = new(90, 90);
            Point16 allSurroundingNotRightAndUpIsOtherVert = new(90, 144);
            //
            Point16 onlyDownAndIsOtherVert = new(108, 90);
            Point16 onlyUpAndIsOtherVert = new(108, 144);
            Point16 onlyLeftAndIsOtherHori = new(0, 234);
            Point16 onlyRightAndIsOtherHori = new(54, 234);
            //
            Point16 onlyUpDownAndDownIsOtherVert = new(126, 90);
            Point16 onlyUpDownAndUpIsOtherVert = new(126, 144);
            Point16 onlyLeftRightAndLeftIsOtherHori = new(0, 252);
            Point16 onlyLeftRightAndRightIsOtherHori = new(54, 252);
            //
            Point16 allSurroundingNotUpAndLeftIsOtherHori = new(0, 198);
            Point16 allSurroundingNotDownAndLeftIsOtherHori = new(0, 216);
            Point16 allSurroundingNotUpAndRightIsOtherHori = new(54, 198);
            Point16 allSurroundingNotDownAndRightIsOtherHori = new(54, 216);
            //
            Point16 onlyUpDownAndBothOtherVert = new(108, 216);
            Point16 onlyLeftRightAndBothOtherHori = new(162, 198);
            //
            Point16 allSurroundingAndDownIsOtherHori = new(144, 90);
            Point16 allSurroundingAndUpIsOtherHori = new(144, 108);
            Point16 allSurroundingAndRightIsOtherVert = new(144, 126);
            Point16 allSurroundingAndLeftIsOtherVert = new(162, 126);
            //
            Point16 allSurroundingAndUpDownAreOtherHori = new(144, 180);
            Point16 allSurroundingAndLeftRightAreOtherVert = new(180, 126);
            //
            Point16 allSurroundingAreOtherOpensDownVert = new(198, 90);
            Point16 allSurroundingAreOtherOpensUpVert = new(198, 144);
            Point16 allSurroundingAreOtherOpensRightVert = new(216, 90);
            Point16 allSurroundingAreOtherOpensLeftVert = new(216, 144);

            if (upHasOther && downHasOther && leftHasOther && rightHasOther)
            {
                FrameToHori(data.ThisTile, allSurroundingIsOtherHori);
                return;
            }
            VanillaTileMergeWithOtherTopRight(ref data.ThisTile, otherType, data.Up, data.Down, data.Left, data.Right, topRightYOffset);
            if (upHas && downHas && leftHas && rightHas)
            {
                if (downRightHasOther)
                {
                    data.ThisTile.TileFrameX = allSurroundingAndDownRightIsOther[r].X;
                    data.ThisTile.TileFrameY = (short)(allSurroundingAndDownRightIsOther[r].Y + yOffset);
                    return;
                }
                if (downLeftHasOther)
                {
                    data.ThisTile.TileFrameX = allSurroundingAndDownLeftIsOther[r].X;
                    data.ThisTile.TileFrameY = (short)(allSurroundingAndDownLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upLeftHasOther)
                {
                    data.ThisTile.TileFrameX = allSurroundingAndUpLeftIsOther[r].X;
                    data.ThisTile.TileFrameY = (short)(allSurroundingAndUpLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upRightHasOther)
                {
                    data.ThisTile.TileFrameX = allSurroundingAndUpRightIsOther[r].X;
                    data.ThisTile.TileFrameY = (short)(allSurroundingAndUpRightIsOther[r].Y + yOffset);
                    return;
                }
            }
            if (rightHas && downHas && upHasOther && leftHasOther)
            {
                data.ThisTile.TileFrameX = allSurroundingButUpAndLeftIsOther[r].X;
                data.ThisTile.TileFrameY = (short)(allSurroundingButUpAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && downHas && upHasOther && rightHasOther)
            {
                data.ThisTile.TileFrameX = allSurroundingButUpAndRightIsOther[r].X;
                data.ThisTile.TileFrameY = (short)(allSurroundingButUpAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (rightHas && upHas && downHasOther && leftHasOther)
            {
                data.ThisTile.TileFrameX = allSurroundingButDownAndLeftIsOther[r].X;
                data.ThisTile.TileFrameY = (short)(allSurroundingButDownAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && upHas && downHasOther && rightHasOther)
            {
                data.ThisTile.TileFrameX = allSurroundingButDownAndRightIsOther[r].X;
                data.ThisTile.TileFrameY = (short)(allSurroundingButDownAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && downHasOther && upHas && rightHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotLeftAndDownIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && upHasOther && downHas && rightHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotLeftAndUpIsOtherVert);
                return;
            }
            if (!rightHas && !rightHasOther && downHasOther && upHas && leftHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotRightAndDownIsOtherVert);
                return;
            }
            if (!rightHas && !rightHasOther && upHasOther && downHas && leftHas)
            {
                FrameToVert(data.ThisTile, allSurroundingNotRightAndUpIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !upHas && !upHasOther && !rightHas && !rightHasOther && downHasOther)
            {
                FrameToVert(data.ThisTile, onlyDownAndIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !downHas && !downHasOther && !rightHas && !rightHasOther && upHasOther)
            {
                FrameToVert(data.ThisTile, onlyUpAndIsOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !rightHas && !rightHasOther && !downHas && !downHasOther && leftHasOther)
            {
                FrameToHori(data.ThisTile, onlyLeftAndIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && !leftHas && !leftHasOther && !downHas && !downHasOther && rightHasOther)
            {
                FrameToHori(data.ThisTile, onlyRightAndIsOtherHori);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHas && downHasOther)
            {
                FrameToVert(data.ThisTile, onlyUpDownAndDownIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && downHas && upHasOther)
            {
                FrameToVert(data.ThisTile, onlyUpDownAndUpIsOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && rightHas && leftHasOther)
            {
                FrameToHori(data.ThisTile, onlyLeftRightAndLeftIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHas && rightHasOther)
            {
                FrameToHori(data.ThisTile, onlyLeftRightAndRightIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && leftHasOther && rightHas && downHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotUpAndLeftIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && rightHasOther && leftHas && downHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotUpAndRightIsOtherHori);
                return;
            }
            if (!downHas && !downHasOther && leftHasOther && rightHas && upHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotDownAndLeftIsOtherHori);
                return;
            }
            if (!downHas && !downHasOther && rightHasOther && leftHas && upHas)
            {
                FrameToHori(data.ThisTile, allSurroundingNotDownAndRightIsOtherHori);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHasOther && downHasOther)
            {
                FrameToVert(data.ThisTile, onlyUpDownAndBothOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHasOther && rightHasOther)
            {
                FrameToVert(data.ThisTile, onlyLeftRightAndBothOtherHori);
                return;
            }
            if (upHas && leftHas && rightHas && downHasOther)
            {
                FrameToHori(data.ThisTile, allSurroundingAndDownIsOtherHori);
                return;
            }
            if (downHas && leftHas && rightHas && upHasOther)
            {
                FrameToHori(data.ThisTile, allSurroundingAndUpIsOtherHori);
                return;
            }
            if (upHas && downHas && leftHas && rightHasOther)
            {
                FrameToVert(data.ThisTile, allSurroundingAndRightIsOtherVert);
                return;
            }
            if (upHas && downHas && rightHas && leftHasOther)
            {
                FrameToVert(data.ThisTile, allSurroundingAndLeftIsOtherVert);
                return;
            }
            if (upHasOther && downHasOther && leftHas && rightHas)
            {
                FrameToHori(data.ThisTile, allSurroundingAndUpDownAreOtherHori);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHas)
            {
                FrameToVert(data.ThisTile, allSurroundingAndLeftRightAreOtherVert);
                return;
            }
            //
            if (leftHasOther && rightHasOther && upHasOther && downHas)
            {
                FrameToVert(data.ThisTile, allSurroundingAreOtherOpensDownVert);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHasOther)
            {
                FrameToVert(data.ThisTile, allSurroundingAreOtherOpensUpVert);
                return;
            }
            if (leftHas && rightHasOther && upHasOther && downHasOther)
            {
                FrameToVert(data.ThisTile, allSurroundingAreOtherOpensLeftVert);
                return;
            }
            if (leftHasOther && rightHas && upHasOther && downHasOther)
            {
                FrameToVert(data.ThisTile, allSurroundingAreOtherOpensRightVert);
                return;
            }
        }
    }
}
