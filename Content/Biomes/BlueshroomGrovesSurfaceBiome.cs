using ITD.Content.Backgrounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Systems;
using Terraria.ID;

namespace ITD.Content.Biomes
{
    public class BlueshroomGrovesSurfaceBiome : ModBiome
    {
        // Select all the scenery
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<BlueshroomGrovesSurfaceBackgroundStyle>();

        // Select Music
        public override int Music => ITD.Instance.GetMusic("BlueshroomGroves") ?? MusicID.Mushrooms;

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
            return ModContent.GetInstance<ITDSystem>().bluegrassCount >= 50 && (player.ZoneOverworldHeight || player.ZoneSkyHeight);
        }

        public override float GetWeight(Player player)
        {
            return 0.6f;
        }
    }
}