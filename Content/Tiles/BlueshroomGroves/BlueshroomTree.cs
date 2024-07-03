using ITD.Content.Items.Materials;
using ITD.Content.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.BlueshroomGroves;

public class BlueshroomTree : ModTree
{
    private Asset<Texture2D> texture;
    private Asset<Texture2D> branchesTexture;
    private Asset<Texture2D> topsTexture;

    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<Bluegrass>()];
        texture = ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomTree");
        branchesTexture = ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomBranches");
        topsTexture = ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomTops");
    }
    public override Asset<Texture2D> GetTexture()
    {
        return texture;
    }

    public override int SaplingGrowthType(ref int style)
    {
        style = 0;
        return ModContent.TileType<BlueshroomSapling>();
    }

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        
    }

    public override Asset<Texture2D> GetBranchTextures() => branchesTexture;

    public override Asset<Texture2D> GetTopTextures() => topsTexture;

    public override int DropWood()
    {
        return ModContent.ItemType<Blueshroom>();
    }
    public override bool CanDropAcorn()
    {
        return false;
    }
    public override bool Shake(int x, int y, ref bool createLeaves)
    {
        Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ModContent.ItemType<Blueshroom>());
        return false;
    }
}