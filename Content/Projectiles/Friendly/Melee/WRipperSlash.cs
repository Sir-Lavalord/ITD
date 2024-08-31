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
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 100; Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 2000;//make the anim plays out then kill
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 40;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.1f;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
            Projectile.noEnchantmentVisuals = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
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
            if (Projectile.alpha < 180)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
            //Don't hardcode guys
/*			Projectile.Center = player.MountedCenter + Projectile.velocity * (11-Projectile.timeLeft);
*/            Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + 1.57f;
            Projectile.alpha += 5;
            Projectile.velocity.X *= 0.96f;
            Projectile.velocity.Y *= 0.96f;
            if (Projectile.alpha > 180)
            {
                Projectile.Kill();
            }
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 8)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {

                    Projectile.Kill();
                }
            }
/*            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 1; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height / 3, DustID.SilverCoin, 0, 0, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                    Dust dust = Main.dust[dustIndex];
                    dust.noGravity = true;
                }
            }*/
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = Color.Crimson;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}
