using ITD.Players;
using Terraria;
using ITD.Utilities;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class WRipperSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 5;

        }
        public bool bFirstHit;
        public bool bPeakSwing;
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 94; Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 2000;//make the anim plays out then kill
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 15;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.1f;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
            Projectile.noEnchantmentVisuals = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
            Projectile.scale *= 1.05f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - (Projectile.alpha / 255f));
        }
		
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)//make it once per swing
        {
			Player player = Main.player[Projectile.owner];
            ITDPlayer modPlayer = player.GetITDPlayer();
            if (!bFirstHit)
            {
                if (modPlayer.itemVar[0] < 3)
                {
                    if (modPlayer.itemVar[0] == 2)
                    {
                        SoundStyle wRipperCharge = new SoundStyle("ITD/Content/Sounds/WRipperCharge");
                        SoundEngine.PlaySound(wRipperCharge, player.Center);
                    }
                    modPlayer.itemVar[0]++;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bFirstHit = true;
        }
        public override bool? CanDamage()
        {
                return true;
        }
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
            // mirror why'd you comment this part out, it's meant to be the sword slashing

            // It was hard-coded (11- projectile.timeleft) part, it's supposed to play out its anim before dying
            // This will make do for now
			Projectile.Center = player.MountedCenter + Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (!bPeakSwing)
            {
                if (Projectile.alpha <= 0)
                {
                    bPeakSwing = true;
                }
                Projectile.alpha -= 5;
            }
            else
            {
                if (Projectile.frame <= 3)
                {
                    Projectile.alpha += 4;
                }
                else Projectile.alpha += 8;
                if (Projectile.alpha > 180)
                {
                    Projectile.Kill();
                }
            }
                Projectile.velocity *= 1.1f;
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {

                    Projectile.Kill();
                }
            }
            if (Projectile.frame == 2)//Arkhalis dusting thing
            {
                if (Main.rand.NextBool(2))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.Purple, Main.rand.NextFloat(1f, 1.4f));
                        Dust dust = Main.dust[dustIndex];
                        dust.noGravity = true;
                    }
                }
            }
        }
    }
}
