using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

using Terraria.DataStructures;
namespace ITD.Content.Projectiles.Hostile.CosjelTest
{
    public class CosmicSwarm : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 54;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            CooldownSlot = 1;
            Projectile.scale = 1.25f;
            Projectile.alpha = 50;
            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 1200 * (Projectile.extraUpdates + 1);
        }
        bool isStuck = false;
        public override void AI()
        {
            if (Projectile.localAI[0]++ >= 120)
            {
                Projectile.velocity *= 0.9f;
            }
            if (++Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
                dust.velocity *= 2f;
                Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
                dust2.velocity *= 1f;
                dust2.noGravity = true;
            }
            Projectile.scale += Main.rand.NextFloat(0.5f, 1f);
        }
        public override void OnKill(int timeleft)
        {
            for (int i = 0; i < 20; i++)
            {
                int num469 = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.BlueTorch, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100, default, 2f);
                Main.dust[num469].noGravity = true;
                Main.dust[num469].velocity *= 2f;
                num469 = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.BlueTorch, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100);
                Main.dust[num469].velocity *= 2f;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
    }
}