using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.NPCs.Bosses;
using ITD.Physics;
using Terraria.ID;
using System;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.Audio;
using Terraria.Localization;
using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Tiles.DeepDesert;

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
        public class ITDSystem : ModSystem
        {
            public static bool hasMeteorFallen;
            public int bluegrassCount;
            public int deepdesertTileCount;
            public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
            {
                bluegrassCount = tileCounts[ModContent.TileType<Bluegrass>()];
                deepdesertTileCount = tileCounts[ModContent.TileType<DioriteTile>()]+tileCounts[ModContent.TileType<PegmatiteTile>()];
            }

            public override void ClearWorld()
            {
                hasMeteorFallen = false;
            }
            public override void SaveWorldData(TagCompound tag)
            {
                if (hasMeteorFallen)
                {
                    tag["hasMeteorFallen"] = true;
                }
            }

            public override void LoadWorldData(TagCompound tag)
            {
                hasMeteorFallen = tag.ContainsKey("hasMeteorFallen");
            }

            public override void NetSend(BinaryWriter writer)
            {
                var flags = new BitsByte();
                flags[0] = hasMeteorFallen;
                writer.Write(flags);
            }

            public override void NetReceive(BinaryReader reader)
            {
                BitsByte flags = reader.ReadByte();
                hasMeteorFallen = flags[0];
            }

            public override void AddRecipeGroups()
            {
                RecipeGroup group = new(() => Language.GetTextValue("LegacyMisc.37") + " Iron Ore",
                [
                ItemID.IronOre,
                ItemID.LeadOre
                ]);
                RecipeGroup.RegisterGroup("IronOre", group);
            }
            public override void PostUpdateDusts()
            {
                BlueshroomTree.sinElement += 0.02f;
                BlueshroomTree.opac = ((float)Math.Sin(BlueshroomTree.sinElement) + 1f) / 2f;
            }
        }
    }
}
