using ITD.Content.Backgrounds;
using ITD.Systems;

namespace ITD.Content.Biomes;

public class BlueshroomGrovesBiome : ModBiome
{
    // Select all the scenery
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<BlueshroomGrovesBackgroundStyle>();

    // Select Music
    public override int Music => ITD.Instance.GetMusic("WanderingTheBlueshrooms") ?? MusicID.Ice;

    // Sets how the Scene Effect associated with this biome will be displayed with respect to vanilla Scene Effects. For more information see SceneEffectPriority & its values.
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh; // We have set the SceneEffectPriority to be BiomeLow for purpose of example, however default behavior is BiomeLow.

    // Populate the Bestiary Filter
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => base.BackgroundColor;

    // Calculate when the biome is active.
    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<ITDSystem>().bluegrassCount >= 50 && (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight);
    }
}