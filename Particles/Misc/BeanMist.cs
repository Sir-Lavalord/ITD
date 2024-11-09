using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using ITD.Utilities;

namespace ITD.Particles.Misc
{
    public class BeanMist : ITDParticle
    {
        public override void SetDefaults()
        {
            canvas = ParticleDrawCanvas.WorldUnderProjectiles;
            scale *= 1.6f;
            timeLeft = 40;
        }
        public override void AI()
        {
            scale = spawnScale * ProgressOneToZero;
            velocity *= 0.95f;
        }
        public override Color GetAlpha() => Color.White;
        public void DrawOutline(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/BeanMist_Outline").Value;
            DrawCommon(spriteBatch, tex, CanvasOffset);
        }
        public override bool PreDraw(SpriteBatch spriteBatch)
        {
            if (!(tag is Projectile projectile && projectile.Exists()))
            {
                DrawOutline(spriteBatch);
                return true;
            }
            return false;
        }
    }
}
