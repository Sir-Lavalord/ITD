using ITD.Systems.WorldNPCs;
using ITD.Utilities;
using ITD.Utilities.EntityAnim;
using ITD.Utilities.Placeholders;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

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
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Markers"));
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
        public string speakerKey;
        public string dialogueKey;
        public string Goal { get; set; }

        public float speakerHeadYPositionPercentOffset;
        public float speakerHeadVerticalScale = 1f;
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
        public Keyframe<float> tweenReference;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetDimensions().ToRectangle();
            float boxOpacity = EasingFunctions.OutQuad(openProgress);
            Texture2D boxStyle = GetTexture().Value;

            // draw speaker head
            if (!string.IsNullOrEmpty(speakerKey))
            {
                Texture2D head = currentData.Texture.Value;
                byte frameCount = currentData.FrameCount;
                Rectangle frame = head.Frame(1, frameCount, 0, speakerHeadFrame);
                Vector2 drawPos = new(bounds.X + TextPadding.X + head.Width / 2, bounds.Y + head.Height / 2 - (head.Height * speakerHeadYPositionPercentOffset));
                Vector2 drawScale = new(1f / speakerHeadVerticalScale, speakerHeadVerticalScale);
                Vector2 origin = new(head.Width / 2, head.Height / frameCount / 2);
                spriteBatch.Draw(head, drawPos, frame, Color.White * boxOpacity, 0f, origin, drawScale, SpriteEffects.None, 0f);
            }

            // draw label box
            string worldNPCKey = $"{DialogueLanguageKey}.{speakerKey}.Name";

            string label = Language.Exists(worldNPCKey) ? Language.GetTextValue(worldNPCKey) : "...";
            int labelWidth = (int)(font.Value.MeasureString(label).X + TextPadding.X * 2);
            int labelHeight = 42;
            Rectangle labelRect = new(bounds.X + bounds.Width - labelWidth, bounds.Y - labelHeight, labelWidth, labelHeight + boxStyle.Height / 3);
            DrawAdjustableBox(spriteBatch, boxStyle, labelRect, Color.White * boxOpacity);
            DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, label, labelRect.Location.ToVector2() + new Vector2(TextPadding.X, labelHeight / 3), Color.White * boxOpacity, Color.Black * boxOpacity);
            
            DrawAdjustableBox(spriteBatch, boxStyle, bounds, Color.White * boxOpacity);
            var lines = TextHelpers.WordwrapStringSuperSmart(_text, Color.White, font.Value, bounds.Width - (int)(TextPadding.X * 1.9f), 10);
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                Vector2 drawPos = new(bounds.X + TextPadding.X, bounds.Y + TextPadding.Y + (i * 32f));
                DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, [.. line], drawPos, Color.White * boxOpacity, Color.Black * boxOpacity);
                
            }
            base.DrawSelf(spriteBatch);
        }
        private static void DrawColorCodedStringWithShadowWithValidOpacity(SpriteBatch spriteBatch, Asset<DynamicSpriteFont> font, string text, Vector2 position, Color color, Color shadowColor, float scale = 1f, float spread = 2f) => DrawColorCodedStringWithShadowWithValidOpacity(spriteBatch, font, [..ChatManager.ParseMessage(text, color)], position, color, shadowColor, scale, spread);
        private static void DrawColorCodedStringWithShadowWithValidOpacity(SpriteBatch spriteBatch, Asset<DynamicSpriteFont> font, TextSnippet[] snippets, Vector2 position, Color color, Color shadowColor, float scale = 1f, float spread = 2f)
        {
            for (int i = 0; i < ChatManager.ShadowDirections.Length; i++)
            {
                ChatManager.DrawColorCodedString(spriteBatch, font.Value, snippets, position + ChatManager.ShadowDirections[i] * spread, shadowColor, 0f, Vector2.Zero, new Vector2(scale), out var _, -1, ignoreColors: true);
            }
            ChatManager.DrawColorCodedString(spriteBatch, font.Value, snippets, position, color, 0f, Vector2.Zero, new Vector2(scale), out var _, -1, ignoreColors: true);
        }
        public Asset<Texture2D> GetTexture()
        {
            WorldNPC talk = TalkNPC;
            if (talk is null)
                return ModContent.Request<Texture2D>("ITD/Systems/WorldNPCs/Assets/BoxStyles/DefaultBoxStyle");
            return talk.DialogueBoxStyle;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (writing)
            {
                if (++textTimer > textSpeed)
                {
                    if (textStep + 1 > Goal.Length) // message has ended, stop writing.
                    {
                        writing = false;
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
        private bool IsInIncompleteTag(string text, int step)
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
            string lookForKey = DialogueInstance;
            string lookForValidBody = $"{lookForKey}.Body";
            if (!Language.Exists(lookForValidBody))
            {
                Main.NewText($"Error when trying to begin typewriter: the given key, {lookForKey}, was not found to have a valid Body.", Color.Red);
                return;
            }
            Goal = Language.GetTextValue(lookForValidBody);
            textStep = 0;
            writing = true;
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
        public void Open(string key)
        {
            openProgress = 0f;
            tweenReference = (Keyframe<float>)Tweener.Tween(AnimHelpers.CreateFor(this, () => openProgress, () => 1f, 32, EasingFunctions.OutCubic, () =>
            {
                tweenReference = null;
                BeginTypewriter(key);
                currentData = DrawingData;
                currentSpeechSounds = SpeechSounds;
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
                speakerHeadYPositionPercentOffset = 0f;
                speakerHeadVerticalScale = 1f;
            }));
        }
    }
}
