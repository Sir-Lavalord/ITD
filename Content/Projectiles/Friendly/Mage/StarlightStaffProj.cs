using System;
using Terraria.Audio;
using ITD.Utilities;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class StarlightStaffProj : ModProjectile
    {
        public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
			.UseProjectionMatrix(true)
			.UseImage0("Images/Extra_" + 192)
			.UseImage1("Images/Extra_" + 194)
			.UseImage2("Images/Extra_" + 190)
			.UseSaturation(-4f)
			.UseOpacity(2f)
			.UseColor(Color.Black);

        public VertexStrip TrailStrip = new VertexStrip();
        public override string Texture => ITD.BlankTexture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
            if (Projectile.spriteDirection == 1)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;
            else
                Projectile.rotation = Projectile.velocity.ToRotation();
            float maxDetectRadius = 450f;

            if (Projectile.timeLeft > 30)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                    return;
                if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                {
                    HomingTarget = null;
                    return;
                }

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(4)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);
            }
            else
            {
                Projectile.velocity *= 0f;
                if (Projectile.timeLeft == 10)
                    SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
                if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 10)
                {
                    Projectile.Resize(200, 200);

                    Projectile.tileCollide = false;
                   
                    Projectile.position = Projectile.Center;
                    Projectile.Center = Projectile.position;
                }
            }
        }
        public override bool? CanDamage()
        {
            if (Projectile.timeLeft <= 10 || Projectile.timeLeft > 30)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > 30)
            {
                Projectile.timeLeft = 30;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			if (Projectile.oldPos[0] != new Vector2())
				Projectile.position = Projectile.oldPos[0];
			
            if (Projectile.timeLeft > 30)
            {
                Projectile.timeLeft = 30;
            }
            return false;
        }
        Color col;
        Color colTrail;
        Color colExplode1;
        Color colExplode2;
        Color colExplode3;

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.rand.NextBool(2))
            {
                Shader.UseImage0("Images/Extra_" + 191);
                col = new Color(255, 242, 191, 30);
                colTrail = new Color(255, 247, 0, 30);
                colExplode1 = new Color(35, 36, 12, 30);
                colExplode2 = new Color(133, 127, 50, 30);
                colExplode3 = new Color(255, 253, 191, 30);

            }
            else
            {
                Shader.UseImage0("Images/Extra_" + 192);
                col = new Color(168, 241, 255, 30);
                colTrail = new Color(0, 153, 255, 30);
                colExplode1 = new Color(12, 25, 36, 30);
                colExplode2 = new Color(50, 78, 133, 30);
                colExplode3 = new Color(191, 247, 255, 30);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        private Color StripColors(float progressOnStrip)
        {
            
            Color result = Color.Lerp(Color.White, colTrail, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * Projectile.Opacity * Projectile.Opacity;
        }
        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(30f, 40f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.1f, progressOnStrip, true);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D effectTexture = TextureAssets.Extra[98].Value;
            Vector2 effectOrigin = effectTexture.Size() / 2f;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float scaleX = 0.75f;
            float scaleY = 1f;
            Rectangle rectangle = texture.Frame(1, 1);
            Player player = Main.player[Projectile.owner];
            lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            if (Projectile.timeLeft > 30)
			{
                Shader.Apply(null);
                TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
                TrailStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, col, 0 - MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(effectTexture, drawPosition, null, col, 0, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
            }
            else if (Projectile.timeLeft > 10 && Projectile.timeLeft <= 30)
            {
                float scaleMultipler = (40 - Projectile.timeLeft) * 0.075f;
                float colorMultiplier = Math.Min(1, Projectile.timeLeft * 0.3f);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, col * colorMultiplier, scaleMultipler * 2f - MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY) * scaleMultipler * 1f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, col * colorMultiplier, scaleMultipler * 2.1f, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY) * scaleMultipler *1f, SpriteEffects.None, 0);
            }
            else if (Projectile.timeLeft <= 10)
            {
                Vector2 position = Projectile.Center - Main.screenPosition;
                Texture2D texture2 = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/WRipperRift").Value;
                Rectangle sourceRectangle = texture2.Frame(1, 1);
                Vector2 origin = sourceRectangle.Size() / 2f;

                float scaleMultipler = (20f - Projectile.timeLeft) * 0.1f;
                float colorMultiplier = Math.Min(1, Projectile.timeLeft * 0.1f);

                Main.EntitySpriteDraw(texture2, position, sourceRectangle, colExplode1 * colorMultiplier, scaleMultipler * 2f, origin, scaleMultipler * 2f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(texture2, position, sourceRectangle, colExplode2 * colorMultiplier, scaleMultipler * 1.5f, origin, scaleMultipler * 1.5f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(texture2, position, sourceRectangle, colExplode3 * colorMultiplier, scaleMultipler, origin, scaleMultipler, SpriteEffects.None, 0f);

            }

            return false;
        }
    }
}
