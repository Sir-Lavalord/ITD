using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Utilities;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class TwilightDemiseProj : ModProjectile
    {
        public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);

        public VertexStrip TrailStrip = new VertexStrip();
        public override string Texture => ITD.BlankTexture;

        public int HomingTime;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
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
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.alpha = 0;
            Projectile.Opacity = 1;

        }
        public override void AI()
        {
            Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
            if (Projectile.spriteDirection == 1)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;
            else
                Projectile.rotation = Projectile.velocity.ToRotation();
            float maxDetectRadius = 20000;
            if (HomingTime++ >= 2)
            {
                if (Projectile.timeLeft >= 20)
                {


                    HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                    if (HomingTarget == null)
                        return;
                    if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                    {
                        HomingTarget = null;
                        return;
                    }
                    int inertia = 20;
                    int vel = 14;
                    float length = Projectile.velocity.Length();
                    float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                    Vector2 homeDirection = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = (Projectile.velocity * inertia + homeDirection * vel) / (inertia + 1f);
                }
                }
            }
        public override bool? CanDamage()
        {
            if (Projectile.timeLeft > 20 && HomingTime >= 2)
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
            Projectile.velocity *= 0f;

            if (Projectile.timeLeft > 20)
            {
                Projectile.timeLeft = 20;
            }
        }
        Color col;
        Color colTrail;
        Color colExplode1;
        Color colExplode2;
        Color colExplode3;

        public override void OnSpawn(IEntitySource source)
        {

                Shader.UseImage0("Images/Extra_" + 195);
                Shader.UseColor(Color.Red);
                col = new Color(255, 242, 191, 30);
                colTrail = new Color(255, 247, 0, 30);
                colExplode1 = new Color(35, 36, 12, 30);
                colExplode2 = new Color(133, 127, 50, 30);
                colExplode3 = new Color(255, 253, 191, 30);

            
        }
        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.DeepSkyBlue, Color.Purple, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * Projectile.Opacity;
        }
        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(60f, 50f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.1f, progressOnStrip, true) * 1.2f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D effectTexture = TextureAssets.Extra[59].Value;
            Vector2 effectOrigin = effectTexture.Size() / 2f;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float scaleX = 0.75f;
            float scaleY = 1f;
            Rectangle rectangle = texture.Frame(1, 1);
            Player player = Main.player[Projectile.owner];
            lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            if (Projectile.timeLeft > 20)
            {
                GameShaders.Misc["LightDisc"].Apply(null);
                TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth , Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
                TrailStrip.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(0, 0, 0, 0), 0 - MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY) * 0.75f, SpriteEffects.None, 0);
            }
            else if (Projectile.timeLeft <= 20)
            {
                float scaleMultipler = (30 - Projectile.timeLeft) * 0.075f;
                float colorMultiplier = Math.Min(1, Projectile.timeLeft * 0.3f);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(0,0,0, 0) * colorMultiplier, scaleMultipler * 2f - MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY) * scaleMultipler * 0.75f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
