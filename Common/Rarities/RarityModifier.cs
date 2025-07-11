namespace ITD.Common.Rarities
{
    public abstract class RarityModifier : ModType // from better expert rarity mod
    {
        // [...]

        public struct DrawData
        {
            public string Text;
            public Vector2 Position;
            public Color Color;
            public float Rotation;
            public Vector2 Origin;
            public Vector2 Scale;
            public float MaxWidth;
            public float ShadowSpread;
        }

        // [public properties and fields]

        public abstract int RarityType { get; }

        // [public methods]

        public abstract void Draw(DrawData data);

        // [protected methods]

        protected sealed override void Register()
        {
            ModTypeLookup<RarityModifier>.Register(this);
            RarityModifierSystem.AddModifier(this);
        }
    }
}