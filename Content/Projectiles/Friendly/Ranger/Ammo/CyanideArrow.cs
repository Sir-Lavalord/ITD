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
    public class CyanideArrow : ModProjectile
    {
		private bool explosion = false;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(1); // assume wooden arrow identity
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
        }

		private void Boom()
		{
			explosion = true;
			
			Projectile.timeLeft = 15;
			Projectile.velocity *= 0f;
			Projectile.tileCollide = false;
			
			Projectile.Resize(100, 100);
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
				
				float scaleMultipler = (20f-Projectile.timeLeft)*0.1f;
				float colorMultiplier = Math.Min(1, Projectile.timeLeft*0.2f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver4, origin, new Vector2(scaleMultipler, 1.5f * scaleMultipler), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver2+MathHelper.PiOver4, origin, new Vector2(scaleMultipler, 1.5f * scaleMultipler), SpriteEffects.None, 0f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f, origin, new Vector2(0.75f * scaleMultipler, scaleMultipler), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50)*colorMultiplier, scaleMultipler*2f+MathHelper.PiOver2, origin, new Vector2(0.75f * scaleMultipler, scaleMultipler), SpriteEffects.None, 0f);
								
				return false;
			}
			else
			{
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50), Main.GlobalTimeWrappedHourly*2f, origin, 1f*Main.essScale, SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50), Main.GlobalTimeWrappedHourly*2f+MathHelper.PiOver2, origin, 1f*Main.essScale, SpriteEffects.None, 0f);
			}

            return true;
        }
    }
}