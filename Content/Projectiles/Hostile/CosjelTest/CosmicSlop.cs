using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.CosjelTest
{
    public class CosmicSlop : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            CooldownSlot = 1;
            Projectile.scale = 1.25f;
            Projectile.alpha = 50;

            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 90 * (Projectile.extraUpdates + 1);
        }
        bool isStuck = false;
                public override bool OnTileCollide(Vector2 oldVelocity)
                {
                    if (Projectile.ai[1] != 0)
                    {
                        return true;
                    }
                    Projectile.velocity = Vector2.Zero;
                    isStuck = true;
                    return false;
                }
                public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
                {
                    NPC CosJel = Main.npc[(int)Projectile.ai[0]];
                    if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
                    {
                        Player player = Main.player[CosJel.target];
                        width = 15;
                        height = 15;
                        fallThrough = player.Center.Y >= Projectile.Bottom.Y + 20;
                    }
                    return true;

                }
        public override void AI()
        {
            if (isStuck)
            {
                if (Projectile.alpha++ >= 180)
                {
                    Projectile.Kill();
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() - (float)Math.PI / 2;  
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