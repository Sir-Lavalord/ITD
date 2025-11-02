using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using ITD.Utilities.EntityAnim;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicJellyfishBlast : BigBlankExplosion
{
    public override int Lifetime => 150;
    public override Vector2 ScaleRatio => new(1.5f, 1f);

    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.White * 1.6f, Color.Purple, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
    }
    public override string Texture => ITD.BlankTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
    }
    public float ProgressZeroToOne => Utils.GetLerpValue(Lifetime, 0f, Projectile.timeLeft, true);

    public override void OnSpawn(IEntitySource source)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);

        }
    }
    public override void AI()
    {
        if (CurrentRadius >= MaxRadius * 0.99f)
            Projectile.Kill();
        CurrentRadius = MathHelper.Lerp(CurrentRadius, MaxRadius, EasingFunctions.OutQuad(ProgressZeroToOne));
        Projectile.scale = MathHelper.Lerp(1.2f, 5f, EasingFunctions.OutQuad(ProgressZeroToOne));
        Projectile.ExpandHitboxBy((int)(CurrentRadius * Projectile.scale), (int)(CurrentRadius * Projectile.scale));
    }
    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);
    }
    public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
}