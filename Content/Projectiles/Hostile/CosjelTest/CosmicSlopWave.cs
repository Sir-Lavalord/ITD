using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Hostile.CosjelTest
{

    public class CosmicSlopWave : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1800;
            Projectile.scale =1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            Projectile.velocity.X = 8 * (int)Projectile.ai[1];
            Projectile.spriteDirection = (int)Projectile.ai[1];
        }
        public override void AI()
        {
            if (Main.rand.NextBool(10))
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height
                    , DustID.WhiteTorch, Projectile.velocity.X * 1.2f, Projectile.velocity.Y * 1.2f, 40, default, 1.5f);   //this defines the flames dust and color, change DustID to wat dust you want from Terraria, or add mod.DustType("CustomDustName") for your custom dust
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.5f;
            }
        }
        public override void OnKill(int timeLeft)
        {

        }
    }
}