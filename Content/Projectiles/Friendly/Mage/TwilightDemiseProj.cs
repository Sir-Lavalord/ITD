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
using ITD.Systems.DataStructures;
using ITD.Systems.Extensions;
using Humanizer;
using ITD.Content.Items.Dyes;
using ITD.Particles.Projectile;
using ITD.Particles;
namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class TwilightDemiseProj : ITDProjectile
    {
        public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);

        public VertexStrip TrailStrip = new VertexStrip();

        public int HomingTime;
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
        public ParticleEmitter emitter;

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

            Shader.UseImage0("Images/Extra_" + 192);
            Shader.UseImage1("Images/Extra_" + 194);
            Shader.UseImage2("Images/Extra_" + 193);
            Shader.UseSaturation(-2.8f);
            Shader.UseOpacity(2f);
            emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (emitter != null)
                emitter.keptAlive = true;
        }
        public override void AI()
        {
            if (emitter != null)
                emitter.keptAlive = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 1; i++)
                {
                    emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(15, 15), Vector2.Zero);
                }
            }
            float maxDetectRadius = 1000;
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
                else
                {
                    Projectile.scale *= 0.98f;

                    Projectile.alpha += 6;
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
        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.Black, Color.Purple, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * Projectile.Opacity;
        }
        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(36f, 60f, 1f);
        }
        public override void OnKill(int timeLeft)
        {

            for (int i = 0; i < 10; i++)
            {
                emitter?.Emit(Projectile.Center, (Vector2.UnitX * 3).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(0.5f, 1.5f));
            }

        }
        float innerScale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            void DrawAtProj(Texture2D tex)
            {
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            if (Projectile.timeLeft >= 20)
            {
                innerScale = 1f;
            }
            else
            {
                innerScale *= 0.96f;
            }
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
            if (Projectile.timeLeft >= 10)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(texture.Width * 0.5f, (texture.Height / Main.projFrames[Type]) * 0.5f), innerScale * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }

        public override int ProjectileShader(int originalShader)
        {
            return GameShaders.Armor.GetShaderIdFromItemId(ItemID.TwilightDye);
        }
    }
}
