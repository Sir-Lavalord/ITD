using ITD.Content.Items.DevTools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ITD.Content.UI
{
    public class MirrorManUI : ITDUIState
    {
        public MirrorManButton horiToggle;
        public MirrorManButton vertiToggle;
        public MirrorManButton selectToggle;
        public bool horiToggled => horiToggle.toggled;
        public bool vertiToggled => vertiToggle.toggled;
        public bool selectToggled => selectToggle.toggled;
        private bool opened;
        public override bool Visible => opened;
        public static bool HoldingMirrorMan => Main.LocalPlayer.HeldItem.type == ModContent.ItemType<MirrorMan>();
        public override void Update(GameTime gameTime)
        {
            if (!HoldingMirrorMan)
                opened = false;
            base.Update(gameTime);
        }
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 2"));
        }
        public void Toggle()
        {
            if (opened)
                Close();
            else
                Open();
        }
        public void Open()
        {
            if (opened)
                return;
            Vector2 mouse = Main.MouseWorld.ToScreenPosition();
            selectToggle?.SetProperties(mouse.Y - 32f, mouse.X - 48f, 32f, 32f);
            horiToggle?.SetProperties(mouse.Y - 32f, mouse.X - 16f, 32f, 32f);
            vertiToggle?.SetProperties(mouse.Y - 32f, mouse.X + 16f, 32f, 32f);
            Recalculate();
            opened = true;
        }
        public void Close()
        {
            if (!opened)
                return;
            opened = false;
        }
        public override void OnInitialize()
        {
            horiToggle = new(MirrorMan.MirroringState.MirrorHorizontally);
            vertiToggle = new(MirrorMan.MirroringState.MirrorVertically);
            selectToggle = new(MirrorMan.MirroringState.MirrorNone);
            Append(horiToggle);
            Append(vertiToggle);
            Append(selectToggle);
        }
    }
    public class MirrorManButton(MirrorMan.MirroringState mirrorType) : ITDUIElement
    {
        private MirrorMan.MirroringState MirrorType = mirrorType;
        public const string buttonTex = "ITD/Content/UI/MirrorManButton";
        public bool hori => MirrorType == MirrorMan.MirroringState.MirrorHorizontally;
        public bool vert => MirrorType == MirrorMan.MirroringState.MirrorVertically;
        public bool select => MirrorType == MirrorMan.MirroringState.MirrorNone;
        public bool toggled = false;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(buttonTex).Value;
            int halfWidth = tex.Width / 3;
            Rectangle frame = new(select ? 0 : hori ? halfWidth : halfWidth * 2, 0, halfWidth, tex.Height);
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), frame, toggled ? Color.White : Color.Gray);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                UICommon.TooltipMouseText(this.GetLocalization(select ? "MouseHoverName" : hori ? "MouseHoverName0" : "MouseHoverName1").Value);
            }
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            toggled = !toggled;
            if ((hori || vert) && toggled)
                UILoader.GetUIState<MirrorManUI>().selectToggle.toggled = false;
            if (select && toggled)
            {
                MirrorManUI p = UILoader.GetUIState<MirrorManUI>();
                p.horiToggle.toggled = false;
                p.vertiToggle.toggled = false;
            }
        }
    }
}
