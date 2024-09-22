using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Utilities;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Humanizer;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class BloodPactCut : ModProjectile
    {
        private int cutCounter;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            cutCounter = 0;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void AI()
        {
            Projectile.velocity *= 1f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
            cutCounter++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            float scaleX = 1f / (1 + cutCounter * 0.1f); 
            float scaleY = 1f * (1 + cutCounter * 0.1f);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float randomFloat = Main.rand.Next(0, 100);

            Texture2D tex = TextureAssets.Extra[98].Value;

            Main.EntitySpriteDraw(tex, drawPosition, null, lightColor, randomFloat, tex.Size()/2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
            return false;
        }
    }
}
