using ITD.Content.Tiles.BlueshroomGroves;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

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
        public static Vector2? RecursiveRaycast(Vector2 startWorldPos, float approxLengthTiles, float currentLengthTiles)
        {
            if (!(currentLengthTiles > approxLengthTiles))
            {
                currentLengthTiles++;
                if (Collision.SolidCollision(startWorldPos, 1, 1))
                {
                    return startWorldPos;
                }
                else
                {
                    return RecursiveRaycast(startWorldPos + new Vector2(0f, 8f), approxLengthTiles, currentLengthTiles);
                }
            }
            return null;
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
        public static bool AptForTree(int i, int j, int height)
        {
            Rectangle rect = new(i, j - height, 5, height);
            for (int k = rect.Left; k < rect.Right; k++)
            {
                for (int l = rect.Top; l < rect.Bottom; l++)
                {
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
        /// Attempts to recreate vanilla tile framing behaviour. I'm using this for pegmatite as I need pegmatite and diorite to merge properly and not look ugly.
        /// </summary>
        /// <param name="tileToCheck">Tile that will get re-framed</param>
        /// <param name="up">Tile above the center tile</param>
        /// <param name="down">Tile below the center tile</param>
        /// <param name="left">Tile to the left of the center tile</param>
        /// <param name="right">Tile to the right of the center tile</param>
        public static void VanillaTileFraming(ref Tile tileToCheck, Tile up, Tile upLeft, Tile upRight, Tile down, Tile downLeft, Tile downRight, Tile left, Tile right)
        {
            int r = Main.rand.Next(3);
            int t = tileToCheck.TileType;
            bool upHas = TileType(up, t);
            bool downHas = TileType(down, t);
            bool leftHas = TileType(left, t);
            bool rightHas = TileType(right, t);
            bool upLeftHas = TileType(upLeft, t);
            bool upRightHas = TileType(upRight, t);
            bool downLeftHas = TileType(downLeft, t);
            bool downRightHas = TileType(downRight, t);
            Point[] noSurrounding = [new(162, 54), new(180, 54), new(198, 54)];
            Point[] allSurrounding = [new(18, 18), new(36, 18), new(54, 18)];
            Point[] allSurroundingProtrudeUp = [new(108, 18), new(126, 18), new(144, 18)];
            Point[] allSurroundingProtrudeDown = [new(108, 36), new(126, 36), new(144, 36)];
            Point[] allSurroundingProtrudeLeft = [new(180, 0), new(180, 18), new(180, 36)];
            Point[] allSurroundingProtrudeRight = [new(198, 0), new(198, 18), new(198, 36)];
            Point[] allSurroundingNotLeft = [new(0, 0), new(0, 18), new(0, 36)];
            Point[] allSurroundingNotRight = [new(72, 0), new(72, 18), new(72, 36)];
            Point[] allSurroundingNotUp = [new(18, 0), new(36, 0), new(54, 0)];
            Point[] allSurroundingNotDown = [new(18, 36), new(36, 36), new(54, 36)];
            Point[] bothSurroundingUpDown = [new(90, 0), new(90, 18), new(90, 36)];
            Point[] bothSurroundingLeftRight = [new(108, 72), new(126, 72), new(144, 72)];
            Point[] bothSurroundingUpLeft = [new(18, 72), new(54, 72), new(90, 72)];
            Point[] bothSurroundingUpRight = [new(0, 72), new(36, 72), new(72, 72)];
            Point[] bothSurroundingDownLeft = [new(18, 54), new(54, 54), new(90, 54)];
            Point[] bothSurroundingDownRight = [new(0, 54), new(36, 54), new(72, 54)];
            Point[] onlySurroundingDown = [new(108, 0), new(126, 0), new(144, 0)];
            Point[] onlySurroundingUp = [new(108, 54), new(126, 54), new(144, 54)];
            Point[] onlySurroundingLeft = [new(216, 0), new(216, 18), new(216, 36)];
            Point[] onlySurroundingRight = [new(162, 0), new(162, 18), new(162, 36)];
            // If there are no surrounding tiles of the same type in the four cardinal directions
            if (!upHas && !rightHas && !leftHas && !downHas)
            {
                tileToCheck.TileFrameX = (short)noSurrounding[r].X;
                tileToCheck.TileFrameY = (short)noSurrounding[r].Y;
                return;
            }
            // If there are surrounding tiles of the same type in the four cardinal directions
            // This needs extra checks for when tiles are "protruding" vs completely surrounded
            if (upHas && rightHas && leftHas && downHas)
            {
                // If protruding up
                if (!upLeftHas && !upRightHas)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingProtrudeUp[r].X;
                    tileToCheck.TileFrameY = (short)allSurroundingProtrudeUp[r].Y;
                    return;
                }
                // If protruding down
                if (!downLeftHas && !downRightHas)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingProtrudeDown[r].X;
                    tileToCheck.TileFrameY = (short)allSurroundingProtrudeDown[r].Y;
                    return;
                }
                // If protruding left
                if (!upLeftHas && !downLeftHas)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingProtrudeLeft[r].X;
                    tileToCheck.TileFrameY = (short)allSurroundingProtrudeLeft[r].Y;
                    return;
                }
                // If protruding right
                if (!upRightHas && !downRightHas)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingProtrudeRight[r].X;
                    tileToCheck.TileFrameY = (short)allSurroundingProtrudeRight[r].Y;
                    return;
                }
                // If none (completely or almost completely surrounded)
                tileToCheck.TileFrameX = (short)allSurrounding[r].X;
                tileToCheck.TileFrameY = (short)allSurrounding[r].Y;
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the top
            if (leftHas && rightHas && downHas && !upHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotUp[r].X;
                tileToCheck.TileFrameY = (short)allSurroundingNotUp[r].Y;
                return;
            }
            // If there are surrounding tiles of the same type everywhere but the bottom
            if (leftHas && rightHas && upHas && !downHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotDown[r].X;
                tileToCheck.TileFrameY = (short)allSurroundingNotDown[r].Y;
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the left
            if (rightHas && upHas && downHas && !leftHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotLeft[r].X;
                tileToCheck.TileFrameY = (short)allSurroundingNotLeft[r].Y;
                return;
            }
            // If there are surrounding tiles of the same type everywhere but to the right
            if (leftHas && upHas && downHas && !rightHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotRight[r].X;
                tileToCheck.TileFrameY = (short)allSurroundingNotRight[r].Y;
                return;
            }
            // If there are only tiles of the same type above and below
            if (!rightHas && !leftHas && upHas && downHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingUpDown[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingUpDown[r].Y;
                return;
            }
            // If there are only tiles of the same type to the left and to the right
            if (!upHas && !downHas && leftHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingLeftRight[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingLeftRight[r].Y;
                return;
            }
            // If there are only tiles of the same type above and to the left
            if (!downHas && !rightHas && upHas && leftHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingUpLeft[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingUpLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type above and to the right
            if (!downHas && !leftHas && upHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingUpRight[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingUpRight[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the left
            if (!upHas && !rightHas && downHas && leftHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingDownLeft[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingDownLeft[r].Y;
                return;
            }
            // If there are only tiles of the same type below and to the right
            if (!upHas && !leftHas && downHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)bothSurroundingDownRight[r].X;
                tileToCheck.TileFrameY = (short)bothSurroundingDownRight[r].Y;
                return;
            }
            // If there's only a surrounding tile above
            if (!downHas && !rightHas && !leftHas && upHas)
            {
                tileToCheck.TileFrameX = (short)onlySurroundingUp[r].X;
                tileToCheck.TileFrameY = (short)onlySurroundingUp[r].Y;
                return;
            }
            // If there's only a surrounding tile below
            if (!upHas && !rightHas && !leftHas && downHas)
            {
                tileToCheck.TileFrameX = (short)onlySurroundingDown[r].X;
                tileToCheck.TileFrameY = (short)onlySurroundingDown[r].Y;
                return;
            }
            // If there's only a surrounding tile to the left
            if (!upHas && !downHas && !rightHas && leftHas)
            {
                tileToCheck.TileFrameX = (short)onlySurroundingLeft[r].X;
                tileToCheck.TileFrameY = (short)onlySurroundingLeft[r].Y;
                return;
            }
            // If there's only a surrounding tile to the right
            if (!upHas && !downHas && !leftHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)onlySurroundingRight[r].X;
                tileToCheck.TileFrameY = (short)onlySurroundingRight[r].Y;
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
            Point[] allSurroundingNotUpAndDownIsOther = [new(234, 0), new(252, 0), new(270, 0)];
            Point[] allSurroundingNotDownAndUpIsOther = [new(234, 18), new(252, 18), new(270, 18)];
            Point[] allSurroundingNotLeftAndRightIsOther = [new(234, 36), new(252, 36), new(270, 36)];
            Point[] allSurroundingNotRightAndLeftIsOther = [new(234, 54), new(252, 54), new(270, 54)];
            if (!upHas && !upHasOther && leftHas && rightHas && downHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotUpAndDownIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotUpAndDownIsOther[r].Y + yOffset);
                return;
            }
            if (!downHas && !downHasOther && leftHas && rightHas && upHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotDownAndUpIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotDownAndUpIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && upHas && downHas && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotLeftAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotLeftAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!rightHas && !rightHasOther && upHas && downHas && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotRightAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotRightAndLeftIsOther[r].Y + yOffset);
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
            Point[] allSurroundingIsOther = [new(108, 198), new(126, 198), new(144, 198)];
            //
            Point[] allSurroundingAndDownRightIsOther = [new(0, 90), new(0, 126), new(0, 162)];
            Point[] allSurroundingAndDownLeftIsOther = [new(18, 90), new(18, 126), new(18, 162)];
            Point[] allSurroundingAndUpRightIsOther = [new(0, 108), new(0, 144), new(0, 180)];
            Point[] allSurroundingAndUpLeftIsOther = [new(18, 108), new(18, 144), new(18, 180)];
            //
            Point[] allSurroundingButUpAndLeftIsOther = [new(36, 90), new(36, 126), new(36, 162)];
            Point[] allSurroundingButUpAndRightIsOther = [new(54, 90), new(54, 126), new(54, 162)];
            Point[] allSurroundingButDownAndLeftIsOther = [new(36, 108), new(36, 144), new(36, 180)];
            Point[] allSurroundingButDownAndRightIsOther = [new(54, 108), new(54, 144), new(54, 180)];
            //
            Point[] allSurroundingNotLeftAndDownIsOther = [new(72, 90), new(72, 108), new(72, 126)];
            Point[] allSurroundingNotLeftAndUpIsOther = [new(72, 144), new(72, 162), new(72, 180)];
            Point[] allSurroundingNotRightAndDownIsOther = [new(90, 90), new(90, 108), new(90, 126)];
            Point[] allSurroundingNotRightAndUpIsOther = [new(90, 144), new(90, 162), new(90, 180)];
            //
            Point[] onlyDownAndIsOther = [new(108, 90), new(108, 108), new(108, 126)];
            Point[] onlyUpAndIsOther = [new(108, 144), new(108, 162), new(108, 180)];
            Point[] onlyLeftAndIsOther = [new(0, 234), new(18, 234), new(36, 234)];
            Point[] onlyRightAndIsOther = [new(54, 234), new(72, 234), new(90, 234)];
            //
            Point[] onlyUpDownAndDownIsOther = [new(126, 90), new(126, 108), new(126, 126)];
            Point[] onlyUpDownAndUpIsOther = [new(126, 144), new(126, 162), new(126, 180)];
            Point[] onlyLeftRightAndLeftIsOther = [new(0, 252), new(18, 252), new(36, 252)];
            Point[] onlyLeftRightAndRightIsOther = [new(54, 252), new(72, 252), new(90, 252)];
            //
            Point[] allSurroundingNotUpAndLeftIsOther = [new(0, 198), new(18, 198), new(36, 198)];
            Point[] allSurroundingNotDownAndLeftIsOther = [new(0, 216), new(18, 216), new(36, 216)];
            Point[] allSurroundingNotUpAndRightIsOther = [new(54, 198), new(72, 198), new(90, 198)];
            Point[] allSurroundingNotDownAndRightIsOther = [new(54, 216), new(72, 216), new(90, 216)];
            //
            Point[] onlyUpDownAndBothOther = [new(108, 216), new(108, 234), new(108, 252)];
            Point[] onlyLeftRightAndBothOther = [new(162, 198), new(180, 198), new(198, 198)];
            //
            Point[] allSurroundingAndDownIsOther = [new(144, 90), new(162, 90), new(180, 90)];
            Point[] allSurroundingAndUpIsOther = [new(144, 108), new(162, 108), new(180, 108)];
            Point[] allSurroundingAndRightIsOther = [new(144, 126), new(144, 144), new(144, 162)];
            Point[] allSurroundingAndLeftIsOther = [new(162, 126), new(162, 144), new(162, 162)];
            //
            Point[] allSurroundingAndUpDownAreOther = [new(144, 180), new(162, 180), new(180, 180)];
            Point[] allSurroundingAndLeftRightAreOther = [new(180, 126), new(180, 144), new(180, 162)];
            //
            Point[] allSurroundingAreOtherOpensDown = [new(198, 90), new(198, 108), new(198, 126)];
            Point[] allSurroundingAreOtherOpensUp = [new(198, 144), new(198, 162), new(198, 180)];
            Point[] allSurroundingAreOtherOpensRight = [new(216, 90), new(216, 108), new(216, 126)];
            Point[] allSurroundingAreOtherOpensLeft = [new(216, 144), new(216, 162), new(216, 180)];

            if (upHasOther && downHasOther && leftHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingIsOther[r].Y + yOffset);
                return;
            }
            VanillaTileMergeWithOtherTopRight(ref tileToCheck, otherType, up, down, left, right, topRightYOffset);
            if (upHas && downHas && leftHas && rightHas)
            {
                if (downRightHasOther)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingAndDownRightIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndDownRightIsOther[r].Y + yOffset);
                    return;
                }
                if (downLeftHasOther)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingAndDownLeftIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndDownLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upLeftHasOther)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingAndUpLeftIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndUpLeftIsOther[r].Y + yOffset);
                    return;
                }
                if (upRightHasOther)
                {
                    tileToCheck.TileFrameX = (short)allSurroundingAndUpRightIsOther[r].X;
                    tileToCheck.TileFrameY = (short)(allSurroundingAndUpRightIsOther[r].Y + yOffset);
                    return;
                }
            }
            if (rightHas && downHas && upHasOther && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingButUpAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButUpAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && downHas && upHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingButUpAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButUpAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (rightHas && upHas && downHasOther && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingButDownAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButDownAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (leftHas && upHas && downHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingButDownAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingButDownAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && downHasOther && upHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotLeftAndDownIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotLeftAndDownIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && upHasOther && downHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotLeftAndUpIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotLeftAndUpIsOther[r].Y + yOffset);
                return;
            }
            if (!rightHas && !rightHasOther && downHasOther && upHas && leftHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotRightAndDownIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotRightAndDownIsOther[r].Y + yOffset);
                return;
            }
            if (!rightHas && !rightHasOther && upHasOther && downHas && leftHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotRightAndUpIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotRightAndUpIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && !upHas && !upHasOther && !rightHas && !rightHasOther && downHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyDownAndIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyDownAndIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && !downHas && !downHasOther && !rightHas && !rightHasOther && upHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyUpAndIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyUpAndIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && !rightHas && !rightHasOther && !downHas && !downHasOther && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyLeftAndIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyLeftAndIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && !leftHas && !leftHasOther && !downHas && !downHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyRightAndIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyRightAndIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHas && downHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyUpDownAndDownIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyUpDownAndDownIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && downHas && upHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyUpDownAndUpIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyUpDownAndUpIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && rightHas && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyLeftRightAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyLeftRightAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHas && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyLeftRightAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyLeftRightAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && leftHasOther && rightHas && downHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotUpAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotUpAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && rightHasOther && leftHas && downHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotUpAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotUpAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!downHas && !downHasOther && leftHasOther && rightHas && upHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotDownAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotDownAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (!downHas && !downHasOther && rightHasOther && leftHas && upHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingNotDownAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingNotDownAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (!leftHas && !leftHasOther && !rightHas && !rightHasOther && upHasOther && downHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyUpDownAndBothOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyUpDownAndBothOther[r].Y + yOffset);
                return;
            }
            if (!upHas && !upHasOther && !downHas && !downHasOther && leftHasOther && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)onlyLeftRightAndBothOther[r].X;
                tileToCheck.TileFrameY = (short)(onlyLeftRightAndBothOther[r].Y + yOffset);
                return;
            }
            if (upHas && leftHas && rightHas && downHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndDownIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndDownIsOther[r].Y + yOffset);
                return;
            }
            if (downHas && leftHas && rightHas && upHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndUpIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndUpIsOther[r].Y + yOffset);
                return;
            }
            if (upHas && downHas && leftHas && rightHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndRightIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndRightIsOther[r].Y + yOffset);
                return;
            }
            if (upHas && downHas && rightHas && leftHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndLeftIsOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndLeftIsOther[r].Y + yOffset);
                return;
            }
            if (upHasOther && downHasOther && leftHas && rightHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndUpDownAreOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndUpDownAreOther[r].Y + yOffset);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAndLeftRightAreOther[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAndLeftRightAreOther[r].Y + yOffset);
                return;
            }
            //
            if (leftHasOther && rightHasOther && upHasOther && downHas)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAreOtherOpensDown[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAreOtherOpensDown[r].Y + yOffset);
                return;
            }
            if (leftHasOther && rightHasOther && upHas && downHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAreOtherOpensUp[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAreOtherOpensUp[r].Y + yOffset);
                return;
            }
            if (leftHas && rightHasOther && upHasOther && downHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAreOtherOpensLeft[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAreOtherOpensLeft[r].Y + yOffset);
                return;
            }
            if (leftHasOther && rightHas && upHasOther && downHasOther)
            {
                tileToCheck.TileFrameX = (short)allSurroundingAreOtherOpensRight[r].X;
                tileToCheck.TileFrameY = (short)(allSurroundingAreOtherOpensRight[r].Y + yOffset);
                return;
            }
        }
    }
}
