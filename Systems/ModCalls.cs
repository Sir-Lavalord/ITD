using ITD.Players;
using ITD.Systems.Recruitment;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Systems
{
    public static class ModCalls
    {
        public static bool Downed(string bossOrEvent)
        {
            return bossOrEvent.ToLower() switch
            {
                "sandberus" => DownedBossSystem.downedSandberus,
                "lostarchivist" or "keeperofthelost" => DownedBossSystem.downedLostArchivist,
                "cosjel" or "cosmicjellyfish" => DownedBossSystem.downedCosJel,
                "gravekeeper" or "gravekeeper1" => DownedBossSystem.downedGravekeeper,
                "gravekeeperrematch" or "gravekeeper2" => DownedBossSystem.downedGravekeeperRematch,
                "floraldevourer" or "flordev" or "womr" => DownedBossSystem.downedWomr,
                _ => false,
            };
        }
        public static bool Zone(Player player, string zone)
        {
            ITDPlayer modPlayer = player.GetITDPlayer();
            return zone.ToLower() switch
            {
                "blueshroomsurface" or "blueshroomssurface" or "blueshroomgrovessurface" or "bgsurface" => modPlayer.ZoneBlueshroomsSurface,
                "blueshroomunderground" or "blueshroomsunderground" or "blueshroomgroves" or "blueshroomgrovesunderground" or "bgunderground" => modPlayer.ZoneBlueshroomsUnderground,
                "deepdesert" or "dd" => modPlayer.ZoneDeepDesert,
                "catacombs" => modPlayer.ZoneCatacombs,
                _ => false,
            };
        }
        /// <summary>
        /// Args[1] is a mod reference
        /// Args[2] is the NPC type to apply recruitment behavior for
        /// Args[3] is the NPC AI handling delegate
        /// Args[4] is the path to the correct Texture2D for the NPC. This handles shimmer textures automatically
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool TryRegisterRecruitment(params object[] args)
        {
            Mod mod = null;
            int npcType = -1;
            Action<NPC, Player> aiDelegate = null;
            string npcTexturePath = null;
            try
            {
                mod = args[1] as Mod;
                npcType = (int)args[2];
                aiDelegate = args[3] as Action<NPC, Player>;
                npcTexturePath = args[4].ToString();
                TownNPCRecruitmentLoader.RegisterRecruitmentData(mod, npcType, aiDelegate, npcTexturePath);
                if (!Main.dedServ)
                    ITD.LoadRecruitmentTexture(npcTexturePath, npcType);
                return true;
            }
            catch
            {
                ITD.Instance.Logger.Error($"Could not register recruitment data from {mod?.Name ?? "Invalid Mod"}, read log for details.");
                return false;
            }
        }
        public static object Call(params object[] args)
        {
            string callType = args[0].ToString();

            static Player CastToPlayer(object o)
            {
                if (o is int i)
                    return Main.player[i];
                else if (o is Player p)
                    return p;
                return null;
            }

            return callType switch
            {
                "Downed" => Downed(args[1].ToString()),
                "Zone" => Zone(CastToPlayer(args[1]), args[2].ToString()),
                "RegisterRecruitment" => TryRegisterRecruitment(args),
                _ => null,
            };
        }
    }
}
