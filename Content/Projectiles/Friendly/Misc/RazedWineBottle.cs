using ITD.Content.Buffs.Debuffs;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class RazedWineBottle : ModProjectile
{
    float progress = 0f;
    NPC travelTarget = null;
    Vector2 start = Vector2.Zero;
    private const int duration = 42;
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.height = Projectile.width = 14;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = duration;
        Projectile.scale = 1.1f;
    }
    public override void OnSpawn(IEntitySource source)
    {
        start = Projectile.Center;
        travelTarget = Main.npc[(int)Projectile.ai[0]];
    }
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
        int gore0 = Mod.Find<ModGore>("RazedWineGore0").Type;
        int gore1 = Mod.Find<ModGore>("RazedWineGore1").Type;
        for (int i = 0; i < 10; i++)
        {
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SomethingRed);
            d.scale *= 1.5f;
        }
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, new Vector2(-2f, -2f), gore0);
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, new Vector2(2f, -2f), gore1);
    }
    public override void AI()
    {
        Projectile.rotation += Math.Sign(travelTarget.position.X - Projectile.position.X) / 4f;
        progress = 1f - (Projectile.timeLeft / (float)duration);
        Projectile.Center = Vector2.Lerp(start, travelTarget.Center, progress) - new Vector2(0f, (float)Math.Sin(progress * Math.PI) * 128f);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.whoAmI == travelTarget.whoAmI)
        {
            target.AddBuff<ToastedBuff>(120);
            Projectile.Kill();
        }
    }
    public override bool? CanHitNPC(NPC target)
    {
        return target.whoAmI == travelTarget.whoAmI;
    }
}
