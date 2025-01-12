using ITD.Systems.WorldNPCs;
using ITD.Utilities;
using ITD.Utilities.EntityAnim;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.UI
{
    public class WorldNPCDialogue : ITDUIState
    {
        private WorldNPCDialogueBox dialogueBox;
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
        public void Open()
        {
            isClosing = false;
            if (isOpen)
                return;
            dialogueBox.Open();
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
        public const int BoxPaddingSides = 72;
        public const int BoxPaddingDown = 12;
        private Vector2 TextPadding = new(12, 8);
        private bool writing = false;
        private string _text = "";
        private int textStep;
        private int previousTextStep;
        private const int textSpeed = 6;
        /// <summary>
        /// Not in frames, but in text steps. 0 will play a voice clip for every single letter typed.
        /// </summary>
        private int voiceFrequency = 1;
        public float openProgress;
        public string Goal { get; set; }
        public WorldNPCDialogue ParentState => Parent as WorldNPCDialogue;
        public Keyframe<float> tweenReference;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // those who snow
            //Top.Set()
            Rectangle bounds = GetDimensions().ToRectangle();
            float boxOpacity = EasingFunctions.OutQuad(openProgress);
            DrawAdjustableBox(spriteBatch, GetTexture().Value, bounds, Color.White * boxOpacity);
            var lines = Utils.WordwrapStringSmart(_text, Color.White, font.Value, 460, 10);
            base.DrawSelf(spriteBatch);
        }
        public Asset<Texture2D> GetTexture()
        {
            int worldNPC = Main.LocalPlayer.GetITDPlayer().TalkWorldNPC;
            if (worldNPC == -1)
            {
                return ModContent.Request<Texture2D>("ITD/Systems/WorldNPCs/Assets/BoxStyles/DefaultBoxStyle");
                //return null;
            }
            WorldNPC singleton = NPCLoader.GetNPC(Main.npc[worldNPC].type) as WorldNPC;
            return singleton.DialogueBoxStyle;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //Main.NewText(ParentState.isOpen);

        }
        public void Open()
        {
            openProgress = 0f;
            tweenReference = (Keyframe<float>)Tweener.Tween(AnimHelpers.CreateFor(this, () => openProgress, () => 1f, 32, EasingFunctions.OutCubic, () => tweenReference = null));
        }
        public void Close()
        {
            if (tweenReference != null)
                Tweener.CancelTween(tweenReference);

            Tweener.Tween(AnimHelpers.CreateFor(this, () => openProgress, () => 0f, 32, EasingFunctions.OutCubic, () =>
            {
                ParentState.ForceClose();
                tweenReference = null;
            }));
        }
    }
}
