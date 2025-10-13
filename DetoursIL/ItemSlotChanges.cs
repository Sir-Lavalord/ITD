using ITD.Content.TileEntities;
using ITD.Content.Tiles;
using MonoMod.Cil;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.UI;

namespace ITD.DetoursIL;

public class ItemSlotChanges : DetourGroup
{
    private static int maxArrayNewSize = 40;
    public override void Load()
    {
        foreach (var type in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDChest))))
        {
            ITDChest instance = (ITDChest)Activator.CreateInstance(type);
            maxArrayNewSize = Math.Max(maxArrayNewSize, instance.StorageDimensions.X * instance.StorageDimensions.Y);
        }
        // this is necessary so the game doesn't crash
        Array.Resize(ref UIAccessors.GetInventoryGlowTime(null), maxArrayNewSize + 18);
        Array.Resize(ref UIAccessors.GetInventoryGlowHue(null), maxArrayNewSize + 18);
        Array.Resize(ref UIAccessors.GetInventoryGlowTimeChest(null), maxArrayNewSize + 18);
        Array.Resize(ref UIAccessors.GetInventoryGlowHueChest(null), maxArrayNewSize + 18);

        IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += SkipGamepadThing;
    }

    private static void SkipGamepadThing(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            // find this sequence of instructions
            if (!c.TryGotoNext(i => i.MatchLdloc(out _), i => i.MatchLdarg(out _), i => i.MatchLdloc(out _), i => i.MatchLdcR4(0.75f)))
            {
                LogError("Instructions not found");
                return;
            }
            // define a label for branching
            var skipLabel = il.DefineLabel();
            // get the bool that tells us whether or not we should skip the gamepad thing or not
            c.EmitDelegate(() =>
            {
                TileEntity te = Main.LocalPlayer.tileEntityAnchor.GetTileEntity();
                return te != null && te is ITDChestTE chest && chest.Dimensions != new Systems.DataStructures.Point8(10, 4);
            });
            // branch
            c.EmitBrtrue(skipLabel);
            // go to ret to mark the label
            if (!c.TryGotoNext(i => i.MatchRet()))
            {
                LogError("Couldn't find method end");
                return;
            }
            // mark it
            c.MarkLabel(skipLabel);
        }
        catch
        {
            DumpIL(il);
        }
    }
}
