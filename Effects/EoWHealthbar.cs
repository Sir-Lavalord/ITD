using ITD.Content.Items.Accessories.Master;
using System.Collections.Generic;
using Terraria.GameContent;

namespace ITD.Effects;

public class EoWHealthbar : ModResourceOverlay
{
    private readonly Dictionary<string, Asset<Texture2D>> vanillaAssetCache = [];

    private Asset<Texture2D> heartTexture, fancyPanelTexture, barsFillingTexture, barsPanelTexture;

    public override void PostDrawResource(ResourceOverlayDrawContext context)
    {
        Asset<Texture2D> asset = context.texture;

        string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
        string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

        bool drawingBarsPanels = CompareAssets(asset, barsFolder + "HP_Panel_Middle");

        int testnum = Main.LocalPlayer.GetModPlayer<EoWTailPlayer>().testnum;


        if (context.resourceNumber >= 2 * testnum)
            return;

        if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2)
        {
            DrawClassicFancyOverlay(context);
        }
        else if (CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B"))
        {
            DrawClassicFancyOverlay(context);
        }
        else if (CompareAssets(asset, barsFolder + "HP_Fill") || CompareAssets(asset, barsFolder + "HP_Fill_Honey"))
        {
            DrawBarsOverlay(context);
        }
        else if (CompareAssets(asset, fancyFolder + "Heart_Left") || CompareAssets(asset, fancyFolder + "Heart_Middle") || CompareAssets(asset, fancyFolder + "Heart_Right") || CompareAssets(asset, fancyFolder + "Heart_Right_Fancy") || CompareAssets(asset, fancyFolder + "Heart_Single_Fancy"))
        {
            DrawFancyPanelOverlay(context);
        }
        else if (drawingBarsPanels)
        {
            DrawBarsPanelOverlay(context);
        }
    }

    private bool CompareAssets(Asset<Texture2D> existingAsset, string compareAssetPath)
    {
        if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
            asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);

        return existingAsset == asset;
    }

    private void DrawClassicFancyOverlay(ResourceOverlayDrawContext context)
    {
        context.texture = heartTexture ??= Mod.Assets.Request<Texture2D>("Effects/ClassicLifeOverlay");
        context.Draw();
    }

    private void DrawFancyPanelOverlay(ResourceOverlayDrawContext context)
    {
        string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";

        Vector2 positionOffset;

        if (context.resourceNumber == context.snapshot.AmountOfLifeHearts - 1)
        {
            if (CompareAssets(context.texture, fancyFolder + "Heart_Single_Fancy"))
            {
                positionOffset = new Vector2(8, 8);
            }
            else
            {
                // Other panels existed in this panel's row
                // Vanilla texture is "Heart_Right_Fancy"
                positionOffset = new Vector2(8, 8);
            }
        }
        else if (CompareAssets(context.texture, fancyFolder + "Heart_Left"))
        {
            // First panel in this row
            positionOffset = new Vector2(4, 4);
        }
        else if (CompareAssets(context.texture, fancyFolder + "Heart_Middle"))
        {
            // Any panel that has a panel to its left AND right
            positionOffset = new Vector2(0, 4);
        }
        else
        {
            // Final panel in the first row
            // Vanilla texture is "Heart_Right"
            positionOffset = new Vector2(0, 4);
        }

        // "context" contains information used to draw the resource
        // If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
        context.texture = fancyPanelTexture ??= Mod.Assets.Request<Texture2D>("Effects/FancyLifeOverlay_Panel");
        // Due to the replacement texture and the vanilla texture having different dimensions, the source needs to also be modified
        context.source = context.texture.Frame();
        context.position += positionOffset;
        context.Draw();
    }

    private void DrawBarsOverlay(ResourceOverlayDrawContext context)
    {
        // Draw over the Bars life bars
        // "context" contains information used to draw the resource
        // If you want to draw directly on top of the vanilla bars, just replace the texture and have the context draw the new texture
        context.texture = barsFillingTexture ??= Mod.Assets.Request<Texture2D>("Effects/BarsLifeOverlay_Fill");
        context.Draw();
    }
    private void DrawBarsPanelOverlay(ResourceOverlayDrawContext context)
    {

        context.texture = barsPanelTexture ??= Mod.Assets.Request<Texture2D>("Effects/BarsLifeOverlay_Panel");
        context.source = context.texture.Frame();
        context.position.Y += 6;
        context.Draw();
    }
}