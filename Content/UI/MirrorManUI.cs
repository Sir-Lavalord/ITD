using ITD.Content.Items.DevTools;
using System.Collections.Generic;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ITD.Content.UI
{
    public class MirrorManUI : ITDUIState
    {
        public MirrorManButton horiToggle;
        public MirrorManButton vertiToggle;
        public MirrorManButton selectToggle;
        private MirrorManDraggableTab draggableTab;
        private MirrorManButton[] orderedButtons;
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
            float dragTabX = mouse.X - 23f;
            float dragTabY = mouse.Y - 48f;
            draggableTab?.SetProperties(dragTabY, dragTabX, 46f, 16f);
            RepositionOrderedButtons();

            Recalculate();
            opened = true;
        }
        public void RepositionOrderedButtons()
        {
            float dragTabX = draggableTab.Left.Pixels;
            float dragTabY = draggableTab.Top.Pixels;
            float mouseX = dragTabX + draggableTab.Width.Pixels * 0.5f;
            float totalButtonWidth = 32f * orderedButtons.Length;
            for (int i = 0; i < orderedButtons.Length; i++)
            {
                MirrorManButton button = orderedButtons[i];
                button?.SetProperties(dragTabY + 16f, (mouseX - totalButtonWidth / 2f) + 32f * i, 32f, 32f);
            }
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
            orderedButtons = new MirrorManButton[3];
            orderedButtons[0] = selectToggle;
            orderedButtons[1] = horiToggle;
            orderedButtons[2] = vertiToggle;

            draggableTab = new();
            Append(horiToggle);
            Append(vertiToggle);
            Append(selectToggle);
            Append(draggableTab);
        }
    }
    public class MirrorManDraggableTab : ITDUIElement
    {
        public static Asset<Texture2D> tex => ModContent.Request<Texture2D>("ITD/Content/UI/DraggableTab");
        private Vector2 offset;
        public bool dragging;
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            if (evt.Target == this)
            {
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
                dragging = true;
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (evt.Target == this)
            {
                Vector2 endMousePosition = evt.MousePosition;
                dragging = false;

                Left.Set(endMousePosition.X - offset.X, 0f);
                Top.Set(endMousePosition.Y - offset.Y, 0f);

                if (Parent is MirrorManUI ui)
                {
                    ui.RepositionOrderedButtons();
                    ui.Recalculate();
                }

                Recalculate();
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex.Value, GetDimensions().ToRectangle(), null, Color.White);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                if (Parent is MirrorManUI ui)
                {
                    ui.RepositionOrderedButtons();
                    ui.Recalculate();
                }
                Recalculate();
            }

            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!parentSpace.Contains(GetDimensions().ToRectangle()))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
            base.Update(gameTime);
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
            base.Update(gameTime);
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
            base.LeftClick(evt);
        }
    }
}
