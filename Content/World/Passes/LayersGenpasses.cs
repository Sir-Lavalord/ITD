using Terraria.DataStructures;

namespace ITD.Content.World.Passes
{
    [Autoload(false)]
    public sealed class LayersPass : ITDGenpass
    {
        public override string Name => "World Layers";
        public override double Weight => 200.0;
        public override GenpassOrder Order => new(GenpassOrderType.AfterEverything);
        public override void Generate(Point16 selectedOrigin)
        {

        }
    }
}
