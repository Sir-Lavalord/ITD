using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace ITD.Systems
{
    public static class NaturalSpawns
    {
        static bool cosJelCounter = false;
        static int cosJelTimer = 0;
        private static readonly int cosJelTime = 60 * 80;
        public static void CosmicJellyfish(bool curTime, bool prevTime, Player player)
        {
            if (prevTime && !curTime) // It has just turned into nighttime
            {
                if (NPC.downedBoss1 && ITDSystem.hasMeteorFallen && (player.ZoneOverworldHeight || player.ZoneSkyHeight) && !cosJelCounter && !DownedBossSystem.downedCosJel)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Main.NewText("It's going to be a wiggly night...", Color.Purple);
                        cosJelCounter = true;
                    }
                }
            }
            if (cosJelCounter)
            {
                cosJelTimer++;
                if (cosJelTimer > cosJelTime)
                {
                    cosJelTimer = 0;
                    cosJelCounter = false;
                    SoundEngine.PlaySound(SoundID.Roar, player.position);

                    int type = ModContent.NPCType<CosmicJellyfish>();

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.SpawnOnPlayer(player.whoAmI, type);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                    }
                }
            }
        }
    }
}
