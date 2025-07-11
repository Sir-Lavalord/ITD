using ITD.Utilities;
using Terraria.DataStructures;

namespace ITD.Content.Buffs.EquipmentBuffs
{
    public class SoulTalismanBuff : ModBuff
    {
        public static readonly int Stacks = 5;
        public static readonly int AnimationSpeed = 60;
        int CurrentStack;
        public static readonly string AnimationSheetPath = "ITD/Content/Buffs/EquipmentBuffs/SoulTalismanBuff_Anim";
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
            var modPlayer = player.GetITDPlayer();
            modPlayer.soulTalismanEffect = true;
            CurrentStack = player.GetITDPlayer().soulTalismanStack;
        }
        public override bool ReApply(Player player, int time, int buffIndex)
        {
            var modPlayer = player.GetITDPlayer();

            if (modPlayer.soulTalismanStack < 4)
            {
                modPlayer.soulTalismanStack++;
            }
            if (player.buffTime[buffIndex] <= 1200)
            {
                if (player.buffTime[buffIndex] + time >= 1200)
                    player.buffTime[buffIndex] = 1200;
                else
                player.buffTime[buffIndex] +=  time;
            }
            if (player.buffTime[buffIndex] >= 1200)
            {
                player.buffTime[buffIndex] = 1200;
            }
            return false;
        }
    }
}