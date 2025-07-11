using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class CrowdControllerProj : ModProjectile
    {
        public VertexStrip TrailStrip = new VertexStrip();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
        }
        public bool startAnim;
        public override void AI()
        {
            if (startAnim)
            {
                if (Projectile.ai[1] <= 0)
                {
                    Projectile.ai[1]++;
                    Projectile.frame = 0;
                    if (Projectile.ai[0] == 0)
                    {
                        Projectile.rotation = 0;
                        Vector2 vel = new(18, 0);
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<CrowdControllerProj>(), (int)(Projectile.damage), Projectile.knockBack, Projectile.owner);
                        proj.scale = 0.75f;
                        proj.ai[0] = 1;
                        proj.penetrate = 4;
                        Projectile proj1 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, -vel,
                            ModContent.ProjectileType<CrowdControllerProj>(), (int)(Projectile.damage), Projectile.knockBack, Projectile.owner);
                        proj1.scale = 0.75f;
                        proj1.ai[0] = 1;
                        proj1.penetrate = 4;
                    
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0, 0, 0, default, 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 3f;
                    }
                }
                    Projectile.penetrate = -1;

                Projectile.Resize((int)(100 * Projectile.scale), (int)(100 * Projectile.scale));
                Projectile.timeLeft = 2;
                if (Projectile.frameCounter++ >= 3)
                {
                    Projectile.frameCounter = 0;

                    if (Projectile.frame++ >= 4)
                    {
                        Projectile.Kill();
                    }
                }
            }
            else
            {
                if (Main.rand.NextBool(5))
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0, 0, 0, default, 1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.frameCounter++ >= 8)
                {
                    Projectile.frameCounter = 0;

                    if (Projectile.frame++ >= 1)
                    {
                        Projectile.frame = 0;
                    }
                }
            }
        }
        public Vector2 spawnVel;
        public override void OnSpawn(IEntitySource source)
        {
            spawnVel = Projectile.velocity;

        }
        public override void OnKill(int timeLeft)
        {
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 10;
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.penetrate <= 2)
            {
                Projectile.velocity *= 0;
                startAnim = true;
            }
            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity *= 0;
                startAnim = true;
            }
/*            else
                Projectile.damage =(int)(Projectile.damage * 0.9f);*/
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0;
            startAnim = true;
            return false;
        }
        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.White, Color.Red, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * 0.5f;
        }
        private float StripWidth(float progressOnStrip)
        {
            return 14 * Projectile.scale;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 170, 90);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D explosionA = ModContent.Request<Texture2D>(Texture + "_ExplosionA").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Rectangle exFrameA = explosionA.Frame(1, 5, 0, Projectile.frame);
            GameShaders.Misc["LightDisc"].Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            if (startAnim)
                sb.Draw(explosionA, Projectile.Center - Main.screenPosition, exFrameA, Color.White, Projectile.rotation, new Vector2(explosionA.Width * 0.5f, (explosionA.Height / 5) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            else
            {
                sb.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(texture.Width * 0.5f, (texture.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                TrailStrip.DrawTrail();
            }

            return false;
        }
    }
}
