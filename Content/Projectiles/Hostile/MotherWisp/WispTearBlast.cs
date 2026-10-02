using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using ITD.Utilities.EntityAnim;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispTearBlast : BigBlankExplosion
{
    public override int Lifetime => 45;
    public override Vector2 ScaleRatio => new(1.5f, 1f);

    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.White * 1.6f, new Color(131, 255, 236), MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

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

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<WispSharpTear>() && p.ai[1] == 0f)
                {
                    if (Vector2.Distance(Projectile.Center, p.Center) <= Projectile.width / 3f)
                    {
                        p.ai[1] = 1f;
                        p.netUpdate = true;
                    }
                }
            }
        }
        base.AI();
    }

    public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.5f);
}