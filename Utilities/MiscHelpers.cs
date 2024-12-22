using System;
using System.Linq;
using Terraria.ID;
using Terraria;
using ITD.Content.NPCs;
using ReLogic.Utilities;
using Microsoft.Xna.Framework.Graphics;

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
        public static Vector2D ToRotationVector2D (this double d) => new(Math.Cos(d), Math.Sin(d));
        public static Vector2D ToRotationVector2D (this float f) => ToRotationVector2D(f);
        public static Vector3 ToVector3(this Vector2 v) => new(v.X, v.Y, 0);
        public static bool Exists(this NPC n) => n != null && n.active;
        public static bool Exists(this Projectile p) => p != null && p.active;
        public static bool Exists(this Item i) => i != null && i.active && !i.IsAir;
        //Make the invul boss part untargetable please
        public static bool IsValidTarget(this Projectile projectile, NPC target)
        {
            return target.CanBeChasedBy() && Collision.CanHit(projectile.Center, 1, 1, target.position, target.width, target.height);
        }
        public static void CreateLightningEffects(Vector2 start, Vector2 end)
        {
            Vector2 direction = Vector2.Normalize(end - start);
            float divisions = 8f;
            int length = (int)((end - start).Length() / divisions);

            for (int j = 0; j < length; j++)
            {
                Vector2 dustPos = start + direction * j * divisions + Main.rand.NextVector2Circular(16f, 16f);

                Dust dust = Dust.NewDustPerfect(
                    dustPos,
                    DustID.Electric,
                    Vector2.Zero,
                    0,
                    Color.White,
                    Main.rand.NextFloat(1.5f, 2f)
                );
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
        }
        //Reflectable
        public static bool Reflectable(this Projectile projectile)
        {
            if (projectile.aiStyle >=0 &&
                projectile.hostile &&
                projectile.active &&
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

        public static void Zap(Vector2 origin, Player player, int damage, int critChance, int chain)
        {
			NPC target = null;
			float reach = 300;
			
			foreach (var npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(npc.Center, origin);
					if (distance < reach && !npc.GetGlobalNPC<ITDGlobalNPC>().zapped)
					{
						reach = distance;
						target = npc;
					}
                }
            }
			if (target != null)
			{
				bool crit = false;
				if (Main.rand.Next(1, 101) <= critChance)
				{
					crit = true;
					damage *= 2;
				}
				damage = Main.DamageVar(damage, player.luck);
				
				target.StrikeNPC(new NPC.HitInfo
				{
					Damage = damage,
					Knockback = 1f,
					HitDirection = target.Center.X < origin.X ? -1 : 1,
					Crit = crit
				});

				target.GetGlobalNPC<ITDGlobalNPC>().zapped = true;

				CreateLightningEffects(origin, target.Center);
				if (chain > 0)
				{
					origin = target.Center;
					damage = (int)(damage*0.75f);
					chain--;
					Zap(origin, player, damage, critChance, chain);
				}
			}
        }
		
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
    }
    public static class TrailingModeID
    {
        public static class NPCTrailing
        {
            public const int PosEveryThreeFrames = 0;
            public const int PosEveryFrame = 1;
            public const int PosRotEveryFrame = 3;
        }
    }
}
