using ITD.Content.Dusts;
using ITD.Content.Items.Materials;
using ITD.Content.Items.Placeable.Biomes.BlueshroomGroves;
using ITD.Content.NPCs.BlueshroomGroves.Critters;
using ITD.Systems;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace ITD.Content.Tiles.BlueshroomGroves;

public class BlueshroomTree : ITDTree
{
    private readonly Asset<Texture2D> glow = Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomTops_Glow");
    private readonly Asset<Texture2D> branchGlow = Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomBranches_Glow");
    public static float opac = 1f;
    public override void SetStaticTreeDefaults()
    {
        ITDSets.LeafGrowFX[Type] = GoreType<BlueshroomGrowLeaves>();
        DustType = DustType<BlueshroomSporesDust>();
        MapColor = new Color(179, 167, 134);
        WoodType = ItemType<BlueshroomStem>();
        DropAcorns = ItemType<Blueshroom>();
    }
    public override TreeShakeSettings TreeShakeSettings => new([NPCType<SmallSnowpoff>()], [NPCType<LargeSnowpoff>()], [NPCID.Firefly], [NPCID.GoldButterfly], [ItemID.Starfruit]);
    public override void PostDrawTreeTops(int i, int j, SpriteBatch spriteBatch, Rectangle sourceRect, Vector2 offset, Vector2 origin, Color color)
    {
        WeatherSystem.DrawTreeSway(i, j, spriteBatch, glow.Value, sourceRect, offset, origin);
    }
    public override void PostDrawBranch(SpriteBatch spriteBatch, Vector2 position, Vector2 origin, Color color, Rectangle sourceRect, bool isLeftBranch)
    {
        spriteBatch.Draw(branchGlow.Value, position, sourceRect, Color.White * opac, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (IsTopTile(i, j))
        {
            Vector2 worldCoords = new Point(i, j).ToWorldCoordinates();
            Lighting.AddLight(worldCoords, new Vector3(0f, 0.85f, 0.9f) * opac);
            if (Main.rand.NextBool(12))
            {
                int offset = 20;
                Dust.NewDust(worldCoords - new Vector2(offset, offset + 64), 16 + offset * 2, 16 + offset * 2, DustType<BlueshroomSporesDust>());
            }
        }
    }
}
public class BlueshroomGrowLeaves : ModGore
{
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
        GoreID.Sets.SpecialAI[Type] = 3;
        GoreID.Sets.PaintedFallingLeaf[Type] = true;
    }
}
