using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Summoner;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class CicadianSentry : ModProjectile
    {   
		public ref float AttackTimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 116;
            Projectile.height = 46;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.sentry = true;
            Projectile.DamageType = DamageClass.Summon;
        }
		
		public override void AI()
        {
            Projectile.velocity.Y = 8f;

            NPC target;
			NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
			if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this, false))
				target = ownerMinionAttackTargetNPC;
            else
				target = Projectile.FindClosestNPC(900f);
            if (target != null)
			{
				AttackTimer = ++AttackTimer % 60;
				if (AttackTimer == 0)
				{
					SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
					Vector2 toPlayer = Vector2.Normalize(target.Center - Projectile.Center);
					for (int i = 0; i < 10; i++)
					{
						Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow);
						d.scale *= 1.5f;
						d.velocity = toPlayer - new Vector2(0, 1);
						d.noGravity = true;
					}
					if (Main.myPlayer == Projectile.owner)
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SummonIcyBoulder>(), Projectile.damage, 0.2f, Projectile.owner, target.whoAmI);
				}
            }
        }
		
		public override bool? CanDamage() => false;
		public override bool OnTileCollide(Vector2 oldVelocity) => false;
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
    }
}