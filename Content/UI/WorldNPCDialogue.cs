using ITD.Systems.WorldNPCs;
using ITD.Utilities;
using ITD.Utilities.EntityAnim;
using ITD.Utilities.Placeholders;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ITD.Content.UI
{
    public class WorldNPCDialogue : ITDUIState
    {
        public WorldNPCDialogueBox dialogueBox;
        public bool isOpen = false;
        public bool isClosing = false;
        public override bool Visible => isOpen;
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
        }
        public override void OnInitialize()
        {
            // sharp
            OverrideSamplerState = SamplerState.PointClamp;
            dialogueBox = new WorldNPCDialogueBox();
            Append(dialogueBox);
        }
        public void RecalculateBoxDimensions()
        {
            if (dialogueBox != null)
            {
                float height = (Main.screenHeight / 3f) * dialogueBox.openProgress;
                float top = Main.screenHeight - WorldNPCDialogueBox.BoxPaddingDown - height;
                float left = WorldNPCDialogueBox.BoxPaddingSides;
                float width = Main.screenWidth - WorldNPCDialogueBox.BoxPaddingSides * 2;
                dialogueBox.SetProperties(top, left, width, height);
                Recalculate();
            }
        }
        public override void Update(GameTime gameTime)
        {
            RecalculateBoxDimensions();
            base.Update(gameTime);
        }
        public void Open(string key = null)
        {
            isClosing = false;
            if (isOpen)
                return;
            dialogueBox.Open(key);
            RecalculateBoxDimensions();
            isOpen = true;
        }
        public void Close()
        {
            if (!isOpen)
                return;
            dialogueBox.Close();
            isClosing = true;
        }
        public void ForceClose()
        {
            isClosing = false;
            isOpen = false;
        }
    }
    /// <summary>
    /// ported from my game #shamelessplug
    /// </summary>
    public class WorldNPCDialogueBox : ITDUIElement
    {
        private readonly Asset<DynamicSpriteFont> font = FontAssets.MouseText;
        public const string DialogueLanguageKey = "Mods.ITD.WorldNPCDialogue";
        public const int BoxPaddingSides = 72;
        public const int BoxPaddingDown = 12;
        private Vector2 TextPadding = new(32, 24);
        private bool writing = false;
        private string _text = "";
        private int textStep;
        private int textSpeed = 1;
        private int textTimer;
        /// <summary>
        /// Not in frames, but in text steps. 0 will play a voice clip for every single letter typed.
        /// </summary>
        private int voiceFrequency = 6;
        private int voiceTimer;
        public float openProgress;
        public string DialogueInstance => $"{DialogueLanguageKey}.{speakerKey}.{dialogueKey}";
        /// <summary>
        /// This decides which music to use. Jumping between tracks for different speakers (even if it's not realistically possible) would be bad.
        /// </summary>
        public string godSpeaker;
        public string speakerKey;
        public string dialogueKey;
        public string Goal { get; set; }

        public float speakerHeadYPositionPercentOffset;
        public float speakerHeadVerticalScale = 1f;

        public List<DialogueButton> buttons = [];
        public float buttonsOpacity = 0f;
        public DialogueButton closeButton;
        public SpeakerHeadDrawingData DrawingData { 
            get
            {
                return string.IsNullOrEmpty(speakerKey) ? new SpeakerHeadDrawingData(ModContent.Request<Texture2D>(Placeholder.PHGeneric), 1) : (ITD.Instance.Find<ModNPC>(speakerKey) as WorldNPC).DrawingData;
            }
        }
        public List<SoundStyle> SpeechSounds
        {
            get
            {
                if (string.IsNullOrEmpty(speakerKey))
                {
                    return [SoundID.MenuTick];
                }
                else
                {
                    return [..(ITD.Instance.Find<ModNPC>(speakerKey) as WorldNPC).GetSpeechSounds()];
                }
            }
        }
        public SpeakerHeadDrawingData currentData;
        public List<SoundStyle> currentSpeechSounds;
        public int speakerHeadFrame = 0;
        public WorldNPCDialogue ParentState => Parent as WorldNPCDialogue;
        public WorldNPC TalkNPC { get
            {
                int worldNPC = Main.LocalPlayer.GetITDPlayer().TalkWorldNPC;
                if (worldNPC == -1)
                    return null;
                return NPCLoader.GetNPC(Main.npc[worldNPC].type) as WorldNPC;
            }
        }
        private Keyframe<float> tweenReference;
        private bool drawSpeakerHead = false;
        public override void OnInitialize()
        {
            closeButton = new DialogueButton(new(string.Empty, DialogueAction.CloseDialogueBox, string.Empty)) { Texture = ModContent.Request<Texture2D>(WorldNPC.WorldNPCAssetsPath + "X", AssetRequestMode.ImmediateLoad), Active = true };
            Append(closeButton);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetDimensions().ToRectangle();
            float boxOpacity = EasingFunctions.OutQuad(openProgress);
            Texture2D boxStyle = GetTexture().Value;

            // draw speaker head
            if (drawSpeakerHead)
            {
                Texture2D head = currentData.Texture.Value;
                byte frameCount = currentData.FrameCount;
                Rectangle frame = head.Frame(1, frameCount, 0, speakerHeadFrame);
                Vector2 drawPos = new(bounds.X + bounds.Width / 4f, bounds.Y + head.Height / 2 - (head.Height * speakerHeadYPositionPercentOffset));
                Vector2 drawScale = new(1f / speakerHeadVerticalScale, speakerHeadVerticalScale);
                Vector2 origin = new(head.Width / 2, head.Height / frameCount / 2);
                spriteBatch.Draw(head, drawPos, frame, Color.White * boxOpacity, 0f, origin, drawScale, SpriteEffects.None, 0f);
            }

            Rectangle half = new(bounds.X, bounds.Y, bounds.Width / 2, bounds.Height);
            Rectangle otherHalf = new(bounds.X + half.Width, bounds.Y, half.Width, bounds.Height);
            DrawAdjustableBox(spriteBatch, boxStyle, half, Color.White * boxOpacity);
            DrawAdjustableBox(spriteBatch, boxStyle, otherHalf, Color.White * boxOpacity);

            // draw label box
            string worldNPCKey = $"{DialogueLanguageKey}.{speakerKey}.Name";

            string label = Language.Exists(worldNPCKey) ? Language.GetTextValue(worldNPCKey) : "...";
            int labelWidth = (int)(font.Value.MeasureString(label).X + TextPadding.X * 2);
            int labelHeight = 42;
            Rectangle labelRect = new(bounds.X + bounds.Width / 4 - labelWidth / 2, bounds.Y, labelWidth, labelHeight);
            DrawAdjustableBox(spriteBatch, boxStyle, labelRect, Color.White * boxOpacity);
            DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, label, labelRect.Location.ToVector2() + new Vector2(TextPadding.X, labelHeight / 4), Color.White * boxOpacity, Color.Black * boxOpacity);

            //int maxTextWidth = bounds.Width - (int)(TextPadding.X * 1.9f);
            int maxTextWidth = (int)((bounds.Width / 2) - TextPadding.X * 2f);
            var lines = TextHelpers.WordwrapStringSuperSmart(_text, Color.White, font.Value, maxTextWidth, 10);
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                Vector2 drawPos = new(bounds.X + TextPadding.X, bounds.Y + TextPadding.Y + labelHeight + (i * 32f));
                DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, [.. line], drawPos, Color.White * boxOpacity, Color.Black * boxOpacity);
                
            }
            DrawButtons(spriteBatch, boxStyle, font);
            base.DrawSelf(spriteBatch);
        }
        public void DrawButtons(SpriteBatch spriteBatch, Texture2D texture, Asset<DynamicSpriteFont> font)
        {
            closeButton?.DrawButton(spriteBatch, texture, 1f, font);
            if (buttons.Any(b => b.Active))
            {
                if (buttonsOpacity < 1f)
                {
                    buttonsOpacity += 0.05f;
                }
                foreach (DialogueButton button in buttons)
                {
                    button.DrawButton(spriteBatch, texture, buttonsOpacity, font);
                }
            }
        }
        public static void DrawColorCodedStringWithShadowWithValidOpacity(SpriteBatch spriteBatch, Asset<DynamicSpriteFont> font, string text, Vector2 position, Color color, Color shadowColor, float scale = 1f, float spread = 2f) => DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, [..ChatManager.ParseMessage(text, color)], position, color, shadowColor, scale, spread);
        public static void DrawColorCodedStringWithShadowWithValidOpacity(SpriteBatch spriteBatch, Asset<DynamicSpriteFont> font, TextSnippet[] snippets, Vector2 position, Color color, Color shadowColor, float scale = 1f, float spread = 2f)
        {
            for (int i = 0; i < ChatManager.ShadowDirections.Length; i++)
            {
                ChatManager.DrawColorCodedString(spriteBatch, font.Value, snippets, position + ChatManager.ShadowDirections[i] * spread, shadowColor, 0f, Vector2.Zero, new Vector2(scale), out var _, -1, ignoreColors: true);
            }
            ChatManager.DrawColorCodedString(spriteBatch, font.Value, snippets, position, color, 0f, Vector2.Zero, new Vector2(scale), out var _, -1, ignoreColors: true);
        }
        public Asset<Texture2D> GetTexture()
        {
            if (string.IsNullOrEmpty(speakerKey))
                return ModContent.Request<Texture2D>("ITD/Systems/WorldNPCs/Assets/BoxStyles/DefaultBoxStyle");
            return (ITD.Instance.Find<ModNPC>(speakerKey) as WorldNPC).DialogueBoxStyle;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            PositionButtons();
            if (writing)
            {
                if (++textTimer > textSpeed)
                {
                    if (textStep + 1 > Goal.Length) // message has ended, stop writing.
                    {
                        writing = false;
                        foreach (var b in buttons)
                        {
                            b.Active = true;
                        }
                    }
                    else
                    {
                        textStep += 1;
                        textTimer = 0;
                        ++voiceTimer;
                        while (IsInIncompleteTag(Goal, textStep)) // check if we're inside an incomplete tag (e. g. [mvs], or [c], basically a tag that doesn't have a : end marker.
                        {
                            textStep += 1;
                            if (textStep >= Goal.Length)
                            {
                                writing = false;
                                break;
                            }
                            if (!IsInIncompleteTag(Goal, textStep)) // jump ahead right after the while loop is over
                                textStep += 1;
                        }
                        if (++voiceTimer > voiceFrequency)
                        {
                            SoundStyle chosenSound = Main.rand.NextFromCollection(currentSpeechSounds);
                            SoundEngine.PlaySound(chosenSound);
                            voiceTimer = 0;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Goal))
            {
                _text = Goal[..textStep];
            }
        }
        private static bool IsInIncompleteTag(string text, int step)
        {
            int lastOpenBracket = text.LastIndexOf('[', step - 1);
            if (lastOpenBracket == -1) return false;

            int nextColon = text.IndexOf(':', lastOpenBracket);
            int nextCloseBracket = text.IndexOf(']', lastOpenBracket);

            return (nextColon == -1 || nextColon >= step) && (nextCloseBracket == -1 || nextCloseBracket >= step);
        }
        public void BeginTypewriter(string key)
        {
            string[] keyElements = key.Split('.'); // first element will be the WorldNPC key, second will be the specific dialogue key.
            speakerKey = keyElements[0];
            dialogueKey = keyElements[1];
            if (string.IsNullOrEmpty(godSpeaker))
                godSpeaker = speakerKey;
            string lookForKey = DialogueInstance;
            string lookForValidBody = $"{lookForKey}.Body";
            if (!Language.Exists(lookForValidBody))
            {
                Main.NewText($"Error when trying to begin typewriter: the given key, {lookForKey}, was not found to have a valid Body.", Color.Red);
                return;
            }
            GenerateButtons();
            Goal = Language.GetTextValue(lookForValidBody);
            textStep = 0;
            writing = true;
        }
        public void PositionButtons()
        {
            Rectangle boxBounds = GetDimensions().ToRectangle();

            // position the close button:
            float dim = 64f;
            closeButton?.SetProperties(boxBounds.Height - dim - TextPadding.Y, boxBounds.Width - dim - TextPadding.X, dim, dim);

            //Rectangle buttonSpace = new(boxBounds.X + (int)TextPadding.X, boxBounds.Y, boxBounds.Width - (int)TextPadding.X * 2, boxBounds.Height);
            Rectangle buttonSpace = new(boxBounds.X + (int)TextPadding.X, boxBounds.Y, (boxBounds.Width / 2) - (int)TextPadding.X * 2, (int)(boxBounds.Height - dim - (TextPadding.Y * 3f)));
            //float countWidth = buttons.Count > 0 ? buttonSpace.Width / buttons.Count : default;
            float countHeight = buttons.Count > 0 ? buttonSpace.Height / buttons.Count : default;
            for (int i = 0; i < buttons.Count; i++)
            {
                DialogueButton button = buttons[i];
                // buttons must be positioned in accordance to the text padding.
                /*
                float stringWidth = font.Value.MeasureString(button.Label).X + TextPadding.X * 2f;
                float buttonWidth = Math.Max(stringWidth, countWidth);
                float buttonHeight = 64f;
                float left = TextPadding.X + buttonWidth * i;
                float top = boxBounds.Height - buttonHeight - TextPadding.Y;
                */
                float buttonWidth = buttonSpace.Width;
                float left = boxBounds.Width / 2 + TextPadding.X;
                float top = TextPadding.Y + countHeight * i;
                button.SetProperties(top, left, buttonWidth, countHeight);
            }
        }
        public void GenerateButtons()
        {
            var buttonData = GetButtonData();
            foreach (GeneratedButtonData data in buttonData)
            {
                DialogueButton newButton = new(data) { Active = false };
                Append(newButton);
                buttons.Add(newButton);
            }
            PositionButtons();
            Recalculate();
        }
        public GeneratedButtonData[] GetButtonData()
        {
            List<GeneratedButtonData> buttonBuffer = [];
            int buttonIndex = 0;
            // while valid buttons are found, add new buttondatas to the buffer.
            while (Language.Exists($"{DialogueInstance}.Buttons.Button{buttonIndex}.Text"))
            {
                string possibleKey = $"{DialogueInstance}.Buttons.Button{buttonIndex}.Key";
                string realKey = Language.Exists(possibleKey) ? Language.GetTextValue(possibleKey) : null;

                string possibleAction = $"{DialogueInstance}.Buttons.Button{buttonIndex}.Action";
                DialogueAction realAction = Language.Exists(possibleAction) ? Enum.Parse<DialogueAction>(Language.GetTextValue(possibleAction)) : DialogueAction.None;

                string label = Language.GetTextValue($"{DialogueInstance}.Buttons.Button{buttonIndex}.Text");

                buttonBuffer.Add(new GeneratedButtonData(label, realAction, realKey));
                buttonIndex++;
            }
            return [.. buttonBuffer];
        }
        public void TweenSpeakerHead()
        {
            Tweener.Tween(AnimHelpers.CreateFor(this, () => speakerHeadYPositionPercentOffset, () => 1f, 16, EasingFunctions.OutCubic, () =>
            {

            }));
            Tweener.Tween(AnimHelpers.CreateFor(this, () => speakerHeadVerticalScale, () => 1.6f, 16, EasingFunctions.OutCubic, () =>
            {
                Tweener.Tween(AnimHelpers.CreateFor(this, () => speakerHeadVerticalScale, () => 1f, 8, EasingFunctions.OutCubic));
            }));
        }
        public void Setup(string key)
        {
            buttonsOpacity = 0f;
            RemoveAllChildren();
            OnInitialize();
            buttons.Clear();
            tweenReference = null;
            BeginTypewriter(key);
            currentData = DrawingData;
            currentSpeechSounds = SpeechSounds;
        }
        public void Open(string key)
        {
            openProgress = 0f;
            Setup(key);
            tweenReference = (Keyframe<float>)Tweener.Tween(AnimHelpers.CreateFor(this, () => openProgress, () => 1f, 32, EasingFunctions.OutCubic, () =>
            {
                drawSpeakerHead = true;
                TweenSpeakerHead();
            }));
        }
        public void Close()
        {
            if (tweenReference != null)
                Tweener.CancelTween(tweenReference);

            Tweener.Tween(AnimHelpers.CreateFor(this, () => openProgress, () => 0f, 32, EasingFunctions.OutCubic, () =>
            {
                ParentState.ForceClose();
                tweenReference = null;
                writing = false;
                Goal = string.Empty;
                _text = string.Empty;
                speakerKey = string.Empty;
                dialogueKey = string.Empty;
                godSpeaker = string.Empty;
                speakerHeadYPositionPercentOffset = 0f;
                speakerHeadVerticalScale = 1f;
                RemoveAllChildren();
                OnInitialize();
                buttons.Clear();
                drawSpeakerHead = false;
            }));
        }
    }
    public enum DialogueAction : ushort
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        None,
        /// <summary>
        /// Does as it says.
        /// </summary>
        CloseDialogueBox,
        /// <summary>
        /// meme
        /// </summary>
        KillPlayerInstantly,
        /// <summary>
        /// Opens Mudkarp's shop (good luck implementing this, Ajax)
        /// </summary>
        OpenMudkarpShop
    }
    public struct GeneratedButtonData(string label, DialogueAction action, string goTo)
    {
        public string Label = label;
        public DialogueAction Action = action;
        public string GoTo = goTo;
    }
    public class DialogueButton(GeneratedButtonData buttonData) : ITDUIElement()
    {
        public DialogueAction Action { get; private set; } = buttonData.Action;
        public string Label { get; private set; } = buttonData.Label;
        public string GoTo { get; private set; } = buttonData.GoTo;
        public bool Active { get; set; }
        public Asset<Texture2D> Texture { get; set; }
        private Color drawColor = Color.White;
        private float extraSize = 0f;
        private Keyframe<float> tweenReference;
        public void DrawButton(SpriteBatch spriteBatch, Texture2D texture, float opacity, Asset<DynamicSpriteFont> font)
        {
            //Main.NewText(opacity);
            WorldNPCDialogueBox parent = Parent as WorldNPCDialogueBox;
            float realOpacity = opacity;
            if (parent != null)
                realOpacity *= EasingFunctions.OutQuad(parent.openProgress);
            Rectangle bounds = GetDimensions().ToRectangle().Inflated((int)extraSize);
            DrawAdjustableBox(spriteBatch, texture, bounds, drawColor * realOpacity);
            if (Texture != null)
                spriteBatch.Draw(Texture.Value, bounds.Center.ToVector2(), null, Color.White * opacity, 0f, Texture.Size() / 2f, 1f, SpriteEffects.None, 0f);
            Vector2 textSize = font.Value.MeasureString(Label);
            Vector2 textPosition = bounds.Center.ToVector2() - (textSize * 0.5f);
            WorldNPCDialogueBox.DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, Label, textPosition, Color.White * realOpacity, Color.Black * opacity);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            if (tweenReference != null)
            {
                Tweener.CancelTween(tweenReference);
                tweenReference = null;
            }
            tweenReference = (Keyframe<float>)Tweener.Tween(AnimHelpers.CreateFor(this, () => extraSize, () => 16, 8, EasingFunctions.OutCubic, () =>
            {
                tweenReference = null;
            }));
            drawColor = Color.Gray;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        public override void MouseOut(UIMouseEvent evt)
        {
            if (tweenReference != null)
            {
                Tweener.CancelTween(tweenReference);
                tweenReference = null;
            }
            tweenReference = (Keyframe<float>)Tweener.Tween(AnimHelpers.CreateFor(this, () => extraSize, () => 0f, 8, EasingFunctions.OutCubic, () =>
            {
                tweenReference = null;
            }));
            drawColor = Color.White;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            if (!Active)
                return;
            DoAction();
            if (!string.IsNullOrEmpty(GoTo))
                (Parent as WorldNPCDialogueBox).Setup(GoTo);

        }
        public void DoAction()
        {
            switch (Action)
            {
                case DialogueAction.None:
                    break;

                case DialogueAction.CloseDialogueBox:
                    UILoader.GetUIState<WorldNPCDialogue>().Close();
                    break;

                case DialogueAction.KillPlayerInstantly:
                    Main.LocalPlayer.KillMeCustom("MudkarpEvil");
                    goto case DialogueAction.CloseDialogueBox;

                case DialogueAction.OpenMudkarpShop:
                    // (again, good luck implementing this, Ajax)
                    break;
            }
        }
    }
}
