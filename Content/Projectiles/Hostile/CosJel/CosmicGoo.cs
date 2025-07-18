﻿using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicGoo : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.width = Projectile.height = 20;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public ref float MaxTimeleft => ref Projectile.ai[1];
        public ref float ProjectileLaser => ref Projectile.ai[0];
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft = (int)MaxTimeleft;
        }

        public override bool PreDraw(ref Color lightColor)
        {



            return false;
        }





    }
}
