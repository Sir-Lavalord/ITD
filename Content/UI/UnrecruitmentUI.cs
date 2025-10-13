using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Systems;
using ITD.Systems.Recruitment;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ITD.Content.UI;

public class UnrecruitmentGui : ITDUIState
{
    private UnrecruitmentButton unrecruitmentButton;
    public bool isOpen = false;
    public int npc = -1;
    public override bool Visible => isOpen;
    public override int InsertionIndex(List<GameInterfaceLayer> layers)
    {
        return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Markers"));
    }
    public override void OnInitialize()
    {
        unrecruitmentButton = new UnrecruitmentButton();
        Append(unrecruitmentButton);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        float dim = 32f * Main.GameZoomTarget;
        if (npc > -1)
        {
            Vector2 screenPos = (Main.npc[npc].Center - Vector2.UnitY * 48f).ToScreenPosition();
            unrecruitmentButton?.UpdateProperties(dim, screenPos.X - (dim * 0.5f), screenPos.Y - (dim * 0.5f));
            Recalculate();
        }
        base.Draw(spriteBatch);
    }
    public void Open(Vector2 atWorldPosition, int npcWhoAmI)
    {
        if (isOpen)
            return;
        RemoveAllChildren();
        OnInitialize();
        Recalculate();
        npc = npcWhoAmI;
        isOpen = true;
    }
    public void Close()
    {
        isOpen = false;
        npc = -1;
    }
}
public class UnrecruitmentButton : ITDUIElement
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
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    public static void DoUnrecruit()
    {
        Player player = Main.LocalPlayer;
        UnrecruitmentGui gui = UILoader.GetUIState<UnrecruitmentGui>();
        Guid guid = player.GetITDPlayer().guid;
        RecruitData rData = ITDSystem.recruitmentData[guid];

        if (Main.netMode == NetmodeID.SinglePlayer)
            TownNPCRecruitmentLoader.QueueUnrecruit(guid);
        else
            NetSystem.SendPacket(new QueueUnrecruitmentPacket(guid));

        gui.Close();
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        DoUnrecruit();
    }
}
