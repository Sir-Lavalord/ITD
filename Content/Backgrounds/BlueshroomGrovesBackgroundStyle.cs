namespace ITD.Content.Backgrounds
{
    public class BlueshroomGrovesBackgroundStyle : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Backgrounds/BlueshroomBiomeUnderground0");
            textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Backgrounds/BlueshroomBiomeUnderground1");
            textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Backgrounds/BlueshroomBiomeUnderground2");
            textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Backgrounds/BlueshroomBiomeUnderground3");
        }
    }
}