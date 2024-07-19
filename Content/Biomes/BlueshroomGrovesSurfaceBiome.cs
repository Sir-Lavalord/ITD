using ITD.Content.Backgrounds;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ITD;

namespace ITD.Content.Biomes
{
    public class BlueshroomGrovesSurfaceBiome : ModBiome
    {
        // Select all the scenery
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<BlueshroomGrovesSurfaceBackgroundStyle>();

        // Select Music
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/WanderingTheBlueshrooms");

        // Sets how the Scene Effect associated with this biome will be displayed with respect to vanilla Scene Effects. For more information see SceneEffectPriority & its values.
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh; // We have set the SceneEffectPriority to be BiomeLow for purpose of example, however default behavior is BiomeLow.

        // Populate the Bestiary Filter
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        // Calculate when the biome is active.
        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<ITD.ITDSystem>().bluegrassCount >= 50 && (player.ZoneOverworldHeight || player.ZoneSkyHeight);
        }

        public override float GetWeight(Player player)
        {
            return 0.6f;
        }
    }
}