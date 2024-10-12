using ITD.Content.Tiles.BlueshroomGroves;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Audio;
using static ITD.Utilities.TileHelpers;

namespace ITD.Utilities
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
        public static Point Lerp(this Point p1, Point p2, float alpha)
        {
            return new Point((int)MathHelper.Lerp(p1.X, p2.X, alpha), (int)MathHelper.Lerp(p1.Y, p2.Y, alpha));
        }
        public static (Vector2, bool) QuickRaycast(Vector2 origin, Vector2 direction, bool shouldHitNPCs = false, bool shouldHitPlatforms = false, float maxDistTiles = 64f)
        {
            origin /= 16f;
            direction = direction.SafeNormalize(Vector2.UnitY);
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
                if ((shouldHitPlatforms && SolidTile(tileCheck.X, tileCheck.Y)) || WorldGen.SolidTile(tileCheck.X, tileCheck.Y, false))
                {
                    tileFound = true;
                }
                if (shouldHitNPCs)
                {
                    Vector2 worldPosition = tileCheck.ToWorldCoordinates();
                    foreach (var npc in Main.ActiveNPCs)
                    {
                        if (npc.getRect().Contains(worldPosition.ToPoint()))
                        {
                            tileFound = true;
                        }
                    }
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
            return (intersection * 16f, tileFound);
        }
        public static float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
        }
        public static bool GrowBluegrass(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            if (t.TileType == TileID.SnowBlock)
            {
                if (EdgeTile(i ,j) && !(Framing.GetTileSafely(i, j-1).TileType == TileID.Trees))
                {
                    t.TileType = (ushort)ModContent.TileType<Bluegrass>();
                    WorldGen.SquareTileFrame(i, j);
                    NetMessage.SendTileSquare(-1, i, j, 1);
                    return true;
                }
            }
            return false;
        }
        public static void GrowTallBluegrass(int i, int j)
        {
            if (TileType(i, j + 1, ModContent.TileType<Bluegrass>()) && !SolidTile(i, j) && !TileType(i, j, ModContent.TileType<BluegrassBlades>()))
            {
                //WorldGen.Place1x1(i, j, ModContent.TileType<BluegrassBlades>(), WorldGen._genRand.Next(2));
                if (WorldGen.PlaceTile(i, j, ModContent.TileType<BluegrassBlades>(), true, false, -1, WorldGen._genRand.Next(3)))
                    NetMessage.SendTileSquare(-1, i, j, 1);
            }
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
