using ITD.Systems.Recruitment;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ITD.Content.UI
{
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
        public override void Update(GameTime gameTime)
        {
            float dim = 32f * Main.GameZoomTarget;
            if (npc > -1)
            {
                Vector2 screenPos = (Main.npc[npc].Center - Main.LocalPlayer.velocity - Vector2.UnitY * 48f).ToScreenPosition(); // why do i have to add localplayer velocity :sob:
                unrecruitmentButton?.UpdateProperties(dim, screenPos.X - (dim * 0.5f), screenPos.Y - (dim * 0.5f));
                Recalculate();
            }
            base.Update(gameTime);
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
    public class UnrecruitmentButton : UIElement
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
                UICommon.TooltipMouseText(ITD.Instance.GetLocalization($"UI.{nameof(UnrecruitmentButton)}.MouseHoverName").Value);
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        public static void DoUnrecruit()
        {
            Player player = Main.LocalPlayer;
            UnrecruitmentGui gui = UILoader.GetUIState<UnrecruitmentGui>();
            TownNPCRecruitmentLoader.Unrecruit(gui.npc, player);
            gui.Close();
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            DoUnrecruit();
        }
    }
}
