using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace ITD.Particles.Testing
{
    public class ShaderTestParticle : ITDParticle
    {
        public override void SetStaticDefaults()
        {
            ParticleSystem.particleShaders[type] = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
        }
        public override void SetDefaults()
        {
            canvas = ParticleDrawCanvas.WorldUnderProjectiles;
            timeLeft = 120;
        }
        public override void AI()
        {
            velocity += Vector2.UnitY * 0.1f;
            rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}
