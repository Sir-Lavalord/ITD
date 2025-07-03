using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Utilities;

namespace ITD.Systems
{
    public static class NaturalSpawns
    {
        static bool cosJelCounter = false;
        static int cosJelTimer = 0;
        private static readonly int cosJelTime = 60 * 80;
        public static LocalizedText cosJelWarning {  get; private set; }
        public static void SetStaticDefaults()
        {
            cosJelWarning = Language.GetOrRegister(ITD.Instance.GetLocalizationKey($"NPCs.{nameof(CosmicJellyfish)}.SpawnWarning"));
        }
        public static void CosmicJellyfishSpawn(bool curTime, bool prevTime, Player player)
        {
            if (prevTime && !curTime) // It has just turned into nighttime
            {
                if (NPC.downedBoss1 && ITDSystem.hasMeteorFallen && (player.ZoneOverworldHeight || player.ZoneSkyHeight) && !cosJelCounter && !DownedBossSystem.downedCosJel)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Main.NewText(cosJelWarning.Value, Color.Purple);
                        cosJelCounter = true;
                    }
                }
            }
            if (cosJelCounter)
            {
                //Main.NewText(cosJelTimer);
                cosJelTimer++;
                if (cosJelTimer > cosJelTime)
                {
                    cosJelTimer = 0;
                    cosJelCounter = false;
                    SoundEngine.PlaySound(SoundID.Roar, player.position);

                    //stop cosjel from spawning while another one is alive
                    int type = ModContent.NPCType<CosmicJellyfish>();
                    if (MiscHelpers.NPCExists(type) == null)
                    {
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
        public static void LeaveWorld()
        {
            cosJelCounter = false;
            cosJelTimer = 0;
        }
    }
}
