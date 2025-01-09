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
        public void Open()
        {
            if (isOpen)
                return;
            dialogueBox.Open();
        }
    }
    /// <summary>
    /// ported from my game #shamelessplug
    /// </summary>
    public class WorldNPCDialogueBox : ITDUIElement
    {
        private readonly Asset<DynamicSpriteFont> font = FontAssets.MouseText;
        private const int BoxPaddingSides = 72;
        private const int BoxPaddingDown = 12;
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
        private float boxHeight;
        private float boxBottom;
        public string Goal { get; set; }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // those who snow
            //Top.Set()
            var state = Parent as WorldNPCDialogue;
            Rectangle bounds = GetDimensions().ToRectangle();
            DrawAdjustableBox(spriteBatch, GetTexture().Value, bounds, Color.White);
            var lines = Utils.WordwrapStringSmart(_text, Color.White, font.Value, 460, 10);
            base.DrawSelf(spriteBatch);
        }
        public Asset<Texture2D> GetTexture()
        {
            int worldNPC = Main.LocalPlayer.GetITDPlayer().TalkWorldNPC;
            if (worldNPC == -1)
                return null;
            WorldNPC singleton = NPCLoader.GetNPC(Main.npc[worldNPC].type) as WorldNPC;
            return singleton.DialogueBoxStyle;
        }
        public void Open()
        {
            //Tweener.Tween(AnimHelpers.CreateFor<float>(this, () => ))
        }
    }
}
