using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CicadarangProjectile : ModProjectile
    {

        public VertexStrip TrailStrip = new VertexStrip();
        public ref float Duration => ref Projectile.localAI[0];
        public ref float Stuck => ref Projectile.ai[0];
        public ref float OffsetX => ref Projectile.ai[1];
        public ref float OffsetY => ref Projectile.ai[2];

        public int cooldown = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
		
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 54; 
            Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
			Projectile.timeLeft = 150;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            cooldown = 0;
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(16, 16);
        }
		
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
			
			if (oldVelocity.X != Projectile.velocity.X) {
				Projectile.velocity.X = (0f - oldVelocity.X);
			}

			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.Y = (0f - oldVelocity.Y);
			}

            Projectile.ai[0] += 5;

            if (Main.rand.NextBool(3))
            {
                SoundStyle impact = new SoundStyle("ITD/Content/Sounds/CicadarangImpact") with
                {
                    Volume = 0.75f,
                    Pitch = 1f,
                    PitchVariance = 0.1f,
                    MaxInstances = 3,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                };
                SoundEngine.PlaySound(impact, Projectile.Center);
            }

            if (cooldown <= 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float speedX = -Projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-8f, 8f);
                    float speedY = -Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + speedX, Projectile.position.Y + speedY, speedX, speedY, ModContent.ProjectileType<CicadarangMiniStriker>(), Projectile.damage / 2, 0f, Projectile.owner);
                }

                cooldown = 5;
            }

            return false;
		}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                float speedX = -Projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-8f, 8f);
                float speedY = -Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + speedX, Projectile.position.Y + speedY, speedX, speedY, ModContent.ProjectileType<CicadarangMiniStriker>(), Projectile.damage / 2, 0f, Projectile.owner);
            }
        }
        public override void PostAI()
        {
			Projectile.ai[0]++;
			if (Projectile.ai[0] > 120)
			{
				Projectile.tileCollide = false;
                Projectile.timeLeft = 999;
				Player player = Main.player[Projectile.owner];
								
				float length = Projectile.velocity.Length();
                Projectile.velocity = Projectile.AngleTo(player.Center).ToRotationVector2() * length;

                if (player.Distance(Projectile.Center) < 32)
					Projectile.Kill();
			}

            if (cooldown > 0)
            {
                cooldown--;
                if (cooldown < 1)
                {
                    cooldown = 0;
                }
            }

			if (Projectile.ai[0] % 10 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
			}
			
			int dust = Dust.NewDust(Projectile.Center - new Vector2(16f, 16f), 32, 32, 135, 0f, 0f, 0, default, 2f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity = Projectile.velocity * 0.5f;

            Projectile.rotation += 0.4f * (float)Projectile.direction;
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

            GameShaders.Misc["LightDisc"].Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            return false;
        }
    }
}
