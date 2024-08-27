using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicJellyfishMiniProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //TODO: Animate the thing
            Main.projFrames[Projectile.type] = 1;
        }
    }
}

