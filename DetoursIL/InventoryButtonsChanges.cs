using System;
using System.Linq;
using ITD.Content.TileEntities;
using MonoMod.Cil;
using Terraria.DataStructures;
using Terraria.UI;

namespace ITD.DetoursIL
{
    public class InventoryButtonsChanges : DetourGroup
    {
        public override void Load()
        {
            // not entirely sure why these are separate methods when they're basically the same method just with different x. even the IL edits are the exact same
            IL_Main.DrawBestiaryIcon += BestiaryITDChestAdjust;
            IL_Main.DrawEmoteBubblesButton += EmotesITDChestAdjust;
        }

        private static void EmotesITDChestAdjust(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // find the instructions to load num4 and 4 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchAdd(), i => i.MatchLdcI4(4), i => i.MatchAdd()))
                {
                    LogError("EmotesITDChestAdjust: num4 and 4 load instructions not found");
                    return;
                }
                // now let's add our own amount to the thing
                c.EmitDelegate(() =>
                {
                    Player player = Main.LocalPlayer;
                    var anchor = player.tileEntityAnchor;
                    TileEntity te = anchor.GetTileEntity();
                    if (te != null && te is ITDChestTE chest)
                    {
                        return chest.StorageDimensions.Y * 36;
                    }
                    return 0;
                });
                // add the calculated thing
                c.EmitAdd();
            }
            catch
            {
                DumpIL(il);
            }
        }
        private static void BestiaryITDChestAdjust(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // find the instructions to load num4 and 4 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchAdd(), i => i.MatchLdcI4(4), i => i.MatchAdd()))
                {
                    LogError("BestiaryITDChestAdjust: num4 and 4 load instructions not found");
                    return;
                }
                // now let's add our own amount to the thing
                c.EmitDelegate(() =>
                {
                    Player player = Main.LocalPlayer;
                    var anchor = player.tileEntityAnchor;
                    TileEntity te = anchor.GetTileEntity();
                    if (te != null && te is ITDChestTE chest)
                    {
                        return chest.StorageDimensions.Y * 36;
                    }
                    return 0;
                });
                // add the calculated thing
                c.EmitAdd();
            }
            catch
            {
                DumpIL(il);
            }
        }
    }
}
