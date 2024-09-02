using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee
{
	public class CyaniteSpearProjectile : ModProjectile
	{
		protected virtual float HoldoutRangeMin => 24f;
		protected virtual float HoldoutRangeMax => 128f;
		protected virtual float StoppingPoint => 8f;

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.scale = 1.2f;
			Projectile.ownerHitCheck = true;
			Projectile.timeLeft = 20;
		}

		private float Progress(int time)
		{
			float progress;
			
			if (time < StoppingPoint)
			{
				progress = time / StoppingPoint;
			}
			else
			{
				progress = (20 - time) / StoppingPoint;
			}
			
			return progress;
		}

		private Vector2 Holdout(Vector2 direction, int time)
		{
			Player player = Main.player[Projectile.owner];
			return Vector2.SmoothStep(direction * HoldoutRangeMin, direction * HoldoutRangeMax, Progress(time));
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float num32 = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 100f * Projectile.scale, 96f * Projectile.scale, ref num32);
		}

		public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;
			
			Vector2 direction = Vector2.Normalize(Projectile.velocity);

			Projectile.Center = player.MountedCenter + Holdout(direction, Projectile.timeLeft);

			Projectile.rotation = direction.ToRotation();
			if (Projectile.spriteDirection == -1) {
				Projectile.rotation += MathHelper.ToRadians(45f);
			}
			else {
				Projectile.rotation += MathHelper.ToRadians(135f);
			}

			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 135, 0f, 0f, 0, default, 2f);
			Main.dust[dust].noGravity = true;
			
			if (Projectile.timeLeft < StoppingPoint)
			{
				Main.dust[dust].velocity = -Projectile.velocity;
			}
			else
			{
				Main.dust[dust].velocity = Projectile.velocity;
			}			

			return false;
		}
		
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			lightColor = Lighting.GetColor((int)player.Center.X/16, (int)player.Center.Y/16);
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle rectangle = texture.Frame(1, 1);
			
			Vector2 rotatedDirection = Vector2.Normalize(Projectile.velocity);

			Vector2 position = player.MountedCenter + Holdout(rotatedDirection, Projectile.timeLeft) - Main.screenPosition;
			
			Texture2D effectTexture = TextureAssets.Extra[98].Value;
			Vector2 effectOrigin = effectTexture.Size() / 2f;

			Main.EntitySpriteDraw(effectTexture, position, new Rectangle?(rectangle), new Color(120, 184, 255, 50)*0.05f*Projectile.timeLeft, Projectile.rotation, effectOrigin, new Vector2(1f, Progress(Projectile.timeLeft)*3.5f) * Projectile.scale, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(effectTexture, position, new Rectangle?(rectangle), new Color(120, 184, 255, 50)*0.05f*Projectile.timeLeft, Projectile.rotation - MathHelper.PiOver2, effectOrigin, new Vector2(1f, Progress(Projectile.timeLeft)*3.5f) * Projectile.scale, SpriteEffects.None, 0f);
			
			Main.EntitySpriteDraw(effectTexture, position, new Rectangle?(rectangle), new Color(120, 184, 255, 50)*0.05f*Projectile.timeLeft, Projectile.rotation - MathHelper.PiOver2 * 0.5f, effectOrigin, new Vector2(1f, Progress(Projectile.timeLeft)*5f) * Projectile.scale, SpriteEffects.None, 0f);
			
			Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), lightColor, Projectile.rotation, rectangle.Size() / 6f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}