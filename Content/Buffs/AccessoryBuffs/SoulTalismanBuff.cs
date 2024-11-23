using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.AccessoryBuffs
{
    public class SoulTalismanBuff : ModBuff
    {
        public static readonly int Stacks = 5;
        public static readonly int AnimationSpeed = 60;
        int CurrentStack;
        public static readonly string AnimationSheetPath = "ITD/Content/Buffs/AccessoryBuffs/SoulTalismanBuff_Anim";
        private Asset<Texture2D> animatedTexture;

        public override void SetStaticDefaults()
        {
            animatedTexture = ModContent.Request<Texture2D>(AnimationSheetPath);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
        {
            Texture2D ourTexture = animatedTexture.Value;
            Rectangle ourSourceRectangle = ourTexture.Frame(verticalFrames: Stacks, frameY: CurrentStack % Stacks);

            
            drawParams.Texture = ourTexture;
            drawParams.SourceRectangle = ourSourceRectangle;
            return true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            CurrentStack = player.GetITDPlayer().soulTalismanStack;
        }
        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.GetITDPlayer().soulTalismanStack++;
            return true;
        }
    }
}