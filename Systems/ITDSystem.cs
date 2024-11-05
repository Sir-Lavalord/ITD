using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Tiles.DeepDesert;
using System;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;
using static Terraria.Mount;
using Terraria.Localization;
using System.Linq;
using System.Collections.Generic;
using ITD.Content.Items.Other;
using ITD.Content.Items.Weapons.Melee;
using ITD.Utilities;
using ITD.Systems.Recruitment;

namespace ITD.Systems
{
    public class ITDSystem : ModSystem
    {
        internal static bool _hasMeteorFallen;
        public static bool hasMeteorFallen
        {
            get => _hasMeteorFallen;
            set
            {
                if (!value)
                    _hasMeteorFallen = false;
                else
                    NPC.SetEventFlagCleared(ref _hasMeteorFallen, -1);
            }
        }
        public int bluegrassCount;
        public int deepdesertTileCount;
        //misc
        public static SimpleMountData[] defaultMountData;
        public override void SetStaticDefaults()
        {
            NaturalSpawns.SetStaticDefaults();
        }
        public override void PostSetupContent()
        {
            defaultMountData = mounts.Select(a => a.ToSimple()).ToArray();
        }
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            bluegrassCount = tileCounts[ModContent.TileType<Bluegrass>()];
            deepdesertTileCount = tileCounts[ModContent.TileType<DioriteTile>()] + tileCounts[ModContent.TileType<PegmatiteTile>()];
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
        public override void PostUpdateDusts()
        {
            BlueshroomTree.opac = ((float)Math.Sin(Main.GameUpdateCount / 40f) + 1f) / 2f;
        }
        public override void PostUpdateTime()
        {
            // prevent recruited town NPCs from spawning again
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.ModNPC is not RecruitedNPC rNPC)
                    continue;
                int originalType = rNPC.originalType;
                if (originalType > -1)
                    Main.townNPCCanSpawn[originalType] = false;
            }
        }
    }
}
