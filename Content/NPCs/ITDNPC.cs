using System;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.NPCs
{
    public abstract class ITDNPC : ModNPC
    {
        public static LocalizedText BestiaryEntry { get; set; }
        public Rectangle HitboxTiles { get { return new Rectangle((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16), NPC.width / 16, NPC.height / 16); } }
        public Rectangle BigHitboxTiles => MiscHelpers.TileRectangle(NPC);
        public bool InvalidTarget { get { return NPC.target < 0 || NPC.target == 255 || !Main.player[NPC.target].Exists(); } }
        public SpriteEffects CommonSpriteDirection { get { return NPC.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; } }
        /// <summary>
        /// Allows you to do stuff when this NPC is right clicked.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnRightClick(Player player)
        {

        }
        /// <summary>
        /// Use this for loops. Big flexibility
        /// </summary>
        /// <param name="frameHeight"></param>
        /// <param name="minFrame"></param>
        /// <param name="maxFrame">If left null, this will default to <see cref="Main.npcFrameCount"/> indexed with type, minus one</param>
        /// <param name="maxCounter"></param>
        /// <param name="counterIncrement"></param>
        public void CommonFrameLoop(int frameHeight, int minFrame = 0, int? maxFrame = null, float maxCounter = 5f, float counterIncrement = 1f)
        {
            // get the real max frame value
            int realMaxFrame = maxFrame ?? Main.npcFrameCount[Type] - 1;
            // clamp to min
            if (NPC.frame.Y < minFrame * frameHeight)
                NPC.frame.Y = minFrame * frameHeight;
            // increase counter by the increment
            NPC.frameCounter += counterIncrement;
            // change frames
            if (NPC.frameCounter > maxCounter)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > frameHeight * realMaxFrame)
                {
                    NPC.frame.Y = frameHeight * minFrame;
                }
            }
        }
        public void HideFromBestiary()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public void StepUp()
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }
    }
}
