using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class CyanideBullet : ModProjectile
    {
		private bool explosion = false;
		
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;

			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

            AIType = ProjectileID.Bullet;
        }
		
		private void Boom()
		{
			explosion = true;
			
			Projectile.timeLeft = 30;
			Projectile.velocity *= 0f;
			Projectile.tileCollide = false;
			
			Projectile.Resize(75, 75);
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
        {
			if (Projectile.oldPos[0] != new Vector2())
				Projectile.position = Projectile.oldPos[0];
			
			Boom();
			
            return false;
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			if (!explosion)
				Boom();
            target.AddBuff(BuffID.Frostburn2, 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
			Vector2 position = Projectile.Center - Main.screenPosition;
			
			Texture2D texture = TextureAssets.Extra[98].Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;

			if (explosion)
			{
				
				float scaleMultipler = (20f-Projectile.timeLeft*0.5f)*0.1f;
				float colorMultiplier = Math.Min(1, Projectile.timeLeft*0.1f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver4, origin, new Vector2(0.75f * scaleMultipler, 1.25f * scaleMultipler), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver2+MathHelper.PiOver4, origin, new Vector2(0.75f * scaleMultipler, 1.25f * scaleMultipler), SpriteEffects.None, 0f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f, origin, new Vector2(0.5f * scaleMultipler, 0.75f * scaleMultipler), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver2, origin, new Vector2(0.5f * scaleMultipler, 0.75f * scaleMultipler), SpriteEffects.None, 0f);
								
				return false;
			}

            return true;
        }
    }
}