using System;
using Terraria.DataStructures;
using Terraria.Audio;
using static ITD.Utilities.TileHelpers;

namespace ITD.Utilities
{
    public readonly struct RaycastData(Vector2 end, bool hit, float lengthSQ, Tile? tile = null)
    {
        public readonly Vector2 End = end;
        public readonly bool Hit = hit;
        public readonly float LengthSQ = lengthSQ;
        public readonly float Length { get { return MathF.Sqrt(LengthSQ); } }
        public readonly Tile? Tile = tile;
    }
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
        public static Point Lerp(this Point p1, Point p2, float alpha)
        {
            return new Point((int)MathHelper.Lerp(p1.X, p2.X, alpha), (int)MathHelper.Lerp(p1.Y, p2.Y, alpha));
        }
        public static void Along(this Vector2 mag, Vector2 center, float spacing, Action<Vector2> action)
        {
            Vector2 dir = mag.SafeNormalize(Vector2.Zero);
            if (dir == Vector2.Zero)
            {
                action(center);
                return;
            }
            for (float distance = 0f; (distance * distance) < mag.LengthSquared(); distance += spacing)
            {
                Vector2 point = center + (dir * distance);
                action(point);
            }
        }
        /// <summary>
        /// Cast a ray using a DDA algorithm with extra support for half blocks and slopes.
        /// </summary>
        /// <param name="origin">The point (in world coordinates) from which this ray should start.</param>
        /// <param name="direction">A vector representing the direction of the ray. Doesn't need to be normalized.</param>
        /// <param name="shouldContinue">A delegate that takes in a point and returns whether or not the ray should continue in a condition where it would otherwise stop. Useful for stopping platform collision, for example.</param>
        /// <param name="maxDistTiles">The tile distance at which the ray should stop if it hasn't found anything.</param>
        /// <param name="visualize">If true, spawns dust for every tile the ray analyzes. (even if it is skipped due to custom collision calculations)</param>
        /// <returns></returns>
        public static RaycastData QuickRaycast(Vector2 origin, Vector2 direction, Func<Point, bool> shouldContinue = null, float maxDistTiles = 64f, bool visualize = false)
        {
            shouldContinue ??= p => false;
            origin /= 16f;
            direction = direction.SafeNormalize(Vector2.Zero);
            Vector2 unitStepSize = new (
                (float)Math.Sqrt(1 + direction.Y / direction.X * (direction.Y / direction.X)),
                (float)Math.Sqrt(1 + direction.X / direction.Y * (direction.X / direction.Y))
                );
            Point tileCheck = origin.ToPoint();
            Vector2 rayLength1D;
            Point step;
            float curDistPixels = 0f;
            if (direction.X < 0)
            {
                step.X = -1;
                rayLength1D.X = (origin.X - tileCheck.X) * unitStepSize.X;
            }
            else
            {
                step.X = 1;
                rayLength1D.X = (tileCheck.X + 1 - origin.X) * unitStepSize.X;
            }
            if (direction.Y < 0)
            {
                step.Y = -1;
                rayLength1D.Y = (origin.Y - tileCheck.Y) * unitStepSize.Y;
            }
            else
            {
                step.Y = 1;
                rayLength1D.Y = (tileCheck.Y + 1 - origin.Y) * unitStepSize.Y;
            }
            bool tileFound = false;
            Tile? intersectTile = null;
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
                if (visualize)
                {
                    Dust d = Dust.NewDustPerfect(tileCheck.ToWorldCoordinates(), DustID.WhiteTorch);
                    d.noGravity = true;
                }
                if (SolidTile(tileCheck, out Tile tile))
                {
                    // set to false to skip this tile
                    bool isHit = true;
                    // vector2 that goes from vector2.zero on the top left of the tile to new vector2.one on the bottom right
                    Vector2 localIntersection = (origin + direction * curDistPixels) - tileCheck.ToVector2();
                    if (tile.IsHalfBlock)
                    {
                        bool isTopHalf = localIntersection.Y < 0.5f;
                        // if the local intersection is on the top half:
                        if (isTopHalf)
                        {
                            // if the raycast is going downwards, run all our logic.
                            if (direction.Y > 0f)
                            {
                                float yRemaining = 0.5f - localIntersection.Y;
                                float distToNextIntersection = yRemaining / Math.Abs(direction.Y);

                                // now, we could easily just:
                                // - add distToNextIntersection to curDistPixels
                                // - recalculate localIntersection
                                // - see if localIntersection.X is less than 0f, or more than 1f
                                // but that's a whole one extra flop (probably with more overhead since it's vector math)!
                                // so instead let's just recalculate x, which is all we need anyway.
                                float nextX = localIntersection.X + distToNextIntersection * direction.X;

                                if (nextX < 0f || nextX > 1f)
                                    // out of bounds? that means we missed, so skip this tile.
                                    isHit = false;
                                else
                                    // finally, if we're not out of bounds, add the distance to the ray's pixel distance.
                                    curDistPixels += distToNextIntersection;
                            }
                            // if it's going upwards, that means we missed, so skip this tile.
                            else
                            {
                                isHit = false;
                            }
                        }
                        // we don't need to add extra logic for the bottom half because it would just collide like a regular tile then.
                    }
                    else if (tile.Slope != SlopeType.Solid)
                    {
                        // we can actually calculate all that we need using this. as you can see it's basically the way we did half blocks except we calculate both x and y.
                        void CalculateSlopedCollision(bool rising)
                        {
                            // for a rising slope, the ray's vertical offset decreases as it moves along the slope
                            // for a falling slope, the ray's vertical offset increases as it moves along the slope

                            // this little function took me about 4 hours to make because i fucking suck at math
                            // now i can rest easy
                            float localDiff = rising ? 1f - localIntersection.X - localIntersection.Y : localIntersection.Y - localIntersection.X;
                            float localDistFactor = rising ? direction.X + direction.Y : direction.X - direction.Y;

                            if (localDistFactor == 0f)
                            {
                                isHit = false;
                                return;
                            }

                            float distToNextIntersection = localDiff / localDistFactor;

                            float nextX = localIntersection.X + distToNextIntersection * direction.X;
                            float nextY = localIntersection.Y + distToNextIntersection * direction.Y;

                            if (nextX >= 0f && nextX <= 1f && nextY >= 0f && nextY <= 1f)
                                curDistPixels += distToNextIntersection;
                            else
                                isHit = false;
                        }

                        switch (tile.Slope)
                        {
                            // imagine a square of 4 tiles, sloped to look like a diamond. (image below) (i swear it's not an ip grabber i would never do that i just don't like long links)
                            // https://bit.ly/4hyqy6N

                            // bottom and right are solid and aligned with tile grid. this would be tile [0, 0] on the diamond. it is a rising slope.
                            case SlopeType.SlopeDownRight:
                                // check if custom collision should be done in the first place
                                if (localIntersection.Y < 1f && localIntersection.X < 1f)
                                    CalculateSlopedCollision(true);
                                break;

                            // bottom and left are solid and aligned with tile grid. this would be tile [1, 0] on the diamond. it is a falling slope.
                            case SlopeType.SlopeDownLeft:

                                // check if custom collision should be done in the first place
                                if (localIntersection.Y < 1f && localIntersection.X > 0f)
                                    CalculateSlopedCollision(false);
                                break;

                            // top and right are solid and aligned with tile grid. this would be tile [0, 1] on the diamond. it is a falling slope.
                            case SlopeType.SlopeUpRight:
                                // check if custom collision should be done in the first place
                                if (localIntersection.Y > 0f && localIntersection.X < 1f)
                                    CalculateSlopedCollision(false);
                                break;

                            // top and left are solid and aligned with tile grid. this would be tile [1, 1] on the diamond. it is a rising slope.
                            case SlopeType.SlopeUpLeft:
                                // check if custom collision should be done in the first place
                                if (localIntersection.Y > 0f && localIntersection.X > 0f)
                                    CalculateSlopedCollision(true);
                                break;
                        }
                    }

                    if (isHit && !shouldContinue(tileCheck))
                    {
                        tileFound = true;
                        intersectTile = tile;
                    }
                }
            }
            Vector2 intersection;
            if (tileFound)
            {
                intersection = origin + direction * curDistPixels;
            }
            else
            {
                intersection = origin + direction * maxDistTiles;
            }
            return new RaycastData(intersection * 16f, tileFound, Vector2.DistanceSquared(origin * 16f, intersection * 16f), intersectTile);
        }
        public static float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
        }
        public static void SpreadGrass(int i, int j, int grassType, int soilType)
        {
            GrowGrass(i - 1, j, grassType, soilType);
            GrowGrass(i + 1, j, grassType, soilType);
            GrowGrass(i, j - 1, grassType, soilType);
            GrowGrass(i, j + 1, grassType, soilType);
        }
        public static bool GrowGrass(int i, int j, int grassType, int soilType)
        {
            Tile t = Framing.GetTileSafely(i, j);
            if (t.TileType == soilType)
            {
                if (EdgeTile(i ,j) && !TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Framing.GetTileSafely(i, j-1).TileType])
                {
                    t.TileType = (ushort)grassType;
                    WorldGen.SquareTileFrame(i, j);
                    NetMessage.SendTileSquare(-1, i, j, 1);
                    return true;
                }
            }
            return false;
        }
        public static void GrowTallGrass(int i, int j, int tallGrassType, int grassType)
        {
            if (TileType(i, j + 1, grassType) && !SolidTile(i, j) && !TileType(i, j, tallGrassType))
            {
                if (WorldGen.PlaceTile(i, j, tallGrassType, true, false, -1, WorldGen._genRand.Next(3)))
                    NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
        public static void GrowLongMossForTile(int i , int j, int longMossType, int mossType)
        {
            Tile t = Framing.GetTileSafely(i, j);
            if (t.TileType != mossType)
                return;
            GrowLongMoss(i + 1, j, longMossType, mossType);
            GrowLongMoss(i - 1, j, longMossType, mossType);
            GrowLongMoss(i, j + 1, longMossType, mossType);
            GrowLongMoss(i, j - 1, longMossType, mossType);
        }
        public static void GrowLongMoss(int i, int j, int longMossType, int mossType)
        {
            if (SolidTile(i, j) || TileType(i, j, longMossType)) return;

            bool hasUp = TileType(i, j - 1, mossType);
            bool hasDown = TileType(i, j + 1, mossType);
            bool hasLeft = TileType(i - 1, j, mossType);
            //bool hasRight = TileType(i + 1, j, mossType);

            int alt = hasDown ? 0 : hasUp ? 3 : hasLeft ? 6 : 9;

            if (WorldGen.PlaceObject(i, j, longMossType, true, 0, alt))
            {
                NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
        public static bool UseItem_PlaceSeeds(Player player, int seedsTile, int soilTile)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                int tX = Player.tileTargetX;
                int tY = Player.tileTargetY;
                if (player.IsInTileInteractionRange(tX, tY, TileReachCheckSettings.Simple))
                {
                    if (GrowGrass(tX, tY, seedsTile, soilTile))
                    {
                        SoundEngine.PlaySound(SoundID.Dig, Main.MouseWorld);
                        return true;
                    }
                }
            }
            return false;
        }
        public static void DefaultToSeeds(this Item item)
        {
            item.width = 22;
            item.height = 18;
            item.consumable = true;
            item.maxStack = Item.CommonMaxStack;
            item.useStyle = ItemUseStyleID.Swing;
            item.holdStyle = ItemHoldStyleID.None;
            item.useAnimation = item.useTime = 15;
            item.useTurn = true;
            item.autoReuse = true;
        }
        public static void DefaultToSnaptrap(this Item item, int newWidth, int newHeight, int snaptrapType, float shootSpeed, int useTime, int damage, SoundStyle? useSound = null)
        {
            item.knockBack = 0f;
            item.autoReuse = false;
            item.useStyle = ItemUseStyleID.Swing;
            item.width = newWidth;
            item.height = newHeight;
            item.damage = damage;
            item.shoot = snaptrapType;
            item.useTime = item.useAnimation = useTime;
            item.shootSpeed = shootSpeed;
            item.DamageType = DamageClass.Melee;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.UseSound = useSound ?? SoundID.Item1;
        }
        public static void DefaultToFurniture(this Item item, int createTile, int width, int height)
        {
            item.width = width;
            item.height = height;
            item.maxStack = 9999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.Swing;
            item.consumable = true;
            item.createTile = createTile;
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
    }
}
