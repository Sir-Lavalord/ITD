using ITD.Content.Backgrounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Systems;
using Terraria.ID;
using ITD.Particles;
using ITD.Particles.Ambience;
using ITD.Utilities;

namespace ITD.Content.Biomes
{
    public class BlueshroomGrovesSurfaceBiome : ModBiome
    {
        // Select all the scenery
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<BlueshroomGrovesSurfaceBackgroundStyle>();

        // Select Music
        public override int Music
        {
            get
            {
                if (Main.IsItDay())
                    return ITD.Instance.GetMusic("BlueshroomGroves") ?? MusicID.Mushrooms;
                return ITD.Instance.GetMusic("BlueshroomNight") ?? MusicID.Mushrooms;
            }
        }

        // Sets how the Scene Effect associated with this biome will be displayed with respect to vanilla Scene Effects. For more information see SceneEffectPriority & its values.
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh; // We have set the SceneEffectPriority to be BiomeLow for purpose of example, however default behavior is BiomeLow.

        // Populate the Bestiary Filter
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;
        private ParticleEmitter emitter;
        // Calculate when the biome is active.
        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<ITDSystem>().bluegrassCount >= 50 && (player.ZoneOverworldHeight || player.ZoneSkyHeight);
        }
        public override float GetWeight(Player player)
        {
            return 0.6f;
        }
        public override void OnEnter(Player player)
        {
            if (!Main.IsItDay())
            {
                emitter = ParticleSystem.NewEmitter<LyteflyParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
                emitter.keptAlive = true;
            }
        }
        public override void OnInBiome(Player player)
        {
            if (!Main.IsItDay())
            {
                if (emitter != null)
                {
                    emitter.keptAlive = true;
                    if (Main.GameUpdateCount % 64 == 0)
                    {
                        Vector2 possiblePos = Main.screenPosition + new Vector2(Main.rand.NextFloat(Main.screenWidth + float.Epsilon), Main.rand.NextFloat(Main.screenHeight + float.Epsilon));
                        if (!TileHelpers.SolidTile(possiblePos.ToTileCoordinates()))
                            emitter.Emit(possiblePos, Vector2.Zero, lifetime: 800);
                    }
                }
                else
                {
                    emitter = ParticleSystem.NewEmitter<LyteflyParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
                    emitter.keptAlive = true;
                }
            }
        }
        public override void OnLeave(Player player)
        {
            emitter = null;
        }
    }
}