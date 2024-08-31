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
	public class ElectrumSpearProjectile : ModProjectile
	{
		protected virtual float HoldoutRangeMin => 24f;
		protected virtual float HoldoutRangeMax => 80f;

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.scale = 1.2f;
			Projectile.ownerHitCheck = true;
			Projectile.timeLeft = 20;
		}

		private Vector2 Holdout(Vector2 direction, int time)
		{
			Player player = Main.player[Projectile.owner];

			float stoppingPoint = 20 * 0.25f;
			float progress;
			if (time < stoppingPoint)
			{
				progress = time / stoppingPoint;
			}
			else
			{
				progress = (20 - time) / stoppingPoint;
			}
			
			return player.MountedCenter + Vector2.SmoothStep(direction * HoldoutRangeMin, direction * HoldoutRangeMax, progress);
		}

		public override bool PreAI()
		{
			if (Projectile.ai[0] == 1)
			{
				Player player = Main.player[Projectile.owner];
				player.heldProj = Projectile.whoAmI;
			}

			if (Projectile.timeLeft == 20)
			{
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity*4f, ModContent.ProjectileType<ElectrumSpearBeam>(), (int)(Projectile.damage * 1.25), Projectile.knockBack * 0.5f, Projectile.owner);
				SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
			}
			
			Vector2 direction = Vector2.Normalize(Projectile.velocity);

			Projectile.Center = Holdout(direction, Projectile.timeLeft);

			return false;
		}
		
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] == 1)
			{
				Player player = Main.player[Projectile.owner];
				
				Texture2D texture = TextureAssets.Projectile[Type].Value;
				Rectangle sourceRectangle = texture.Frame(1, 1);
				Vector2 origin = sourceRectangle.Size() / 6f;

				for (int i = 0; i < 3; i++)
				{
					Vector2 rotatedDirection = Vector2.Normalize(Projectile.velocity).RotatedBy(MathHelper.ToRadians(-15*i));
					int time = Projectile.timeLeft-i*5;
					if (time > 0)
					{
						Vector2 position = Holdout(rotatedDirection, time) - Main.screenPosition;
						float rotation;
						
						int direction = (rotatedDirection.X > 0).ToDirectionInt();
						rotation = rotatedDirection.ToRotation() + MathHelper.ToRadians(135f);
						
						Main.EntitySpriteDraw(texture, position, sourceRectangle, Lighting.GetColor((int)player.Center.X/16, (int)player.Center.Y/16), rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
					}
				}
			}
			return false;
		}
	}
}