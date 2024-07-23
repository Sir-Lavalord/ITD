using ITD.Content.Tiles.BlueshroomGroves;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace ITD
{
    public static class Helpers
    {
        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => new Color(v, t, p),
                1 => new Color(q, v, p),
                2 => new Color(p, v, t),
                3 => new Color(p, q, v),
                4 => new Color(t, p, v),
                _ => new Color(v, p, q),
            };
        }
        public static Vector2 QuickRaycast(Vector2 origin, Vector2 direction, float maxDistTiles = 64f)
        {
            origin /= 16f;
            direction = direction.SafeNormalize(Vector2.UnitY);
            Vector2 unitStepSize = new Vector2(
                (float)Math.Sqrt(1 + (direction.Y / direction.X) * (direction.Y / direction.X)),
                (float)Math.Sqrt(1 + (direction.X / direction.Y) * (direction.X / direction.Y))
                );
            Point tileCheck = origin.ToPoint();
            Vector2 rayLength1D;
            Point step;
            float curDistPixels = 0f;
            if (direction.X < 0)
            {
                step.X = -1;
                rayLength1D.X = (origin.X - (float)(tileCheck.X)) * unitStepSize.X;
            }
            else
            {
                step.X = 1;
                rayLength1D.X = ((float)(tileCheck.X + 1) - origin.X) * unitStepSize.X;
            }
            if (direction.Y < 0)
            {
                step.Y = -1;
                rayLength1D.Y = (origin.Y - (float)(tileCheck.Y)) * unitStepSize.Y;
            }
            else
            {
                step.Y = 1;
                rayLength1D.Y = ((float)(tileCheck.Y + 1) - origin.Y) * unitStepSize.Y;
            }
            bool tileFound = false;
            while (!tileFound && curDistPixels < maxDistTiles)
            {
                // Walk
                if (rayLength1D.X < rayLength1D.Y)
                {
                    tileCheck.X += step.X;
                    curDistPixels = rayLength1D.X;
                    rayLength1D.X += unitStepSize.X;
                }
                else
                {
                    tileCheck.Y += step.Y;
                    curDistPixels = rayLength1D.Y;
                    rayLength1D.Y += unitStepSize.Y;
                }
                if (SolidTile(tileCheck.X, tileCheck.Y))
                {
                    tileFound = true;
                }
            }
            Vector2 intersection = Vector2.Zero;
            if (tileFound)
            {
                intersection = origin + direction * curDistPixels;
            }
            else
            {
                intersection = origin + direction * maxDistTiles;
            }
            return intersection * 16f;
        }
        public static float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
        }
        public static void GrowBluegrass(int i, int j)
        {
            if (Main.tile[i, j].TileType == TileID.SnowBlock)
            {
                if (!SolidTile(i - 1, j) || !SolidTile(i + 1, j) || !SolidTile(i, j - 1) || !SolidTile(i, j + 1) || !SolidTile(i - 1, j - 1) || !SolidTile(i + 1, j + 1) || !SolidTile(i + 1, j - 1) || !SolidTile(i - 1, j + 1))
                {
                    WorldGen.ReplaceTile(i, j, (ushort)ModContent.TileType<Bluegrass>(), default);
                }
            }
        }
        public static void GrowTallBluegrass(int i, int j)
        {
            if (TileType(i, j + 1, ModContent.TileType<Bluegrass>()))
            {
                //WorldGen.Place1x1(i, j, ModContent.TileType<BluegrassBlades>(), WorldGen._genRand.Next(2));
                WorldGen.PlaceTile(i, j, ModContent.TileType<BluegrassBlades>(), true, false, -1, WorldGen._genRand.Next(3));
            }
        }
        public static bool TileType(int i, int j, int t) => Framing.GetTileSafely(i, j).HasTile && Framing.GetTileSafely(i, j).TileType == t;
        public static bool TileType(Tile tile, int t) => tile.HasTile && tile.TileType == t;
        public static bool SolidTile(int i, int j) => Framing.GetTileSafely(i, j).HasTile && Main.tileSolid[Framing.GetTileSafely(i, j).TileType];
        public static bool SolidTopTile(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (Main.tileSolidTop[Framing.GetTileSafely(i, j).TileType] || Main.tileSolid[Framing.GetTileSafely(i, j).TileType]);
        public static bool EdgeTileCross(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (!Framing.GetTileSafely(i + 1, j).HasTile || !Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile);
        public static bool EdgeTileX(int i, int j) => Framing.GetTileSafely(i, j).HasTile && (!Framing.GetTileSafely(i + 1, j + 1).HasTile || !Framing.GetTileSafely(i + 1, j - 1).HasTile || !Framing.GetTileSafely(i - 1, j + 1).HasTile || !Framing.GetTileSafely(i - 1, j - 1).HasTile);
        public static bool EdgeTile(int i, int j) => EdgeTileCross(i, j) || EdgeTileX(i, j);
        public static Vector2 CommonTileOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        public static Vector2 TileExtraPos(int i, int j, Vector2 extraOffset = default) => new Vector2(i, j) * 16 - Main.screenPosition + extraOffset + CommonTileOffset;
        public static bool AptForTree(int i, int j, int height)
        {
            Rectangle rect = new(i - 2, j - height, 5, height);
            if (Framing.GetTileSafely(i, j).HasTile)
            {
                return false;
            }
            for (int m = rect.Left+1; m < rect.Right-1; m++)
            {
                if (!Framing.GetTileSafely(m, rect.Bottom+1).HasTile)
                {
                    return false;
                }
            }
            for (int k = rect.Left; k < rect.Right; k++)
            {
                for (int l = rect.Top; l < rect.Bottom; l++)
                {
                    //Dust.QuickBox(new Vector2(k, l)*16f, new Vector2(k, l)*16f + new Vector2(16, 16), 8, Color.White, null);
                    if (Framing.GetTileSafely(k, l).HasTile)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static Point? GetTreeTopPosition(int i, int j)
        {
            Tile thisTile = Framing.GetTileSafely(i, j);
            bool isStump = (thisTile.TileFrameX == 0 && thisTile.TileFrameY >= 198) || thisTile.TileFrameX == 132 || (thisTile.TileFrameX == 110 && thisTile.TileFrameY <= 44) || thisTile.TileFrameX == 154;
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
        /// If any of the tiles right below this are standable, return true
        /// </summary>
        public static bool IsOnStandableGround(in float startX, float y, int width, bool onlySolid = false)
        {
            if (width <= 0)
            {
                throw new ArgumentException("width cannot be negative");
            }

            float fx = startX;

            //Needs atleast one iteration (in case width is 0)
            do
            {
                Point point = new Vector2(fx, y + 0.01f).ToTileCoordinates(); //0.01f is a magic number vanilla uses
                if (onlySolid && SolidTile(point.X, point.Y) || SolidTopTile(point.X, point.Y))
                {
                    return true;
                }
                fx += 16;
            }
            while (fx < startX + width);

            return false;
        }

        /// <inheritdoc cref="IsOnStandableGround(in float, float, int, bool)"/>
        public static bool IsOnStandableGround(this Entity entity, float yOffset = 0f, bool onlySolid = false)
        {
            return IsOnStandableGround(entity.BottomLeft.X, entity.BottomLeft.Y + yOffset, entity.width, onlySolid);
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

            if ((tile.Slope == 0 && !tile.IsHalfBlock) || (Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType])) //second one should be for platforms
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
        /// <param name="tileToCheck">Tile that will get re-framed</param>
        public static void VanillaTileFraming(ref Tile tileToCheck, Tile up, Tile upLeft, Tile upRight, Tile down, Tile downLeft, Tile downRight, Tile left, Tile right)
        {
            int t = tileToCheck.TileType;
            int r = Main.rand.Next(3);
            bool upHas = TileType(up, t);
            bool downHas = TileType(down, t);
            bool leftHas = TileType(left, t);
            bool rightHas = TileType(right, t);
            bool upLeftHas = TileType(upLeft, t);
            bool upRightHas = TileType(upRight, t);
            bool downLeftHas = TileType(downLeft, t);
            bool downRightHas = TileType(downRight, t);
            void FrameToHori(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = (short)(point.X + (r * 18));
                tile.TileFrameY = point.Y;
            }
            void FrameToVert(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = point.X;
                tile.TileFrameY = (short)(point.Y + (r * 18));
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
                FrameToHori(tileToCheck, noSurroundingHori);
                return;
            }
            // If there are surrounding tiles of the same type in the four cardinal directions
            // This needs extra checks for when tiles are "protruding" vs completely surrounded
            if (upHas && rightHas && leftHas && downHas)
            {
                // If protruding up
                if (!upLeftHas && !upRightHas)
                {
                    FrameToHori(tileToCheck, allSurroundingProtrudeUpHori);
                    return;
                }
                // If protruding down
                if (!downLeftHas && !downRightHas)
                {
                    FrameToHori(tileToCheck, allSurroundingProtrudeDownHori);
                    return;
                }
                // If protruding left
                if (!upLeftHas && !downLeftHas)
                {
                    FrameToVert(tileToCheck, allSurroundingProtrudeLeftVert);
                    return;
                }
                // If protruding right
                if (!upRightHas && !downRightHas)
                {
                    FrameToVert(tileToCheck, allSurroundingProtrudeRightVert);
                    return;
                }
                // If none (completely or almost completely surrounded)
                FrameToHori(tileToCheck, allSurroundingHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the top
            if (leftHas && rightHas && downHas && !upHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotUpHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the bottom
            if (leftHas && rightHas && upHas && !downHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotDownHori);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the left
            if (rightHas && upHas && downHas && !leftHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotLeftVert);
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the right
            if (leftHas && upHas && downHas && !rightHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotRightVert);
                return;
            }
            // If there are only tiles of the same type above and below
            if (!rightHas && !leftHas && upHas && downHas)
            {
                FrameToVert(tileToCheck, bothSurroundingUpDownVert);
                return;
            }
            // If there are only tiles of the same type to the left and to the right
            if (!upHas && !downHas && leftHas && rightHas)
            {
                FrameToHori(tileToCheck, bothSurroundingLeftRightHori);
                return;
            }
            // If there are only tiles of the same type above and to the left
            if (!downHas && !rightHas && upHas && leftHas)
            {
                tileToCheck.TileFrameX = bothSurroundingUpLeft[r].X;
                tileToCheck.TileFrameY = bothSurroundingUpLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type above and to the right
            if (!downHas && !leftHas && upHas && rightHas)
            {
                tileToCheck.TileFrameX = bothSurroundingUpRight[r].X;
                tileToCheck.TileFrameY = bothSurroundingUpRight[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the left
            if (!upHas && !rightHas && downHas && leftHas)
            {
                tileToCheck.TileFrameX = bothSurroundingDownLeft[r].X;
                tileToCheck.TileFrameY = bothSurroundingDownLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the right
            if (!upHas && !leftHas && downHas && rightHas)
            {
                tileToCheck.TileFrameX = bothSurroundingDownRight[r].X;
                tileToCheck.TileFrameY = bothSurroundingDownRight[r].Y;
                return;
            }
            // If there's only a surrounding tile above
            if (!downHas && !rightHas && !leftHas && upHas)
            {
                FrameToHori(tileToCheck, onlySurroundingUpHori);
                return;
            }
            // If there's only a surrounding tile below
            if (!upHas && !rightHas && !leftHas && downHas)
            {
                FrameToHori(tileToCheck, onlySurroundingDownHori);
                return;
            }
            // If there's only a surrounding tile to the left
            if (!upHas && !downHas && !rightHas && leftHas)
            {
                FrameToVert(tileToCheck, onlySurroundingLeftVert);
                return;
            }
            // If there's only a surrounding tile to the right
            if (!upHas && !downHas && !leftHas && rightHas)
            {
                FrameToVert(tileToCheck, onlySurroundingRightVert);
                return;
            }
        }
        public static void VanillaTileMergeWithOtherTopRight(ref Tile tileToCheck, int otherType, Tile up, Tile down, Tile left, Tile right, int yOffset)
        {
            int r = Main.rand.Next(3);
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
                tile.TileFrameX = (short)(point.X + (r * 18));
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
        public static void VanillaTileMergeWithOther(ref Tile tileToCheck, int otherType, Tile up, Tile upLeft, Tile upRight, Tile down, Tile downLeft, Tile downRight, Tile left, Tile right, int yOffset = 0, int topRightYOffset = 0)
        {
            int r = Main.rand.Next(3);
            int t = tileToCheck.TileType;
            bool upHas = TileType(up, t);
            bool downHas = TileType(down, t);
            bool leftHas = TileType(left, t);
            bool rightHas = TileType(right, t);
            bool upHasOther = TileType(up, otherType);
            bool downHasOther = TileType(down, otherType);
            bool leftHasOther = TileType(left, otherType);
            bool rightHasOther = TileType(right, otherType);
            bool upLeftHasOther = TileType(upLeft, otherType);
            bool upRightHasOther = TileType(upRight, otherType);
            bool downLeftHasOther = TileType(downLeft, otherType);
            bool downRightHasOther = TileType(downRight, otherType);
            if (!upHasOther && !downHasOther && !leftHasOther && !rightHasOther && !upLeftHasOther && !upRightHasOther && !downLeftHasOther && !downRightHasOther)
            {
                return;
            }
            void FrameToHori(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = (short)(point.X + (r * 18));
                tile.TileFrameY = (short)(point.Y + yOffset);
            }
            void FrameToVert(Tile tile, Point16 point)
            {
                int r = Main.rand.Next(3);
                tile.TileFrameX = point.X;
                tile.TileFrameY = (short)(point.Y + (r * 18) + yOffset);
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
                FrameToHori(tileToCheck, allSurroundingIsOtherHori);
                return;
            }
            VanillaTileMergeWithOtherTopRight(ref tileToCheck, otherType, up, down, left, right, topRightYOffset);
            if (upHas && downHas && leftHas && rightHas)
            {
                if (downRightHasOther)
                {
                    tileToCheck.TileFrameX = allSurroundingAndDownRightIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndDownRightIsOther[r].Y + yOffset);
                    return;
                }
                if (downLeftHasOther)
                {
                    tileToCheck.TileFrameX = allSurroundingAndDownLeftIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndDownLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upLeftHasOther)
                {
                    tileToCheck.TileFrameX = allSurroundingAndUpLeftIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndUpLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upRightHasOther)
                {
                    tileToCheck.TileFrameX = allSurroundingAndUpRightIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndUpRightIsOther[r].Y + yOffset);
                    return;
                }
            }
            if (rightHas && downHas && upHasOther && leftHasOther)
            {
                tileToCheck.TileFrameX = allSurroundingButUpAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButUpAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && downHas && upHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = allSurroundingButUpAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButUpAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (rightHas && upHas && downHasOther && leftHasOther)
            {
                tileToCheck.TileFrameX = allSurroundingButDownAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButDownAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && upHas && downHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = allSurroundingButDownAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButDownAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && downHasOther && upHas && rightHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotLeftAndDownIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && upHasOther && downHas && rightHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotLeftAndUpIsOtherVert);
                return;
            }
            if (!rightHas && !rightHasOther && downHasOther && upHas && leftHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotRightAndDownIsOtherVert);
                return;
            }
            if (!rightHas && !rightHasOther && upHasOther && downHas && leftHas)
            {
                FrameToVert(tileToCheck, allSurroundingNotRightAndUpIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !upHas && !upHasOther && !rightHas && !rightHasOther && downHasOther)
            {
                FrameToVert(tileToCheck, onlyDownAndIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !downHas && !downHasOther && !rightHas && !rightHasOther && upHasOther)
            {
                FrameToVert(tileToCheck, onlyUpAndIsOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !rightHas && !rightHasOther && !downHas && !downHasOther && leftHasOther)
            {
                FrameToHori(tileToCheck, onlyLeftAndIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && !leftHas && !leftHasOther && !downHas && !downHasOther && rightHasOther)
            {
                FrameToHori(tileToCheck, onlyRightAndIsOtherHori);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHas && downHasOther)
            {
                FrameToVert(tileToCheck, onlyUpDownAndDownIsOtherVert);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && downHas && upHasOther)
            {
                FrameToVert(tileToCheck, onlyUpDownAndUpIsOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && rightHas && leftHasOther)
            {
                FrameToHori(tileToCheck, onlyLeftRightAndLeftIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHas && rightHasOther)
            {
                FrameToHori(tileToCheck, onlyLeftRightAndRightIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && leftHasOther && rightHas && downHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotUpAndLeftIsOtherHori);
                return;
            }
            if (!upHas && !upHasOther && rightHasOther && leftHas && downHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotUpAndRightIsOtherHori);
                return;
            }
            if (!downHas && !downHasOther && leftHasOther && rightHas && upHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotDownAndLeftIsOtherHori);
                return;
            }
            if (!downHas && !downHasOther && rightHasOther && leftHas && upHas)
            {
                FrameToHori(tileToCheck, allSurroundingNotDownAndRightIsOtherHori);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHasOther && downHasOther)
            {
                FrameToVert(tileToCheck, onlyUpDownAndBothOtherVert);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHasOther && rightHasOther)
            {
                FrameToVert(tileToCheck, onlyLeftRightAndBothOtherHori);
                return;
            }
            if (upHas && leftHas && rightHas && downHasOther)
            {
                FrameToHori(tileToCheck, allSurroundingAndDownIsOtherHori);
                return;
            }
            if (downHas && leftHas && rightHas && upHasOther)
            {
                FrameToHori(tileToCheck, allSurroundingAndUpIsOtherHori);
                return;
            }
            if (upHas && downHas && leftHas && rightHasOther)
            {
                FrameToVert(tileToCheck, allSurroundingAndRightIsOtherVert);
                return;
            }
            if (upHas && downHas && rightHas && leftHasOther)
            {
                FrameToVert(tileToCheck, allSurroundingAndLeftIsOtherVert);
                return;
            }
            if (upHasOther && downHasOther && leftHas && rightHas)
            {
                FrameToHori(tileToCheck, allSurroundingAndUpDownAreOtherHori);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHas)
            {
                FrameToVert(tileToCheck, allSurroundingAndLeftRightAreOtherVert);
                return;
            }
            //
            if (leftHasOther && rightHasOther && upHasOther && downHas)
            {
                FrameToVert(tileToCheck, allSurroundingAreOtherOpensDownVert);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHasOther)
            {
                FrameToVert(tileToCheck, allSurroundingAreOtherOpensUpVert);
                return;
            }
            if (leftHas && rightHasOther && upHasOther && downHasOther)
            {
                FrameToVert(tileToCheck, allSurroundingAreOtherOpensLeftVert);
                return;
            }
            if (leftHasOther && rightHas && upHasOther && downHasOther)
            {
                FrameToVert(tileToCheck, allSurroundingAreOtherOpensRightVert);
                return;
            }
        }
    }
}
