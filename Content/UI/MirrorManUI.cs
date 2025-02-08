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
        private MirrorManButton horiToggle;
        private MirrorManButton vertiToggle;
        public bool horiToggled => horiToggle.toggled;
        public bool vertiToggled => vertiToggle.toggled;
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
            Vector2 mouse = Main.MouseScreen;
            horiToggle?.SetProperties(mouse.Y - 32f, mouse.X - 32f, 32f, 32f);
            vertiToggle?.SetProperties(mouse.Y - 32f, mouse.X, 32f, 32f);
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
            Append(horiToggle);
            Append(vertiToggle);
        }
    }
    public class MirrorManButton(MirrorMan.MirroringState mirrorType) : ITDUIElement
    {
        private MirrorMan.MirroringState MirrorType = mirrorType;
        public const string buttonTex = "ITD/Content/UI/MirrorManButton";
        public bool hori => MirrorType == MirrorMan.MirroringState.MirrorHorizontally;
        public bool toggled = false;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(buttonTex).Value;
            int halfWidth = tex.Width / 2;
            Rectangle frame = new(hori ? 0 : halfWidth, 0, halfWidth, tex.Height);
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), frame, toggled ? Color.White : Color.Gray);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                UICommon.TooltipMouseText(this.GetLocalization( hori ? "MouseHoverName" : "MouseHoverName1").Value);
            }
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            toggled = !toggled;
        }
    }
}
