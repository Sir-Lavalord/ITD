
using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile
{
    public class TouhouBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Tofu");
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;

        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Projectile.velocity = (CosJel.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f;
            }
            else
            {
                Projectile.Kill();
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
        public override void AI()
        {
            {
                NPC CosJel = Main.npc[(int)Projectile.ai[0]];
                if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
                {
                    if (CosJel.Center == Projectile.Center)
                    {
                        Projectile.Kill();
                    }
                }
            }
        }
    }
}

