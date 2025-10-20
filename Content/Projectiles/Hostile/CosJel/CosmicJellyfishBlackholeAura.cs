using System.Collections.Generic;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicJellyfishBlackholeAura : ModProjectile
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
        NPC npc = Main.npc[(int)Projectile.ai[0]];
        if (!npc.active)
            Projectile.Kill();

        Projectile.timeLeft = 10;

        Projectile.Center = npc.Center;

        Player player = Main.player[npc.target];
        float range = Projectile.Distance(player.Center);
        if (player.active && !player.dead && !player.ghost && range > 600f && range < 3000f)
        {
            player.AddBuff(BuffID.Obstructed, 2, false);
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);

        overWiresUI.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(235, 186, 123, 0) * 0.5f, Main.GlobalTimeWrappedHourly, texture.Size() / 2, 4f, SpriteEffects.None);


        return false;
    }

}
