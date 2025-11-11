using Terraria.DataStructures;

namespace ITD.Content.Buffs.EquipmentBuffs;

public class WickedHeartBuff : ModBuff
{
    public const int Stacks = 5;
    public const int AnimationSpeed = 60;
    int CurrentStack;
    public const string AnimationSheetPath = "ITD/Content/Buffs/EquipmentBuffs/SoulTalismanBuff_Anim";
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
        var modPlayer = player.ITD();
        modPlayer.wickedHeartEffect = true;
        if (player.IsLocalPlayer() && player.buffTime[buffIndex] == 2)
            CurrentStack--;
    }

    public override bool ReApply(Player player, int time, int buffIndex)
    {
        if (player.IsLocalPlayer())
            CurrentStack++;
        return false;
    }
}