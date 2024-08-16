using ITD.Players;
using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class WRipperSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 100; Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 10;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
		
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            ITDPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<ITDPlayer>();
			if (modPlayer.itemVar[0] < 3)
			{
				modPlayer.itemVar[0]++;
			}
        }
		
        public override void AI()
        {
            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
				
			Player player = Main.player[Projectile.owner];
			Projectile.Center = player.MountedCenter + Projectile.velocity * (11-Projectile.timeLeft);
			if (Projectile.spriteDirection == 1)
				Projectile.rotation = Projectile.velocity.ToRotation();
			else
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2*2;
        }
    }
}
