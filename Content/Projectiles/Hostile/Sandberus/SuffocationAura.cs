﻿using System.Collections.Generic;

namespace ITD.Content.Projectiles.Hostile.Sandberus
{
    public class SuffocationAura : ModProjectile
    {
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
        }
		
		public override void AI()
        {
			NPC npc = Main.npc[(int)(Projectile.ai[0])];
			if (!npc.active)
				Projectile.Kill();
			
			Projectile.timeLeft = 10;
			
			Projectile.Center = Vector2.Lerp(Projectile.Center, npc.Center, 0.025f);
			
			Player player = Main.LocalPlayer;
			float range = Projectile.Distance(player.Center);
			if (player.active && !player.dead && !player.ghost && range > 500f && range < 3000f)
			{
				player.AddBuff(BuffID.Suffocation, 2, false);
			}
        }
		
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overWiresUI.Add(index);
        }
		
		public override bool PreDraw(ref Color lightColor)
        {
			Texture2D auraTexture = ModContent.Request<Texture2D>(Texture).Value;
			Main.EntitySpriteDraw(auraTexture, Projectile.Center - Main.screenPosition, null, new Color(235, 186, 123, 0) * 0.5f,  Main.GlobalTimeWrappedHourly, auraTexture.Size() / 2, 2f, SpriteEffects.None);
			
            return false;
        }
    }
}
