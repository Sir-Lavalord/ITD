using Terraria.DataStructures;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace ITD.Content.World;

public enum GenpassOrderType
{
    Before,
    After,
    BeforeEverything,
    AfterEverything
}
public readonly record struct GenpassOrder(GenpassOrderType Type, string Find = null);
public abstract class ITDGenpass : ILoadable
{
    public abstract string Name { get; }
    public abstract double Weight { get; }
    public LocalizedText PassMessage { get; set; }
    public abstract GenpassOrder Order { get; }
    public virtual Point16 SelectOrigin()
    {
        return Point16.Zero;
    }
    public abstract void Generate(Point16 selectedOrigin);

    public void Load(Mod mod)
    {
        WorldGenSystem.genpassesTemp.Add(this);
    }
    public void Unload()
    {

    }
}
public sealed class SmartGenpass(ITDGenpass passTemplate) : GenPass($"[ITD] {passTemplate.Name}", passTemplate.Weight)
{
    protected sealed override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        Point16 origin = passTemplate.SelectOrigin();

        progress.Message = passTemplate.PassMessage.Value;

        passTemplate.Generate(origin);
    }
}
