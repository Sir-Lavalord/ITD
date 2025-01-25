using ITD.Content.NPCs.Events.LavaRain;
using ITD.Networking;
using ITD.Networking.Packets;
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
        public ushort maxProgress;
        public ushort currentProgress;
        public ParticleEmitter rainParticleEmitter;
        public override void OnActivate()
        {
            int count = 0;
            foreach (var plr in Main.ActivePlayers)
                count++;
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
            float rangePadding = 100f;
            if (Main.rand.NextBool(2))
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
        }
        public override void OnKill(NPC npc)
        {
            if (npc.type == ModContent.NPCType<PyroclasticSlime>())
                currentProgress++;
            NetSystem.SendPacket<SyncEventDataPacket>(new(Type));
        }
        public override void OnDeactivate()
        {
            rainParticleEmitter = null;
        }
    }
}
