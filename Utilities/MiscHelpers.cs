﻿using System;
using System.Linq;
using ReLogic.Utilities;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace ITD.Utilities
{
    public static class MiscHelpers
    {
        public static T[] EntityQuery<T>(T ignore = null, int maxAmount = -1, Func<T, bool> predicate = null) where T : Entity
        {
            Type entityType = typeof(T);

            Entity[] entitiesTemp = entityType switch
            {
                Type t when t == typeof(NPC) => Main.npc,
                Type t when t == typeof(Projectile) => Main.projectile,
                Type t when t == typeof(Player) => Main.player,
                Type t when t == typeof(Item) => Main.item,
                _ => null
            };
            if (entitiesTemp is null) return [];

            T[] entities = entitiesTemp.Cast<T>().ToArray();

            bool shouldTryIgnoringSelf = ignore != null && entityType == ignore.GetType();

            T[] result = entities
            .Where(e => predicate(e) && (!shouldTryIgnoringSelf || ignore.whoAmI != e.whoAmI))
            .Take(maxAmount > 0 ? maxAmount : entities.Length)
            .ToArray();

            return result;
        }
        public static NPC FindClosestNPC(this Projectile projectile, float maxDetectDistance)
        {
            NPC closestNPC = null;

            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            foreach (var target in Main.ActiveNPCs)
            {
                if (projectile.IsValidTarget(target))
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, projectile.Center);

                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
						//Main.NewText(target.type);
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
			return closestNPC;
        }
        //swiped off fargo

        public static Projectile NewProjectileDirectSafe(IEntitySource spawnSource, Vector2 pos, Vector2 vel, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f)
        {
            int p = Projectile.NewProjectile(spawnSource, pos, vel, type, damage, knockback, owner, ai0, ai1);
            return p < Main.maxProjectiles ? Main.projectile[p] : null;
        }

        //SORT of swiped off fargo
        public static int GetProjectileByIdentity(int player, float projectileIdentity, params int[] projectileType)
        {
            return GetProjectileByIdentity(player, (int)projectileIdentity, projectileType);
        }
        public static int GetProjectileByIdentity(int player, int projectileIdentity, params int[] projectileType)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].identity == projectileIdentity && Main.projectile[i].owner == player
                    && (projectileType.Length == 0 || projectileType.Contains(Main.projectile[i].type)))
                {
                    return i;
                }
            }
            return -1;
        }
        /*        public static int GetProjectileByIdentity(int player, int projectileIdentity, params int[] projectileType)
                {
                    foreach (var p in Main.ActiveProjectiles)
                    {
                        if (p.identity == projectileIdentity && p.owner == player && (projectileType.Length == 0 || projectileType.Contains(p.type)))
                            return p.whoAmI;
                    }
                    return -1;
                }*/
        // thank you gabehaswinned 🥲
        // (swiped off of spirit)
        public static Vector2 GetArcVel(Vector2 startingPos, Vector2 targetPos, float gravity, float? minArcHeight = null, float? maxArcHeight = null, float? maxXvel = null, float? heightabovetarget = null, float downwardsYVelMult = 1f)
        {
            Vector2 DistanceToTravel = targetPos - startingPos;
            float MaxHeight = DistanceToTravel.Y - (heightabovetarget ?? 0);
            if (minArcHeight != null)
                MaxHeight = Math.Min(MaxHeight, -(float)minArcHeight);

            if (maxArcHeight != null)
                MaxHeight = Math.Max(MaxHeight, -(float)maxArcHeight);

            float TravelTime;
            float neededYvel;
            if (MaxHeight <= 0)
            {
                neededYvel = -(float)Math.Sqrt(-2 * gravity * MaxHeight);
                TravelTime = (float)Math.Sqrt(-2 * MaxHeight / gravity) + (float)Math.Sqrt(2 * Math.Max(DistanceToTravel.Y - MaxHeight, 0) / gravity); //time up, then time down
            }

            else
            {
                neededYvel = Vector2.Normalize(DistanceToTravel).Y * downwardsYVelMult;
                TravelTime = (-neededYvel + (float)Math.Sqrt(Math.Pow(neededYvel, 2) - (4 * -DistanceToTravel.Y * gravity / 2))) / (gravity); //time down
            }

            if (maxXvel != null)
                return new Vector2(MathHelper.Clamp(DistanceToTravel.X / TravelTime, -(float)maxXvel, (float)maxXvel), neededYvel);

            return new Vector2(DistanceToTravel.X / TravelTime, neededYvel);
        }
        public static Rectangle DynamicRectangle(Point p1, Point p2, out Point topLeftPoint, out Point bottomRightPoint)
        {
            int minX = Math.Min(p1.X, p2.X);
            int minY = Math.Min(p1.Y, p2.Y);

            topLeftPoint = new Point(minX, minY);

            int maxX = Math.Max(p1.X, p2.X);
            int maxY = Math.Max(p1.Y, p2.Y);

            bottomRightPoint = new Point(maxX, maxY);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        public static Rectangle ContainsRectangles(Rectangle rect1, Rectangle rect2)
        {
            int minX = Math.Min(rect1.X, rect2.X);
            int minY = Math.Min(rect1.Y, rect2.Y);

            int maxRight = Math.Max(rect1.Right, rect2.Right);
            int maxBottom = Math.Max(rect1.Bottom, rect2.Bottom);

            int width = maxRight - minX;
            int height = maxBottom - minY;

            return new Rectangle(minX, minY, width, height);
        }
        public static Rectangle Inflated(this Rectangle rect, int horizontalValue, int verticalValue)
        {
            int newX = rect.X - horizontalValue;
            int newY = rect.Y - verticalValue;
            int newW = rect.Width + horizontalValue * 2;
            int newH = rect.Height + verticalValue * 2;
            return new(newX, newY, newW, newH);
        }
        public static Rectangle Inflated(this Rectangle rect, int value) => rect.Inflated(value, value);
        /// <summary>
        /// Returns a <see cref="Rectangle"/> that fully contains this Rectangle in tile space. Suitable for tile queries.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle ToTileRectangle(this Rectangle rect)
        {
            return new Rectangle
            (
                x: rect.X / 16,
                y: rect.Y / 16,
                width: (int)Math.Ceiling(rect.Width / 16f),
                height: (int)Math.Ceiling(rect.Height / 16f)
            );
        }
        public static Rectangle ToWorldRectangle(this Rectangle rect, int addTopLeft = 0, int addBottomRight = 0) => new(rect.X * 16 + addTopLeft, rect.Y * 16 + addTopLeft, rect.Width * 16 + addBottomRight, rect.Height * 16 + addBottomRight);
        /// <summary>
        /// Returns a <see cref="Rectangle"/> that fully contains this Entity in tile space. Suitable for tile queries. More precise than <see cref="ToTileRectangle(Rectangle)"/> in specific situations.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Rectangle TileRectangle(this Entity entity)
        {
            int x = (int)(entity.position.X / 16);
            int y = (int)(entity.position.Y / 16);
            int width = (int)Math.Ceiling(entity.width / 16f);
            int height = (int)Math.Ceiling(entity.height / 16f);

            return new(x, y, width, height);
        }
        public static float AngleDiff(Vector2 origin, params Vector2[] vecs)
        {
            vecs = [origin, .. vecs.OrderBy(v => v.DistanceSQ(origin))];

            float totalAngleDifference = 0f;
            for (int i = 1; i < vecs.Length; i++)
            {
                totalAngleDifference += vecs[i - 1].AngleTo(vecs[i]);
            }
            return totalAngleDifference;
        }
        /// <summary>
        /// <para>Lerps through all members of the provided <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Example: for a Color lerp from [Color.White, Color.Yellow, Color.Red], you would see white at 0f amount, yellow at 0.5f, and red at 1f.</para>
        /// Special thanks to LolXD for code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="amount"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T LerpMany<T>(float amount, ReadOnlySpan<T> values) where T : struct
        {
            float p = MathHelper.Clamp(amount * (values.Length - 1), 0, values.Length - 1);
            int start = (int)p;
            int end = Math.Min(start + 1, values.Length - 1);
            return LerpAny(values[start], values[end], p - start);
        }
        /// <summary>
        /// <para>Lerps ANY of the supported types.</para>
        /// <para>Not for use in general applications. Just use the actual lerp function for your datatype if it exists.</para>
        /// Sorted by size, then amount of members.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static T LerpAny<T>(T start, T end, float amount) where T : struct
        {
            #region EXTENDED LERP TYPES (Unnecessary)
            /*
            if (typeof(T) == typeof(byte))
            {
                byte result = (byte)MathHelper.Lerp(
                    Unsafe.As<T, byte>(ref start),
                    Unsafe.As<T, byte>(ref end),
                    amount
                );
                return Unsafe.As<byte, T>(ref result);
            }

            else if (typeof(T) == typeof(sbyte))
            {
                sbyte result = (sbyte)MathHelper.Lerp(
                    Unsafe.As<T, sbyte>(ref start),
                    Unsafe.As<T, sbyte>(ref end),
                    amount
                );
                return Unsafe.As<sbyte, T>(ref result);
            }

            else if (typeof(T) == typeof(ushort))
            {
                ushort result = (ushort)MathHelper.Lerp(
                    Unsafe.As<T, ushort>(ref start),
                    Unsafe.As<T, ushort>(ref end),
                    amount
                );
                return Unsafe.As<ushort, T>(ref result);
            }

            else if (typeof(T) == typeof(short))
            {
                short result = (short)MathHelper.Lerp(
                    Unsafe.As<T, short>(ref start),
                    Unsafe.As<T, short>(ref end),
                    amount
                );
                return Unsafe.As<short, T>(ref result);
            }

            else if (typeof(T) == typeof(uint))
            {
                uint result = (uint)MathHelper.Lerp(
                    Unsafe.As<T, uint>(ref start),
                    Unsafe.As<T, uint>(ref end),
                    amount
                );
                return Unsafe.As<uint, T>(ref result);
            }

            else if (typeof(T) == typeof(ulong))
            {
                ulong result = (ulong)MathHelper.Lerp(
                    Unsafe.As<T, ulong>(ref start),
                    Unsafe.As<T, ulong>(ref end),
                    amount
                );
                return Unsafe.As<ulong, T>(ref result);
            }

            else if (typeof(T) == typeof(long))
            {
                long result = (long)MathHelper.Lerp(
                    Unsafe.As<T, long>(ref start),
                    Unsafe.As<T, long>(ref end),
                    amount
                );
                return Unsafe.As<long, T>(ref result);
            }
            */
            #endregion

            if (typeof(T) == typeof(int))
            {
                int result = (int)MathHelper.Lerp(
                    Unsafe.As<T, int>(ref start),
                    Unsafe.As<T, int>(ref end),
                    amount
                );
                return Unsafe.As<int, T>(ref result);
            }

            else if (typeof(T) == typeof(float))
            {
                float result = MathHelper.Lerp(
                    Unsafe.As<T, float>(ref start),
                    Unsafe.As<T, float>(ref end),
                    amount
                );
                return Unsafe.As<float, T>(ref result);
            }

            else if (typeof(T) == typeof(double))
            {
                double result = double.Lerp(
                    Unsafe.As<T, double>(ref start),
                    Unsafe.As<T, double>(ref end),
                    amount
                );
                return Unsafe.As<double, T>(ref result);
            }

            else if (typeof(T) == typeof(Vector2))
            {
                Vector2 result = Vector2.Lerp(
                    Unsafe.As<T, Vector2>(ref start),
                    Unsafe.As<T, Vector2>(ref end),
                    amount
                );
                return Unsafe.As<Vector2, T>(ref result);
            }

            else if (typeof(T) == typeof(Vector2D))
            {
                Vector2D result = Vector2D.Lerp(
                    Unsafe.As<T, Vector2D>(ref start),
                    Unsafe.As<T, Vector2D>(ref end),
                    amount
                );
                return Unsafe.As<Vector2D, T>(ref result);
            }

            else if (typeof(T) == typeof(Vector3))
            {
                Vector3 result = Vector3.Lerp(
                    Unsafe.As<T, Vector3>(ref start),
                    Unsafe.As<T, Vector3>(ref end),
                    amount
                );
                return Unsafe.As<Vector3, T>(ref result);
            }

            else if (typeof(T) == typeof(Color))
            {
                Color result = Color.Lerp(
                    Unsafe.As<T, Color>(ref start),
                    Unsafe.As<T, Color>(ref end),
                    amount
                );
                return Unsafe.As<Color, T>(ref result);
            }

            else if (typeof(T) == typeof(Rectangle))
            {
                Rectangle startt = Unsafe.As<T, Rectangle>(ref start);
                Rectangle endd = Unsafe.As<T, Rectangle>(ref end);
                Rectangle result = new(
                    (int)MathHelper.Lerp(startt.X, endd.X, amount),
                    (int)MathHelper.Lerp(startt.Y, endd.Y, amount),
                    (int)MathHelper.Lerp(startt.Width, endd.Width, amount),
                    (int)MathHelper.Lerp(startt.Height, endd.Height, amount));
                return Unsafe.As<Rectangle, T>(ref result);
            }

            else if (typeof(T) == typeof(Vector4))
            {
                Vector4 result = Vector4.Lerp(
                    Unsafe.As<T, Vector4>(ref start),
                    Unsafe.As<T, Vector4>(ref end),
                    amount
                );
                return Unsafe.As<Vector4, T>(ref result);
            }

            else
            {
                throw new InvalidOperationException("Unsupported lerp type");
                // feel free to add your own stuff here but i don't think there's anything else important to add
            }
        }
        public static Vector2D ToRotationVector2D (this double d) => new(Math.Cos(d), Math.Sin(d));
        public static Vector2D ToRotationVector2D (this float f) => ToRotationVector2D(f);
        public static Vector3 ToVector3(this Vector2 v) => new(v.X, v.Y, 0);
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            float length = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            spriteBatch.DrawLine(point1, length, angle, color, thickness);
        }
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            spriteBatch.Draw(origin: new Vector2(0f, 0.5f), scale: new Vector2(length, thickness), texture: ITD.TrueMagicPixel.Value, position: point, sourceRectangle: null, color: color, rotation: angle, effects: SpriteEffects.None, layerDepth: 0f);
        }
        public static void DrawDottedLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float gapFrequency, float gapSize, float thickness = 1f)
        {
            float totalLength = Vector2.Distance(point1, point2);
            Vector2 direction = point1.DirectionTo(point2);

            float segmentLength = gapFrequency - gapSize;
            float t = 0f;

            while (t < totalLength)
            {
                float drawLength = Math.Min(segmentLength, totalLength - t);
                Vector2 start = point1 + direction * t;
                spriteBatch.DrawLine(start, drawLength, (float)Math.Atan2(direction.Y, direction.X), color, thickness);
                t += gapFrequency;
            }
        }
        public static bool Exists(this NPC n) => n != null && n.active;
        public static bool Exists(this Projectile p) => p != null && p.active;
        public static bool Exists(this Item i) => i != null && !i.IsAir;
        public static bool ExistsInWorld(this Item i) => Exists(i) && i.active;
        /// <summary>
        /// WHY TF IS THIS INTERNAL IN VANILLA
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool IsTheSameAs(this Item i1, Item i2)
        {
            if (i1.netID == i2.netID)
            {
                return i1.type == i2.type;
            }
            return false;
        }
        //Make the invul boss part untargetable please
        public static bool IsValidTarget(this Projectile projectile, NPC target)
        {
            return target.CanBeChasedBy() && Collision.CanHit(projectile.Center, 1, 1, target.position, target.width, target.height);
        }
        public static T ModProjectile<T>(this Projectile projectile) where T : ModProjectile
        {
            return projectile.ModProjectile as T;
        }
        //Can do gimmick crap, take kb 
        public static bool Gimmickable(this NPC npc)
        {
            if (!npc.boss &&
                npc.knockBackResist > 0 &&
                !npc.friendly &&
                npc.CanBeChasedBy())
            {
                return true;
            }
            else
                return false;
        }
        public static bool Reflectable(this Projectile projectile)
        {
            if (projectile.aiStyle >=0 &&
                projectile.hostile &&
                projectile.active &&
                projectile.damage <= 5000 &&
                projectile.velocity != Vector2.Zero)
            {
                return true;
            }
            else
            return false;
        }
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2? fallback = null)
        {
            if (!fallback.HasValue)
                fallback = Vector2.Zero;

            return (destination - entity.Center).SafeNormalize(fallback.Value);
        }
        public static NPC NPCExists(int whoAmI, params int[] types)
        {
            return whoAmI > -1 && whoAmI < Main.maxNPCs && Main.npc[whoAmI].active && (types.Length == 0 || types.Contains(Main.npc[whoAmI].type)) ? Main.npc[whoAmI] : null;
        }

        public static NPC NPCExists(float whoAmI, params int[] types)
        {
            return NPCExists((int)whoAmI, types);
        }

        public static void ExpandHitboxBy(this Projectile projectile, int width, int height)
        {
            projectile.position = projectile.Center;
            projectile.width = width;
            projectile.height = height;
            projectile.position -= projectile.Size * 0.5f;
        }
        public static void ExpandHitboxBy(this Projectile projectile, int newSize) => projectile.ExpandHitboxBy(newSize, newSize);
        public static void ExpandHitboxBy(this Projectile projectile, Vector2 newSize) => projectile.ExpandHitboxBy((int)newSize.X, (int)newSize.Y);
        public static void ExpandHitboxBy(this Projectile projectile, float expandRatio) => projectile.ExpandHitboxBy((int)(projectile.width * expandRatio), (int)(projectile.height * expandRatio));
		
		public static void GetPointOnSwungItemPath(Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale, out Vector2 location, out Vector2 outwardDirection)
		{
			float scaleFactor = (float)Math.Sqrt((double)(spriteWidth * spriteWidth + spriteHeight * spriteHeight));
			float num = (float)(player.direction == 1).ToInt() * 1.57079637f;
			if (player.gravDir == -1f)
			{
				num += 1.57079637f * (float)player.direction;
			}
			outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy((double)(3.926991f + num), default);
			location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * scaleFactor * normalizedPointOnPath * itemScale, false, true);
		}
        /// <summary>
        /// <para>Retrieves a <see cref="LiquidPoolData"/> for a liquid pool given the coordinates of its surface.</para>
        /// i suck at coding
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="liquidType"></param>
        /// <returns></returns>
        public static LiquidPoolData ComputeLiquidPool(Point startPos, short liquidType, bool visualize = false)
        {
            List<LiquidStripData> strips = [];

            while (TileHelpers.TileLiquid(startPos, liquidType))
            {
                // find the real start position
                startPos.X--;
            }
            startPos.X++;
            Point query = startPos;
            while (true)
            {
                short thisStartIndex = 0;
                byte thisAmount = 0;

                while (TileHelpers.TileLiquid(query, liquidType))
                {
                    query.X--;
                    thisStartIndex--;
                }
                query.X++;
                thisStartIndex++;

                while (TileHelpers.TileLiquid(query, liquidType))
                {
                    query.X++;
                    thisAmount++;
                    if (visualize)
                    {
                        Dust d = Dust.NewDustPerfect(query.ToWorldCoordinates(), DustID.WhiteTorch, Vector2.Zero);
                        d.noGravity = true;
                    }
                }

                if (thisAmount > 0)
                    strips.Add(new LiquidStripData(thisStartIndex, thisAmount));

                query = new Point(startPos.X, query.Y + 1);
                if (!TileHelpers.TileLiquid(query, liquidType))
                    break;
            }
            return new LiquidPoolData([.. strips], new Point16(startPos.X, startPos.Y), (byte)liquidType);
        }
    }
    public readonly record struct LiquidStripData(short StartIndex, byte Amount);
    public readonly struct LiquidPoolData(LiquidStripData[] data, Point16 startCoords, byte type)
    {
        public readonly LiquidPoolData Empty => new();
        public readonly LiquidStripData[] Data = data;
        public readonly Point16 StartCoordinates = startCoords;
        public readonly byte Type = type;
        public Vector2 CenterAverage
        { 
            get
            {
                Vector2 sum = Vector2.Zero;
                int count = 0;

                for (int i = 0; i < Data.Length; i++)
                {
                    LiquidStripData d = Data[i];
                    for (ushort j = 0; j < d.Amount; j++)
                    {
                        sum += (StartCoordinates + new Point16(d.StartIndex + j, i)).ToWorldCoordinates();
                        count++;
                    }
                }

                return count > 0 ? sum / count : Vector2.Zero;
            } 
        }
    }
    public static class NPCTrailingID
    {
        public const int PosEveryThreeFrames = 0;
        public const int PosEveryFrame = 1;
        public const int PosRotEveryFrame = 3;
    }
}
