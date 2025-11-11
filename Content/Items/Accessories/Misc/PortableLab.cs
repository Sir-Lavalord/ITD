using ITD.Utilities.Placeholders;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ITD.Content.Items.Accessories.Misc;

public class PortableLab : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override string Texture => Placeholder.PHAxe;
    public override void Load()
    {
        IL_Player.AdjTiles += static (il) =>
        {
            var c = new ILCursor(il);

            // find the alchemyTable = false statement
            c.GotoNext(MoveType.After, i => i.MatchStfld(typeof(Player), "alchemyTable"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(ApplyChange);
        };
    }
    private static void ApplyChange(Player player)
    {
        if (player.ITD().portableLab)
        {
            player.adjTile[TileID.Bottles] = true;
            player.adjTile[TileID.AlchemyTable] = true;
            player.alchemyTable = true;
        }
    }
    public override void SetDefaults()
    {
        Item.DefaultToAccessory();
    }
    public override void UpdateInfoAccessory(Player player)
    {
        player.ITD().portableLab = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.ITD().portableLab = true;
    }
}
