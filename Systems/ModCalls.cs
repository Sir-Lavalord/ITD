using ITD.Content.NPCs.Friendly.WorldNPCs;
using ITD.Systems.Recruitment;
using ITD.Utilities;
using System;

namespace ITD.Systems;

public static class ModCalls
{
    public static bool Downed(string bossOrEvent)
    {
        return bossOrEvent.ToLower() switch
        {
            "sandberus" => DownedBossSystem.DownedSandberus,
            "lostarchivist" or "keeperofthelost" => DownedBossSystem.DownedLostArchivist,
            "cosjel" or "cosmicjellyfish" => DownedBossSystem.DownedCosJel,
            "gravekeeper" or "gravekeeper1" => DownedBossSystem.DownedGravekeeper,
            "gravekeeperrematch" or "gravekeeper2" => DownedBossSystem.DownedGravekeeperRematch,
            "floraldevourer" or "flordev" or "womr" => DownedBossSystem.DownedWomr,
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
    public static bool TalkingTo(Player player, string worldNPC)
    {
        ITDPlayer modPlayer = player.GetITDPlayer();
        return worldNPC.ToLower() switch
        {
            "mudkarp" => Main.npc[modPlayer.TalkWorldNPC].ModNPC is Mudkarp,
            _ => false,
        };
    }
    /// <summary>
    /// Args[1] is a mod reference
    /// Args[2] is the NPC type to Apply recruitment behavior for
    /// Args[3] is the NPC AI handling delegate
    /// Args[4] is the path to the correct Texture2D for the NPC. This handles shimmer textures automatically
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool TryRegisterRecruitment(params object[] args)
    {
        Mod mod = null;
        try
        {
            mod = args[1] as Mod;
            int npcType = (int)args[2];
            Action<NPC, Player> aiDelegate = args[3] as Action<NPC, Player>;
            string npcTexturePath = args[4].ToString();
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
            "TalkingTo" or "TalkingToWorldNPC" => TalkingTo(CastToPlayer(args[1]), args[2].ToString()),
            _ => null,
        };
    }
}
