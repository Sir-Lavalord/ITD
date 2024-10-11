using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using ITD.Content.Projectiles.Hostile;

namespace ITD
{
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
        public static bool downedSandberus
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
        public static bool downedLostArchivist
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
        public static bool downedCosJel
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
        public static bool downedGravekeeper
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
        public static bool downedGravekeeperRematch
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
        public static bool downedWomr
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
            downedSandberus = false;
            downedLostArchivist = false;
            downedCosJel = false;
            downedGravekeeper = false;
            downedGravekeeperRematch = false;
            downedWomr = false;
        }
        public override void OnWorldLoad() => ResetFlags();
        public override void OnWorldUnload() => ResetFlags();
        public override void SaveWorldData(TagCompound tag)
        {
            List<string> downed = [];
            if (downedSandberus)
                downed.Add(sandberusName);
            if (downedLostArchivist)
                downed.Add(lostArchivistName);
            if (downedCosJel)
                downed.Add(cosJelName);
            if (downedGravekeeper)
                downed.Add(gravekeeperName);
            if (downedGravekeeperRematch)
                downed.Add(gravekeeperRematchName);
            if (downedWomr)
                downed.Add(womrName);
            tag["downedFlags"] = downed;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> downed = tag.GetList<string>("downedFlags");
            downedSandberus = downed.Contains(sandberusName);
            downedLostArchivist = downed.Contains(lostArchivistName);
            downedCosJel = downed.Contains(cosJelName);
            downedGravekeeper = downed.Contains(gravekeeperName);
            downedGravekeeperRematch = downed.Contains(gravekeeperRematchName);
            downedWomr = downed.Contains(womrName);
        }
    }
}
