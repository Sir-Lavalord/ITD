using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
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
        internal Mod wikithis = null;
        internal Mod bossChecklist = null;
        internal Mod musicDisplay = null;
        public override void PostSetupContent()
        {
            ExternalModSupport.Init();
            ITDSets.Init();
        }
        public override void Load()
        {
            wikithis = null;
            bossChecklist = null;
            musicDisplay = null;
            ModLoader.TryGetMod("Wikithis", out wikithis);
            ModLoader.TryGetMod("BossChecklist", out bossChecklist);
            ModLoader.TryGetMod("MusicDisplay", out musicDisplay);
            if (!Main.dedServ)
            {
                wikithis?.Call("AddModURL", this, "https://itdmod.fandom.com/wiki/{}");
            }
        }
        public override void Unload()
        {
            wikithis = null;
            bossChecklist = null;
            musicDisplay = null;
            Instance = null;
        }
    }
}
