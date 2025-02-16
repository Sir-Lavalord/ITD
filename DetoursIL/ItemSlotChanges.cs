using ITD.Content.TileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Terraria.UI;
using Terraria.DataStructures;

namespace ITD.DetoursIL
{
    public class ItemSlotChanges : DetourGroup
    {
        private static int maxArrayNewSize = 40;
        public override void Load()
        {
            foreach (var type in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDChestTE))))
            {
                ITDChestTE instance = (ITDChestTE)Activator.CreateInstance(type);
                maxArrayNewSize = Math.Max(maxArrayNewSize, instance.StorageDimensions.X * instance.StorageDimensions.Y);
            }
            // this is necessary so the game doesn't crash
            Array.Resize(ref ItemSlotAccessors.GetInventoryGlowTime(null), maxArrayNewSize + 18);
            Array.Resize(ref ItemSlotAccessors.GetInventoryGlowHue(null), maxArrayNewSize + 18);
            Array.Resize(ref ItemSlotAccessors.GetInventoryGlowTimeChest(null), maxArrayNewSize + 18);
            Array.Resize(ref ItemSlotAccessors.GetInventoryGlowHueChest(null), maxArrayNewSize + 18);

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
                    LogError("SkipGamepadThing: Instructions not found");
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
                    LogError("SkipGamepadThing: Couldn't find method end");
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
}
