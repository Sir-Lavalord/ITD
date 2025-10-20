using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace ITD.Systems;

public class DownedBossSystem : ModSystem
{
    // This system is modeled on Calamity's DownedBossSystem. I think this'll be good on the long term since the amount of bosses in this mod is ginormous.
    // Const strings (cuz AAAAAAAA)
    private const string sandberusName = "sandberus";
    private const string lostArchivistName = "lostArchivist";
    private const string cosJelName = "cosJel";
    private const string gravekeeperName = "gravekeeper";
    private const string gravekeeperRematchName = "gravekeeperRematch";
    private const string womrName = "womr";
    // Bosses
    internal static bool _downedSandberus = false;
    internal static bool _downedLostArchivist = false;
    internal static bool _downedCosJel = false;
    internal static bool _downedGravekeeper = false;
    internal static bool _downedGravekeeperRematch = false;
    internal static bool _downedWomr = false;

    #region gettersetters
    public static bool DownedSandberus
    {
        get => _downedSandberus;
        set
        {
            if (!value)
                _downedSandberus = false;
            else
                NPC.SetEventFlagCleared(ref _downedSandberus, -1);
        }
    }
    public static bool DownedLostArchivist
    {
        get => _downedLostArchivist;
        set
        {
            if (!value)
                _downedLostArchivist = false;
            else
                NPC.SetEventFlagCleared(ref _downedLostArchivist, -1);
        }
    }
    public static bool DownedCosJel
    {
        get => _downedCosJel;
        set
        {
            if (!value)
                _downedCosJel = false;
            else
                NPC.SetEventFlagCleared(ref _downedCosJel, -1);
        }
    }
    public static bool DownedGravekeeper
    {
        get => _downedGravekeeper;
        set
        {
            if (!value)
                _downedGravekeeper = false;
            else
                NPC.SetEventFlagCleared(ref _downedGravekeeper, -1);
        }
    }
    public static bool DownedGravekeeperRematch
    {
        get => _downedGravekeeperRematch;
        set
        {
            if (!value)
                _downedGravekeeperRematch = false;
            else
                NPC.SetEventFlagCleared(ref _downedGravekeeperRematch, -1);
        }
    }
    public static bool DownedWomr
    {
        get => _downedWomr;
        set
        {
            if (!value)
                _downedWomr = false;
            else
                NPC.SetEventFlagCleared(ref _downedWomr, -1);
        }
    }
    #endregion

    internal static void ResetFlags()
    {
        DownedSandberus = false;
        DownedLostArchivist = false;
        DownedCosJel = false;
        DownedGravekeeper = false;
        DownedGravekeeperRematch = false;
        DownedWomr = false;
    }
    public override void OnWorldLoad() => ResetFlags();
    public override void OnWorldUnload() => ResetFlags();
    public override void SaveWorldData(TagCompound tag)
    {
        List<string> downed = [];
        if (DownedSandberus)
            downed.Add(sandberusName);
        if (DownedLostArchivist)
            downed.Add(lostArchivistName);
        if (DownedCosJel)
            downed.Add(cosJelName);
        if (DownedGravekeeper)
            downed.Add(gravekeeperName);
        if (DownedGravekeeperRematch)
            downed.Add(gravekeeperRematchName);
        if (DownedWomr)
            downed.Add(womrName);
        tag["downedFlags"] = downed;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        IList<string> downed = tag.GetList<string>("downedFlags");
        DownedSandberus = downed.Contains(sandberusName);
        DownedLostArchivist = downed.Contains(lostArchivistName);
        DownedCosJel = downed.Contains(cosJelName);
        DownedGravekeeper = downed.Contains(gravekeeperName);
        DownedGravekeeperRematch = downed.Contains(gravekeeperRematchName);
        DownedWomr = downed.Contains(womrName);
    }
}
