using ITD.Content.NPCs.Events.LavaRain;
using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Particles;
using ITD.Particles.Events;
using System.Collections.Generic;
using System.Linq;
using ITD.Utilities;

namespace ITD.Content.Events
{
    public class LavaRainEvent : ITDEvent
    {
        public ushort maxProgress;
        public ushort currentProgress;
        public ParticleEmitter rainParticleEmitter;
        public override int Music => ITD.Instance.GetMusic("LavaRain") ?? MusicID.DukeFishron;
        public override void OnActivate()
        {
            int count = Main.player.Where(p => p.Exists()).Count();
            maxProgress = (ushort)(40 + (20 * count));
            currentProgress = 0;

            if (Main.dedServ)
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
            float rangePadding = 250f;
            if (Main.rand.NextBool())
            {
                Vector2 emitPosition = (Main.screenPosition - Vector2.UnitX * rangePadding) + new Vector2(Main.rand.NextFloat(Main.screenWidth + rangePadding + float.Epsilon), 0f);
                Vector2 emitVelocity = Vector2.UnitY * 13.5f;
                rainParticleEmitter?.Emit(emitPosition, emitVelocity, emitVelocity.ToRotation() - MathHelper.PiOver2, 160);
            }
        }
        public override float GetVisualProgress()
        {
            return currentProgress / (float)maxProgress;
        }
        public override IEnumerable<(int, float)> GetPool(NPCSpawnInfo spawnInfo)
        {
            yield return (ModContent.NPCType<PyroclasticSlime>(), 1f);
            yield return (ModContent.NPCType<SmoggyNimbus>(), 0.5f);
            yield return (ModContent.NPCType<Flamgoustine>(), 2f);
            yield return (ModContent.NPCType<Magmaripper>(), 1f);
        }
        public override void OnKill(NPC npc)
        {
            if (ITDSets.LavaRainEnemy[npc.type])
            {
                currentProgress++;
                NetSystem.SendPacket<SyncEventDataPacket>(new(Type));
            }
        }
        public override void OnDeactivate()
        {
            rainParticleEmitter = null;
        }
    }
}
