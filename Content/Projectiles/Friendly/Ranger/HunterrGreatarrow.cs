using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class HunterrGreatarrow : ModProjectile
    {
        public VertexStrip TrailStrip = new VertexStrip();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 80;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.aiStyle = 1; 
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0;
            AIType = ProjectileID.Bullet;
        }

        Vector2 spawnvel;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.extraUpdates += (int)Projectile.ai[0];
            switch (Projectile.ai[0])
            {
                case 0:
                    Projectile.penetrate = 1;
                    break;
                case 1:
                    Projectile.penetrate = 2;
                    Projectile.knockBack *= 1.25f;
                    break;
                case 2:
                    Projectile.penetrate = 3;
                    Projectile.knockBack *= 1.75f;
                    break;
                case 3:
                    Projectile.penetrate = -1;
                    Projectile.knockBack *= 2f;
                    break;
            }
        }
        public override void AI()
        {

                Projectile.rotation = Projectile.velocity.ToRotation();

            
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Collision.HitTiles(Projectile.position, new Vector2(-spawnvel.X / 2, -spawnvel.Y / 2), Projectile.width, Projectile.height);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.GoldCoin, -spawnvel.X / 2, -spawnvel.Y / 2, 60, default, Main.rand.NextFloat(1f, 1.7f));
                dust.noGravity = true;
                dust.velocity *= 4f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.t_Granite, -spawnvel.X / 2, -spawnvel.Y / 2, 60, default, Main.rand.NextFloat(1f, 1.2f));
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //maybe something else goes here, kinda busted
/*            if (Projectile.ai[0] == 3)
            {
                target.AddBuff(ModContent.BuffType<ToppledDebuff>(), 300);
            }*/
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.GoldCoin, Projectile.velocity.X / 1.5f, Projectile.velocity.Y / 1.5f, 60, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
                dust.velocity *= 5f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Granite, Projectile.velocity.X / 1.5f, Projectile.velocity.Y / 1.5f, 60, default, Main.rand.NextFloat(1f, 1.5f));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.GoldCoin, Projectile.velocity.X / 1.5f, Projectile.velocity.Y / 1.5f, 60, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
                dust.velocity *= 5f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Granite, Projectile.velocity.X / 1.5f, Projectile.velocity.Y / 1.5f, 60, default, Main.rand.NextFloat(1f, 1.5f));
            }
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(new Color(255, 198, 104),new Color(93, 44, 44), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * 0.5f;
        }
        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(11f, 9f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
                GameShaders.Misc["LightDisc"].Apply(null);
                TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
                TrailStrip.DrawTrail();
            
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle rectangle = texture.Frame(1, 1);
            Vector2 position = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, position, rectangle, lightColor, Projectile.rotation + MathHelper.PiOver2, rectangle.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}