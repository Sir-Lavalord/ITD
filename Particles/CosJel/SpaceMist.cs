using ITD.Content.Projectiles.Unused;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Particles.CosJel
{
    public class SpaceMist : ITDParticle
    {
        public override void SetDefaults()
        {
            canvas = ParticleDrawCanvas.WorldUnderProjectiles;
            scale *= 1.2f;
            timeLeft = 120;
        }
        public override void AI()
        {
            scale = spawnScale * ProgressOneToZero;
            velocity *= 0.95f;
        }
        public void DrawOutline(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/SpaceMist_Outline").Value;
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
