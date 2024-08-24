using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Hostile
{
    public class SandSpike : ModProjectile
    {
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
			Projectile.height = 32;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
        }
		
		public override void AI()
        {
            int dustType = 0;
			int num2 = 5;
			int num3 = 5;
			int num4 = 0;
			int num5 = 0;
			int num6 = 20;
			int num7 = 20;
			int num8 = 30;
			int maxValue = 6;
			bool flag = Projectile.ai[0] < (float)num6;
			bool flag2 = Projectile.ai[0] >= (float)num7;
			bool flag3 = Projectile.ai[0] >= (float)num8;
			Projectile.ai[0] += 1f;
			Projectile.position -= Projectile.velocity;
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[0] = 1f;
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.frame = Main.rand.Next(maxValue);
				for (int i = 0; i < num2; i++)
				{
					Dust expr_148 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
				}
				for (int j = 0; j < num3; j++)
				{
					Dust expr_21B = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
					expr_21B.fadeIn = 1f;
				}
				SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
			}
			if (flag)
			{
				Projectile.Opacity += 0.1f;
				Projectile.scale = Projectile.Opacity * Projectile.ai[1];
				for (int k = 0; k < num4; k++)
				{
					Dust expr_33F = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
				}
			}
			if (flag2)
			{
				Projectile.Opacity -= 0.2f;
				for (int l = 0; l < num5; l++)
				{
					Dust expr_429 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
				}
			}
			if (flag3)
			{
				Projectile.Kill();
			}
        }
		
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float num32 = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 200f * Projectile.scale, 22f * Projectile.scale, ref num32);
		}
		
		public override void OnKill(int timeLeft)
        {
			for (float num19 = 0f; num19 < 1f; num19 += 0.025f)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitY) * num19 * 200f * Projectile.scale, 0, new Vector2?(Main.rand.NextVector2Circular(3f, 3f)), 0, default(Color), 1f);
				dust.velocity.Y *= 0.2f;
				dust.velocity += Projectile.velocity * 0.2f;
			}
        }
		
		public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
			Microsoft.Xna.Framework.Rectangle rectangle11 = texture.Frame(1, 6, 0, Projectile.frame, 0, 0);
			Vector2 origin10 = new Vector2(16f, (float)(rectangle11.Height / 2));
			Microsoft.Xna.Framework.Color alpha4 = Projectile.GetAlpha(lightColor);
			Vector2 scale10 = new Vector2(Projectile.scale);
			float expr_A5D0 = 35f;
			float lerpValue4 = Utils.GetLerpValue(expr_A5D0, expr_A5D0 - 5f, Projectile.ai[0], true);
			scale10.Y *= lerpValue4;
			Vector4 value25 = lightColor.ToVector4();
			Vector4 vector35 = new Microsoft.Xna.Framework.Color(67, 17, 17).ToVector4();
			vector35 *= value25;
			//Main.EntitySpriteDraw(TextureAssets.Extra[98].get_Value(), Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) - Projectile.velocity * Projectile.scale * 0.5f, null, Projectile.GetAlpha(new Microsoft.Xna.Framework.Color(vector35.X, vector35.Y, vector35.Z, vector35.W)) * 1f, Projectile.rotation + 1.57079637f, TextureAssets.Extra[98].get_Value().Size() / 2f, Projectile.scale * 0.9f, spriteEffects, 0f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle11), alpha4, Projectile.rotation, origin10, scale10, SpriteEffects.None, 0f);

            return false;
        }
    }
}
