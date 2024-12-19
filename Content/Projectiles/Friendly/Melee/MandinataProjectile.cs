using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ITD.Utilities;
using static tModPorter.ProgressUpdate;
using Terraria.ID;
using Terraria.DataStructures;
using Mono.Cecil;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Friendly.Melee;
using Terraria.Audio;
using ITD.Content.Projectiles.Hostile;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class MandinataProjectile : ModProjectile
    {
        protected virtual float HoldoutRangeMin => 24f;
        protected virtual float HoldoutRangeMax => 125f;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Spear);
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner]; 
            int duration = player.itemAnimationMax;

            player.heldProj = Projectile.whoAmI; 

            if (Projectile.timeLeft > duration)
            {
                Projectile.timeLeft = duration;
            }

            Projectile.velocity = Vector2.Normalize(Projectile.velocity); 

            float halfDuration = duration * 0.5f;
            float progress;

            if (Projectile.timeLeft < halfDuration)
            {
                progress = Projectile.timeLeft / halfDuration;
            }
            else
            {
                progress = (duration - Projectile.timeLeft) / halfDuration;
            }

            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += MathHelper.ToRadians(45f);
            }
            else
            {
                Projectile.rotation += MathHelper.ToRadians(135f);
            }

            return false;
        }
    }
}