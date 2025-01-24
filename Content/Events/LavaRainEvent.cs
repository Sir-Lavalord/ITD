using ITD.Content.NPCs.Events.LavaRain;
using ITD.Particles;
using ITD.Particles.Events;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Events
{
    public class LavaRainEvent : ITDEvent
    {
        public float Progress = 0f;
        public ParticleEmitter rainParticleEmitter;
        public override void OnActivate()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            rainParticleEmitter = ParticleSystem.NewEmitter<LavaRainParticle>();
            // if this is not set the emitter automatically dies which we don't want
            rainParticleEmitter.keptAlive = true;
        }
        public override void VisualsUpdate(Player player)
        {
            rainParticleEmitter.keptAlive = true;
            if (player.whoAmI != Main.myPlayer)
                return;
            float rangePadding = 100f;
            if (Main.rand.NextBool(2))
            {
                Vector2 emitPosition = (Main.screenPosition - Vector2.UnitX * rangePadding) + new Vector2(Main.rand.NextFloat(Main.screenWidth + rangePadding + float.Epsilon), 0f);
                Vector2 emitVelocity = Vector2.UnitY * 15f;
                rainParticleEmitter?.Emit(emitPosition, emitVelocity, emitVelocity.ToRotation() - MathHelper.PiOver2, 160);
            }
        }
        public override IEnumerable<(int, float)> GetPool(NPCSpawnInfo spawnInfo)
        {
            yield return (ModContent.NPCType<PyroclasticSlime>(), 1f);
        }
        public override void OnDeactivate()
        {
            rainParticleEmitter = null;
        }
    }
}
