using ITD.Particles;
using ITD.Particles.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile
{
    public class PyroclasticFireball : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;
        public ParticleEmitter emitter;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.width = Projectile.height = 24;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            emitter = ParticleSystem.NewEmitter<PyroclasticParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.additive = true;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3());
            emitter.keptAlive = true;
            emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 3, Projectile.height / 3), -Projectile.velocity / 32f);
            base.AI();
        }
    }
}
