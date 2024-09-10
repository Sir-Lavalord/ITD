using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Networking;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ITD
{
    public class ITD : Mod
    {
        public static ITD Instance;
        public ITD() => Instance = this;

        internal Mod itdMusic = null;

        internal Mod wikithis = null;
        internal Mod bossChecklist = null;
        internal Mod musicDisplay = null;
        internal Mod munchies = null;
        public int? GetMusic(string trackName)
        {
            return itdMusic is not null ? MusicLoader.GetMusicSlot(itdMusic, "Music/" + trackName) : null;
        }
        public override void PostSetupContent()
        {
            ExternalModSupport.Init();
            ITDSets.Init();
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetSystem.HandlePacket(reader, whoAmI);
        public override void Load()
        {
            itdMusic = null;
            wikithis = null;
            bossChecklist = null;
            musicDisplay = null;
            munchies = null;
            ModLoader.TryGetMod("ITDMusic", out itdMusic);
            ModLoader.TryGetMod("Wikithis", out wikithis);
            ModLoader.TryGetMod("BossChecklist", out bossChecklist);
            ModLoader.TryGetMod("MusicDisplay", out musicDisplay);
            ModLoader.TryGetMod("Munchies", out munchies);
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
            Instance = null;
        }
    }
}
