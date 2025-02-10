using ITD.Content.Items.DevTools;
using ITD.Players;
using ITD.Utilities;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ITD.Content.UI
{
    public class MirrorManUI : ITDUIState
    {
        public enum MMButtonFunction : byte
        {
            Select,
            ToggleMirrorHorizontal,
            ToggleMirrorVertical,
            Undo,
            ToggleCut,
        }
        public MirrorManButton horiToggle;
        public MirrorManButton vertiToggle;
        public MirrorManButton selectToggle;
        public MirrorManButton undo;
        public MirrorManButton cutToggle;
        private MirrorManDraggableTab draggableTab;
        private MirrorManButton[] orderedButtons;

        private TileDataButton tileToggle;
        private TileDataButton wallToggle;
        private TileDataButton liquidToggle;
        private TileDataButton wiringToggle;
        private TileDataButton[] orderedTileButtons;
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

            float totalTileButtonWidth = 32f * orderedTileButtons.Length;
            for(int i = 0; i < orderedTileButtons.Length; i++)
            {
                TileDataButton button = orderedTileButtons[i];
                button?.SetProperties(dragTabY + 48f, (mouseX - totalTileButtonWidth / 2f) + 32f * i, 32f, 32f);
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
            horiToggle = new(MMButtonFunction.ToggleMirrorHorizontal);
            vertiToggle = new(MMButtonFunction.ToggleMirrorVertical);
            selectToggle = new(MMButtonFunction.Select);
            undo = new(MMButtonFunction.Undo);
            undo.canBeToggled = false;
            cutToggle = new(MMButtonFunction.ToggleCut);
            orderedButtons = new MirrorManButton[5];
            orderedButtons[0] = selectToggle;
            orderedButtons[1] = horiToggle;
            orderedButtons[2] = vertiToggle;
            orderedButtons[3] = undo;
            orderedButtons[4] = cutToggle;

            tileToggle = new(SimpleTileDataType.Tile);
            wallToggle = new(SimpleTileDataType.Wall);
            liquidToggle = new(SimpleTileDataType.Liquid);
            wiringToggle = new(SimpleTileDataType.Wiring);
            orderedTileButtons = new TileDataButton[4];
            orderedTileButtons[0] = tileToggle;
            orderedTileButtons[1] = wallToggle;
            orderedTileButtons[2] = liquidToggle;
            orderedTileButtons[3] = wiringToggle;

            draggableTab = new();

            Append(horiToggle);
            Append(vertiToggle);
            Append(selectToggle);
            Append(undo);
            Append(cutToggle);

            Append(tileToggle);
            Append(wallToggle);
            Append(liquidToggle);
            Append(wiringToggle);

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
    public class MirrorManButton(MirrorManUI.MMButtonFunction mirrorType) : ITDUIElement
    {
        private readonly MirrorManUI.MMButtonFunction MirrorType = mirrorType;
        public const string buttonTex = "ITD/Content/UI/MirrorManButton";
        public bool hori => MirrorType == MirrorManUI.MMButtonFunction.ToggleMirrorHorizontal;
        public bool vert => MirrorType == MirrorManUI.MMButtonFunction.ToggleMirrorVertical;
        public bool select => MirrorType == MirrorManUI.MMButtonFunction.Select;
        public bool undo => MirrorType == MirrorManUI.MMButtonFunction.Undo;
        public bool cut => MirrorType == MirrorManUI.MMButtonFunction.ToggleCut;
        public bool toggled = false;
        public bool canBeToggled = true;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(buttonTex).Value;
            int halfWidth = tex.Width / 5;
            Rectangle frame = new( (byte)MirrorType * halfWidth, 0, halfWidth, tex.Height);
            bool shouldBeWhite = undo ? canBeToggled : toggled;
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), frame, shouldBeWhite ? Color.White : Color.Gray);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                UICommon.TooltipMouseText(this.GetLocalization($"MouseHoverName{MirrorType}").Value);
            }
            base.Update(gameTime);
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            if (!canBeToggled)
                return;
            if (undo)
            {
                MirrorMan m = Main.LocalPlayer.HeldItem.ModItem as MirrorMan;
                m.DoUndo();
                canBeToggled = false;
                return;
            }
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
    public class TileDataButton(SimpleTileDataType type) : ITDUIElement
    {
        private readonly SimpleTileDataType Type = type;
        public const string buttonTex = "ITD/Content/UI/TileDataButton";
        public bool On
        {
            get
            {
                SimpleTileDataType flags = Main.LocalPlayer.GetITDPlayer().tileDataSelection;
                return (flags & Type) == Type;
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(buttonTex).Value;
            int halfWidth = tex.Width / 4;
            int f = Type switch
            {
                SimpleTileDataType.Tile => 0,
                SimpleTileDataType.Wall => 1,
                SimpleTileDataType.Liquid => 2,
                SimpleTileDataType.Wiring => 3,
                _ => -1
            };
            Rectangle frame = new(f * halfWidth, 0, halfWidth, tex.Height);
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), frame, On ? Color.White : Color.Gray);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                UICommon.TooltipMouseText(this.GetLocalization($"MouseHoverName{Type}").Format(On ? this.GetLocalization("On").Value : this.GetLocalization("Off").Value));
            }
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            ITDPlayer plr = Main.LocalPlayer.GetITDPlayer();
            // if player has this flag
            if ((plr.tileDataSelection & Type) == Type)
            {
                // remove it
                plr.tileDataSelection &= ~Type;
            }
            else
            {
                // otherwise, add it
                plr.tileDataSelection |= Type;
            }
        }
    }
}
