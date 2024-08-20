using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Audio;

namespace ITD.Content.Projectiles
{
    public class ITDGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
		
		public bool quasarAura;
		public int timer = 0;
		
		public override void AI(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];
			if (quasarAura)
			{
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC target = Main.npc[i];
					if (!target.isLikeATownNPC && !target.HasBuff(BuffID.OnFire3) && target.Distance(projectile.Center) < 80)
					{
						target.AddBuff(BuffID.OnFire3, 60, false);
					}
				}
			}
			
			timer++;
		}
		
		public override bool PreDrawExtras(Projectile projectile)
		{
			Vector2 position = projectile.Center - Main.screenPosition;
			if (quasarAura)
			{
				Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Misc/WRipperRift").Value;
				Rectangle sourceRectangle = texture.Frame(1, 1);
				Vector2 origin = sourceRectangle.Size() / 2f;
			
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(36, 12, 34), timer*0.05f, origin, 2f, SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(133, 50, 88), timer*0.075f, origin, 1.5f, SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(249, 203, 151), timer*0.1f, origin, 1f, SpriteEffects.None, 0f);
			}
			
			return true;
		}
	}
}