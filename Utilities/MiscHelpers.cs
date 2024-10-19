using System;
using System.Linq;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using ITD.Content.NPCs;
using ITD.Content.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ITD.Utilities
{
    public static class MiscHelpers
    {
        public static Entity[] EntityQuery<T>(this Entity entity, int maxAmount = -1, bool ignoreSelf = true, Func<Entity, bool> predicate = null) where T : Entity
        {
            Type entityType = typeof(T);

            Entity[] entities = entityType switch
            {
                Type t when t == typeof(NPC) => Main.npc,
                Type t when t == typeof(Projectile) => Main.projectile,
                Type t when t == typeof(Player) => Main.player,
                Type t when t == typeof(Item) => Main.item,
                _ => null
            };

            if (entities is null) return [];

            Entity[] result = entities
            .Where(e => e is T && predicate(e) && (!ignoreSelf || e.whoAmI != entity.whoAmI))
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
			
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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
				damage = Main.DamageVar(damage, player.luck);

				bool crit = false;
				if (Main.rand.Next(1, 101) <= critChance)
				{
					crit = true;
				}

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
			outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy((double)(3.926991f + num), default(Vector2));
			location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * scaleFactor * normalizedPointOnPath * itemScale, false, true);
		}
    }
}
