using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class AsterBlasterBlast : BigBlankExplosion
{
    public override int Lifetime => 26;
    public override Vector2 ScaleRatio => new(1.5f, 1f);

    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.Turquoise * 1.6f, Color.DarkViolet, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
    }
    public override string Texture => ITD.BlankTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.ignoreWater = true;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void OnKill(int timeLeft)
    {
        if (Main.myPlayer == Projectile.owner)
        {
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraLoading"), Projectile.Center);
        }
    }
    public override bool? CanDamage()
    {
        return CurrentRadius <= MaxRadius / 0.8f;
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        Projectile.Center = player.Center;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile other = Main.projectile[i];

            if (i != Projectile.whoAmI &&
                other.Reflectable()
                && Math.Abs(Projectile.Center.X - other.position.X)
                + Math.Abs(Projectile.Center.Y - other.position.Y) < CurrentRadius * 2.5f)
            {
                if (!Main.dedServ)
                {
                    other.GetGlobalProjectile<FishbackerReflectedProj>().IsReflected = true;
                    other.owner = Main.myPlayer;
                    other.velocity.X *= -4f;
                    other.velocity.Y *= -1f;

                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.WallOfFleshGoatMountFlames, new ParticleOrchestraSettings
                    {
                        PositionInWorld = other.Center,
                    }, other.whoAmI);

                    other.friendly = true;
                    other.hostile = false;
                    other.damage *= 10;
                    other.netUpdate = true;
                }
            }
        }
        base.AI();
    }
    public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
}