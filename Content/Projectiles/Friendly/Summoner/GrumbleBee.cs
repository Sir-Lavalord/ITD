using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class GrumbleBee : ModProjectile
    {
        public NPC Target;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.CloneDefaults(ProjectileID.Hornet);
            Projectile.scale = 0.75f;
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 16; ++j)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bee, 0, 0, 0, default, 1f);
            }
        }
    }
}
