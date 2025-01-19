using ITD.Content.Items.Dyes;
using ITD.Content.Projectiles.Unused;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Particles.CosJel
{
    public class SpaceMist : ITDParticle
    {
        public override void SetDefaults()
        {
            canvas = ParticleDrawCanvas.WorldUnderProjectiles;
            scale *= 1.3f;
            timeLeft = 60;
        }
        public override void AI()
        {
            scale = spawnScale * ProgressOneToZero;
            velocity *= 0.95f;
        }
        public override Color GetAlpha() => Color.White;
        public void DrawOutline(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/SpaceMist_Outline").Value;

            Color color1 = new Color(255, 242, 191);
            Color color2 = new Color(168, 241, 255);
            DrawCommon(spriteBatch, tex, CanvasOffset, color: Color.Lerp(color1, color2, Utils.PingPongFrom01To010(MathHelper.Lerp(0,1, (float)timeLeft / 60f))));
        }
        public override bool PreDraw(SpriteBatch spriteBatch)
        {
            if (!(tag is NPC npc && npc.Exists()))
            {
                DrawOutline(spriteBatch);
                return true;
            }
            return false;
        }
    }
}
