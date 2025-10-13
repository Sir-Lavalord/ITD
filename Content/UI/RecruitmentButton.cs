using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Systems;
using ITD.Systems.Recruitment;
using ITD.Utilities;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ITD.Content.UI;

public class RecruitmentButtonGui : ITDUIState
{
    private RecruitmentButton recruitmentButton;
    public override bool Visible => RecruitmentButtonVisible();
    private static bool RecruitmentButtonVisible()
    {
        bool talking = Main.LocalPlayer.talkNPC > -1;
        bool inSpecialNPCTalkMenu = Main.InGuideCraftMenu || Main.InReforgeMenu || Main.npcShop > 0;

        bool shouldShow = talking && !inSpecialNPCTalkMenu;

        Mod dialogueTweakMod = ITD.Instance.dialogueTweak;
        if (dialogueTweakMod is null)
            return shouldShow;

        if (dialogueTweakMod.TryGetModConfigValue("Configuration", "VanillaUI", null, out bool isDialogueTweakVanillaUI))
            return shouldShow && isDialogueTweakVanillaUI;

        return shouldShow;
    }
    public override int InsertionIndex(List<GameInterfaceLayer> layers)
    {
        return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 2"));
    }
    public override void OnInitialize()
    {
        recruitmentButton = new RecruitmentButton();
        Append(recruitmentButton);
    }
    public override void Update(GameTime gameTime)
    {
        DynamicSpriteFont font = FontAssets.MouseText.Value;

        float xOffset = 0f;
        Mod dialogueTweakMod = ITD.Instance.dialogueTweak;

        bool altDrawToLeft = dialogueTweakMod != null;
        if (altDrawToLeft)
        {
            dialogueTweakMod.TryGetModConfigValue("Configuration", "ShowSwapButton", null, out altDrawToLeft); // who needs readable code
        }

        if (Main.npcChatCornerItem > 0 || altDrawToLeft)
            xOffset -= 32f;

        // prev implementation was horrid. this replicates what vanilla does to draw Main.npcChatCornerItem. this is more solid
        List<List<TextSnippet>> lines = Utils.WordwrapStringSmart(Main.npcChatText, Color.White, font, 460, 10);
        int numberOfLines = lines.Count;

        float yOffset = (numberOfLines + 1) * 30 + 30;
        recruitmentButton.UpdateProperties(32f, Main.screenWidth / 2 + 210 + xOffset, 60 + yOffset);
        Recalculate();
        base.Update(gameTime);
    }
}
public class RecruitmentButton : ITDUIElement
{
    public const string buttonTex = "ITD/Content/UI/RecruitmentButton";
    public const string highlight = buttonTex + "_Highlight";
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Texture2D swordTexture = ModContent.Request<Texture2D>(buttonTex).Value;
        Texture2D highlightTexture = ModContent.Request<Texture2D>(highlight).Value;
        spriteBatch.Draw(swordTexture, GetDimensions().ToRectangle(), Color.White);
        if (IsMouseHovering)
            spriteBatch.Draw(highlightTexture, GetDimensions().ToRectangle(), Color.White);
    }
    public void UpdateProperties(float dimension, float left, float top)
    {
        Width.Set(dimension, 0f);
        Height.Set(dimension, 0f);
        Left.Set(left, 0f);
        Top.Set(top, 0f);
    }
    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            UICommon.TooltipMouseText(this.GetLocalization("MouseHoverName").Value);
        }
    }
    public static void DoRecruit()
    {
        Player player = Main.LocalPlayer;

        if (Main.netMode == NetmodeID.SinglePlayer)
            TownNPCRecruitmentLoader.QueueRecruit(player.TalkNPC, player);
        else
            NetSystem.SendPacket(new QueueRecruitmentPacket(new QueuedRecruitment(player.talkNPC, player.TalkNPC.type, player.GetITDPlayer().guid)));

        player.SetTalkNPC(-1);
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        DoRecruit();
    }
}
