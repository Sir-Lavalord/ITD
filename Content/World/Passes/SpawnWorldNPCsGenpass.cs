using Terraria.DataStructures;

namespace ITD.Content.World.Passes;

public class WorldNPCsPass : ITDGenpass
{
    public override string Name => "World NPCs";
    public override double Weight => 0.016;
    public override GenpassOrder Order => new(GenpassOrderType.AfterEverything);
    public override void Generate(Point16 selectedOrigin)
    {
        SpawnMudkarp();
    }
    private static void SpawnMudkarp()
    {

    }
}
