using System.Collections.Generic;
using System;

namespace ITD.Content.Projectiles.Hostile.Sandberus;

public class Dustnado : ModProjectile
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
		Projectile.Opacity = 0f;
    }

    public override void AI()
    {
        NPC npc = Main.npc[(int)Projectile.ai[0]];
        if (!npc.active)
            Projectile.ai[2] = 1f;
		
		if (Projectile.ai[2] == 0f && Projectile.Opacity < 1f)
			Projectile.Opacity += 0.1f;
		if (Projectile.ai[2] > 0f)
		{
			Projectile.Opacity -= 0.1f;
			if (Projectile.Opacity <= 0f)
				Projectile.Kill();
		}
		
        Projectile.timeLeft = 10;

        Projectile.Center = Vector2.Lerp(Projectile.Center, new Vector2(Projectile.Center.X, npc.Center.Y), 0.025f);

        Player player = Main.LocalPlayer;
        if (player.active && !player.dead && !player.ghost && (player.Center.X - Projectile.Center.X) * Projectile.ai[1] > 0f && (player.Center.X - Projectile.Center.X) * Projectile.ai[1] < 3000f)
        {
            player.AddBuff(BuffID.Suffocation, 2, false);
			player.AddBuff(BuffID.Obstructed, 2, false);
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overWiresUI.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
		int frameHeight = texture.Height / Main.projFrames[Projectile.type];
		for (int k = 0; k < 22; k++)
        {
			Rectangle frameRect = new(0, frameHeight * ((k + (int)(Main.GlobalTimeWrappedHourly * 16)) % Main.projFrames[Projectile.type]), texture.Width, frameHeight);
			const float scale = 1.8f;
			Vector2 drawPos = Projectile.Center + new Vector2(0, (frameHeight - 2) * scale * (k - 10.5f));
			Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, frameRect, Lighting.GetColor(drawPos.ToTileCoordinates()) * Projectile.Opacity * 0.5f, Projectile.rotation, new Vector2(texture.Width / 2, frameHeight / 2), new Vector2(scale * 1.5f + (float)Math.Sin(k + Main.GlobalTimeWrappedHourly) * 0.35f, scale), SpriteEffects.None);
		}
		
		for (int k = 0; k < 24; k++)
        {
			Rectangle frameRect = new(0, frameHeight * ((k + (int)(Main.GlobalTimeWrappedHourly * 16)) % Main.projFrames[Projectile.type]), texture.Width, frameHeight);
			const float scale = 1.6f;
			Vector2 drawPos = Projectile.Center + new Vector2(0, (frameHeight - 4) * scale * (k - 11.5f));
			Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, frameRect, Lighting.GetColor(drawPos.ToTileCoordinates()) * Projectile.Opacity, Projectile.rotation, new Vector2(texture.Width / 2, frameHeight / 2), new Vector2(scale * 1.5f + (float)Math.Sin(k + Main.GlobalTimeWrappedHourly) * 0.35f, scale), SpriteEffects.None);
		}

        return false;
    }
}
