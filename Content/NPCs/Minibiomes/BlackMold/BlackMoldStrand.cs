namespace ITD.Content.NPCs.Minibiomes.BlackMold;

public class BlackMoldStrand : ITDNPC
{
    public override string Texture => ITD.BlankTexture;
    public override void SetStaticDefaultsSafe()
    {
        HiddenFromBestiary = true;
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
}
