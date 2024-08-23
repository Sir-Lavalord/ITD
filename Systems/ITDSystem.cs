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

namespace ITD.Systems
{
    public class ITDSystem : ModSystem
    {
        public static bool hasMeteorFallen;
        public int bluegrassCount;
        public int deepdesertTileCount;
        public static SimpleMountData[] defaultMountData;
        public override void PostSetupContent()
        {
            defaultMountData = mounts.Select(a => a.ToSimple()).ToArray();
        }
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            bluegrassCount = tileCounts[ModContent.TileType<Bluegrass>()];
            deepdesertTileCount = tileCounts[ModContent.TileType<DioriteTile>()] + tileCounts[ModContent.TileType<PegmatiteTile>()];
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
        public override void PostUpdateDusts()
        {
            //Main.NewText(Main.hardMode);
            BlueshroomTree.opac = ((float)Math.Sin(Main.GameUpdateCount / 40f) + 1f) / 2f;
        }
    }
}
