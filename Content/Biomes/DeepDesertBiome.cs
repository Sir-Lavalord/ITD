﻿using ITD.Systems;

namespace ITD.Content.Biomes
{
    public class DeepDesertBiome : ModBiome
    {
        public override int Music => ITD.Instance.GetMusic("DeepDesert") ?? MusicID.UndergroundDesert;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh; // We have set the SceneEffectPriority to be BiomeLow for purpose of example, however default behavior is BiomeLow.
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;

        // Calculate when the biome is active.
        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<ITDSystem>().deepdesertTileCount >= 50 && (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight);
        }
    }
}