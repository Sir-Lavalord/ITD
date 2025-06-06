﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using ITD.Utilities;

namespace ITD.Particles.Projectile
{
    public class HoneyParticle : ParticleEmitter
    {
        public override void SetStaticDefaults()
        {
            ParticleSystem.particleUsesRenderTarget[type] = true;
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale *= 1.6f;
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale * particle.ProgressOneToZero;
            particle.velocity *= 0.95f;
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void PreDrawAllParticles()
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/HoneyParticle_Outline").Value;
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawCommon(Main.spriteBatch, tex, CanvasOffset);
            }
        }
    }
}
