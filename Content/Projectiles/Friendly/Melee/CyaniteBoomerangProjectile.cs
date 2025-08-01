﻿using System;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CyaniteBoomerangProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 32; Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
			Projectile.timeLeft = 150;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
        }
		
		public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(32, 32);
        }
		
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
			
			if (oldVelocity.X != Projectile.velocity.X) {
				Projectile.velocity.X = (0f - oldVelocity.X);
			}

			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.Y = (0f - oldVelocity.Y);
			}
			
			SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);
			return false;
		}

        public override void PostAI()
        {
			Projectile.ai[0]++;
			if (Projectile.ai[0] > 24)
			{
				Projectile.tileCollide = false;

				Player player = Main.player[Projectile.owner];
								
				float length = Projectile.velocity.Length();
                Projectile.velocity = Projectile.AngleTo(player.Center).ToRotationVector2() * length;
								
				if (player.Distance(Projectile.Center) < 32)
					Projectile.Kill();
			}
			if (Projectile.ai[0] % 10 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
			}
			
			int dust = Dust.NewDust(Projectile.Center - new Vector2(16f, 16f), 32, 32, 135, 0f, 0f, 0, default, 2f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity = Projectile.velocity * 0.5f;
        }
		
		public override bool PreDraw(ref Color lightColor) 
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texture2 = ModContent.Request<Texture2D>(Texture + "_Aura").Value;
			
			Rectangle rectangle = texture.Frame(1, 1);
			Rectangle rectangle2 = texture2.Frame(1, 1);
			
			Vector2 position = Projectile.Center - Main.screenPosition;
			
			float rotation = Projectile.rotation + Main.GlobalTimeWrappedHourly * 15f;
			
			Main.EntitySpriteDraw(texture2, position, new Rectangle?(rectangle2), new Color(120, 184, 255, 50), rotation*2f+0.1f, rectangle2.Size() / 2f, Projectile.scale*0.4f, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture2, position, new Rectangle?(rectangle2), new Color(120, 184, 255, 50), rotation*2f, rectangle2.Size() / 2f, Projectile.scale*0.4f, SpriteEffects.None, 0f);
			
			Player player = Main.player[Projectile.owner];
			float distanceMult = Math.Min(player.Distance(Projectile.Center)*0.005f-0.2f, 1f);
			Main.EntitySpriteDraw(texture2, position, new Rectangle?(rectangle2), new Color(120, 184, 255, 100) * distanceMult, rotation+0.1f, rectangle2.Size() / 2f, Projectile.scale*0.8f, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture2, position, new Rectangle?(rectangle2), new Color(120, 184, 255, 100) * distanceMult, rotation, rectangle2.Size() / 2f, Projectile.scale*0.8f, SpriteEffects.None, 0f);
						
			Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), lightColor, rotation, rectangle.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
    }
}
