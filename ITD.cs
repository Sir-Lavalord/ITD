global using Microsoft.Xna.Framework;
using ITD.Networking;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using ITD.Systems;
using Terraria.Graphics.Effects;
using ITD.Skies;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using System.Collections.Generic;
using ITD.Systems.Recruitment;
using Terraria.ID;

namespace ITD
{
    public class ITD : Mod
    {
        public static ITD Instance;
        public ITD() => Instance = this;
        public const string BlankTexture = "ITD/Content/BlankTexture";
        public static readonly Dictionary<string, ArmorShaderData> ITDArmorShaders = [];

        internal Mod itdMusic = null;

        internal Mod wikithis = null;
        internal Mod bossChecklist = null;
        internal Mod munchies = null;
        internal Mod achievements = null;
        internal Mod dialogueTweak = null; // this is necessary so the recruitment button doesn't screw up when this mod is on
        public int? GetMusic(string trackName)
        {
            return itdMusic is not null ? MusicLoader.GetMusicSlot(itdMusic, "Music/" + trackName) : null;
        }
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("DialogueTweak", out Mod dialogueTweak))
            {
                dialogueTweak.Call("OnPostPortraitDraw", DrawSomething);
            }
            ExternalModSupport.Init();
            if (!Main.dedServ)
                LoadRecruitmentTextures();
        }
        public override object Call(params object[] args) => ModCalls.Call(args);
        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetSystem.HandlePacket(reader, whoAmI);
        public static void LoadShader(string name, string path)
        {
            Asset<Effect> screen = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
            Filters.Scene[name] = new Filter(new ScreenShaderData(screen, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }
        public static ArmorShaderData LoadArmorShader(string name, string path, string? uImage = null)
        {
            Asset<Texture2D> overlay = null;
            if (uImage != null)
                overlay = ModContent.Request<Texture2D>(uImage, AssetRequestMode.ImmediateLoad);
            ArmorShaderData data = new(ModContent.Request<Effect>(path), name + "Pass");
            if (overlay != null)
                data = data.UseImage(overlay);
            ITDArmorShaders[name] = data;
            return data;
        }
        public override void Load()
        {
            SkyManager.Instance["ITD:CosjelOkuuSky"] = new CosjelOkuuSky();
            LoadShader("BlackMold", "ITD/Shaders/MelomycosisScreen");
            itdMusic = null;
            wikithis = null;
            bossChecklist = null;
            munchies = null;
            achievements = null;
            dialogueTweak = null;
            ModLoader.TryGetMod("ITDMusic", out itdMusic);
            ModLoader.TryGetMod("Wikithis", out wikithis);
            ModLoader.TryGetMod("BossChecklist", out bossChecklist);
            ModLoader.TryGetMod("Munchies", out munchies);
            ModLoader.TryGetMod("TMLAchievements", out achievements);
            ModLoader.TryGetMod("DialogueTweak", out dialogueTweak);
            if (!Main.dedServ)
            {
                wikithis?.Call("AddModURL", this, "https://itdmod.fandom.com/wiki/{}");
            }
        }
        /// <summary>
        /// I DON'T LIKE THE FLICKER THEY DO WHEN THEY'RE FIRST RECRUITED
        /// </summary>
        private void LoadRecruitmentTextures()
        {
            int[] canBeRecruited = TownNPCRecruitmentLoader.NPCsThatCanBeRecruited;
            foreach (int i in canBeRecruited)
            {
                string path = $"ITD/Systems/Recruitment/{nameof(RecruitedNPC)}_{i}";
                LoadRecruitmentTexture(path, i);
            }
            // "so what about modded NPCs?"
            // this logic is handled in the Mod.Call to register a new NPC
        }
        public static void LoadRecruitmentTexture(string path, int i)
        {
            // load regular texture
            ModContent.Request<Texture2D>(path);
            // load shimmer texture if it exists (note: some mods might choose not to add a shimmer texture. in this case, use ModContent.RequestIfExists
            // if an NPC is shimmered but there's no shimmer texture, it'll be automatically handled in the drawcode, so we don't have to worry about that
            if (NPCID.Sets.ShimmerTownTransform[i])
                ModContent.RequestIfExists<Texture2D>(path + "_Shimmer", out _);
        }
        private void DrawSomething(SpriteBatch sb, Color textColor, Rectangle panel)
        {
            var tex = ModContent.Request<Texture2D>("ITD/Effects/ClassicLifeOverlay");
            sb.Draw(tex.Value, panel.Location.ToVector2(), Main.DiscoColor);
        }
        public override void Unload()
        {
            ITDArmorShaders?.Clear();
            itdMusic = null;
            wikithis = null;
            bossChecklist = null;
            munchies = null;
            achievements = null;
            dialogueTweak = null;
            Instance = null;
        }
    }
}
