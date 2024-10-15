global using Microsoft.Xna.Framework;
using ITD.Networking;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using ITD.Systems;

namespace ITD
{
    public class ITD : Mod
    {
        public static ITD Instance;
        public ITD() => Instance = this;
        public const string BlankTexture = "ITD/Content/BlankTexture";

        internal Mod itdMusic = null;

        internal Mod wikithis = null;
        internal Mod bossChecklist = null;
        internal Mod musicDisplay = null;
        internal Mod munchies = null;
        internal Mod achievements = null;
        internal Mod dialogueTweak = null; // this is necessary so the recruitment button doesn't screw up when this mod is on
        public int? GetMusic(string trackName)
        {
            return itdMusic is not null ? MusicLoader.GetMusicSlot(itdMusic, "Music/" + trackName) : null;
        }
        public override void PostSetupContent()
        {
            ExternalModSupport.Init();
            ITDSets.Init();
        }
        public override object Call(params object[] args) => ModCalls.Call(args);
        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetSystem.HandlePacket(reader, whoAmI);
        public override void Load()
        {
            itdMusic = null;
            wikithis = null;
            bossChecklist = null;
            musicDisplay = null;
            munchies = null;
            achievements = null;
            dialogueTweak = null;
            ModLoader.TryGetMod("ITDMusic", out itdMusic);
            ModLoader.TryGetMod("Wikithis", out wikithis);
            ModLoader.TryGetMod("BossChecklist", out bossChecklist);
            ModLoader.TryGetMod("MusicDisplay", out musicDisplay);
            ModLoader.TryGetMod("Munchies", out munchies);
            ModLoader.TryGetMod("TMLAchievements", out achievements);
            ModLoader.TryGetMod("DialogueTweak", out dialogueTweak);
            if (!Main.dedServ)
            {
                wikithis?.Call("AddModURL", this, "https://itdmod.fandom.com/wiki/{}");
            }
        }
        public override void Unload()
        {
            itdMusic = null;
            wikithis = null;
            bossChecklist = null;
            musicDisplay = null;
            munchies = null;
            achievements = null;
            dialogueTweak = null;
            Instance = null;
        }
    }
}
