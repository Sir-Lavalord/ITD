using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CicadarangMiniStriker : ModProjectile
    {

        public VertexStrip TrailStrip = new VertexStrip();
        public ref float Duration => ref Projectile.localAI[0];
        public ref float Stuck => ref Projectile.ai[0];
        public ref float OffsetX => ref Projectile.ai[1];
        public ref float OffsetY => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private int DelayTimer = 0;
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (15);
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }

        public override void AI()
        {
            Projectile.damage = 0;
            float maxDetectRadius = 800f;

            if (DelayTimer < 3)
            {
                DelayTimer += 1;
                return;
            }

            Projectile.damage = 10;

            if (Projectile.penetrate > 0)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    return;
                }
                else
                {

                }

                Vector2 directionToTarget = HomingTarget.Center - Projectile.Center;
                directionToTarget.Normalize();

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);

                Projectile.rotation = directionToTarget.ToRotation();
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 150, default(Color), 1.5f);
                }
                Projectile.Kill();
            }
        }

        public override void PostAI()
        {
			int dust = Dust.NewDust(Projectile.Center - new Vector2(16f, 16f), 32, 32, 135, 0f, 0f, 0, default, 2f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity = Projectile.velocity * 0.5f;
        }

        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.LightSkyBlue, Color.Blue, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * 0.5f;
        }

        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(8f, 6f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle rectangle = texture.Frame(1, 1);
            Vector2 position = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, position, rectangle, lightColor, Projectile.rotation, rectangle.Size() / 2f, 1f, SpriteEffects.None, 0f);

            MiscShaderData expr_0F = GameShaders.Misc["LightDisc"];
            expr_0F.UseSaturation(-2.8f);
            expr_0F.UseOpacity(2f);
            expr_0F.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            return false;
        }
    }
}
