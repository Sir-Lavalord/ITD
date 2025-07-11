using ITD.Content.TileEntities;
using ITD.Content.Tiles;
using ITD.Utilities;
using MonoMod.Cil;
using System;
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

            // fix sorting/quick stack buttons drawing, and also recipe list drawing
            IL_Main.DrawInventory += InventoryITDChestFixes;
            // hello yes this is mr quickstack
            On_Player.QuickStackAllChests += PlayerQuickStackITDChest;

            // funny colors
            On_ItemSlot.SetGlow += ItemGlowITDChestAdjust;
        }

        private void PlayerQuickStackITDChest(On_Player.orig_QuickStackAllChests orig, Player self)
        {
            orig(self);
            int num2 = 39;
            int num3 = (int)(self.Center.X / 16f);
            int num4 = (int)(self.Center.Y / 16f);
            for (int j = num3 - num2; j <= num3 + num2; j++)
            {
                if (j < 0 || j >= Main.maxTilesX)
                {
                    continue;
                }
                for (int k = num4 - num2; k <= num4 + num2; k++)
                {
                    if (k < 0 || k >= Main.maxTilesY)
                    {
                        continue;
                    }
                    int num5 = 0;
                    Tile t = Framing.GetTileSafely(j, k);
                    if (TileLoader.GetTile(t.TileType) is ITDChest)
                        num5 = -1;
                    float range = 600f;
                    if (num5 < 0 && (new Vector2(j * 16 + 8, k * 16 + 8) - self.Center).LengthSquared() < range * range)
                    {
                        ContainerTransferContext context2 = ContainerTransferContext.FromBlockPosition(j, k);
                        Point16 topLeft = TileHelpers.GetTopLeftTileInMultitile(j, k);
                        if (TileEntity.ByPosition.TryGetValue(topLeft, out TileEntity te) && te is ITDChestTE chest)
                        {
                            self.tileEntityAnchor.Set(chest.ID, topLeft.X, topLeft.Y);
                            ChestUI.QuickStack(context2);
                            if (self.useVoidBag())
                            {
                                ChestUI.QuickStack(context2, voidStack: true);
                            }
                            self.tileEntityAnchor.Clear();
                        }
                    }
                }
            }
        }
        private static void ItemGlowITDChestAdjust(On_ItemSlot.orig_SetGlow orig, int index, float hue, bool chest)
        {
            if (!chest && ITDChestTE.IsActiveForLocalPlayer)
                chest = true;
            orig(index, hue, chest);
        }
        private static void InventoryITDChestFixes(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // part 1: fix recipe list not drawing when it should

                // this should be enough
                if (!c.TryGotoNext(
                    i => i.MatchLdsfld<Main>("InReforgeMenu"),
                    i => i.MatchBrtrue(out _),
                    i => i.MatchCall<Main>("get_LocalPlayer"),
                    i => i.MatchLdflda<Player>("tileEntityAnchor")))
                {
                    LogError("Couldn't find instruction sequence for recipe list fix");
                    return;
                }

                c.Index += 2;

                var label = il.DefineLabel();

                c.EmitDelegate(() => 
                {
                    bool flag = (Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked) || Main.hidePlayerCraftingMenu; 
                    return ITDChestTE.IsActiveForLocalPlayer && !flag;
                }
                );
                c.EmitBrtrue(label);

                // find instruction to jump to which should be when -1 is loaded
                if (!c.TryGotoNext(i => i.MatchLdcI4(-1)))
                {
                    LogError("Couldn't find -1 load");
                    return;
                }

                c.MarkLabel(label);

                // part 2: adjust y position of recipe list

                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcR4(410f)))
                {
                    LogError("Couldn't find y position init for avaiable recipe buttons");
                    return;
                }

                c.EmitDelegate<Func<float>>(() =>
                {
                    TileEntity te = Main.LocalPlayer.tileEntityAnchor.GetTileEntity();
                    if (te != null && te is ITDChestTE chest)
                        return (chest.StorageDimensions.Y - 4) * ITDChestTE.FullSlotDim;
                    return 0;
                });

                c.EmitAdd();

                // part 3: fix inventory buttons drawing when they shouldn't

                // try to find this call. this is clean as this is only done once in the whole code
                if (!c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<TileEntity>("OnInventoryDraw")))
                {
                    LogError("OnInventoryDraw call not found");
                    return;
                }

                var skip = il.DefineLabel();
                // get the bool for whether or not we should return
                c.EmitDelegate(() =>
                {
                    return ITDChestTE.IsActiveForLocalPlayer;
                });

                // branch and return
                c.EmitBrtrue(skip);

                c.MarkLabel(skip);
                c.EmitRet();
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void EmotesITDChestAdjust(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // find the instructions to load 534 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(534)))
                {
                    LogError("Couldn't find X position loading");
                }
                // add our own amount
                c.EmitDelegate(() =>
                {
                    Player player = Main.LocalPlayer;
                    var anchor = player.tileEntityAnchor;
                    TileEntity te = anchor.GetTileEntity();
                    if (te != null && te is ITDChestTE chest)
                    {
                        return 5 + chest.UIOffsetX * ITDChestTE.FullSlotDim;
                    }
                    return 0;
                });
                // add
                c.EmitAdd();

                // find the instructions to load num4 and 4 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchAdd(), i => i.MatchLdcI4(4), i => i.MatchAdd()))
                {
                    LogError("num4 and 4 load instructions not found");
                    return;
                }
                // now let's add our own amount to the thing
                c.EmitDelegate(() =>
                {
                    if (ITDChestTE.IsActiveForLocalPlayer)
                        return 4 * ITDChestTE.FullSlotDim;
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

                // find the instructions to load 498 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(498)))
                {
                    LogError("Couldn't find X position loading");
                }
                // add our own amount
                c.EmitDelegate(() =>
                {
                    Player player = Main.LocalPlayer;
                    var anchor = player.tileEntityAnchor;
                    TileEntity te = anchor.GetTileEntity();
                    if (te != null && te is ITDChestTE chest)
                    {
                        return 5 + chest.UIOffsetX * ITDChestTE.FullSlotDim;
                    }
                    return 0;
                });
                // add
                c.EmitAdd();

                // find the instructions to load num4 and 4 onto the stack
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchAdd(), i => i.MatchLdcI4(4), i => i.MatchAdd()))
                {
                    LogError("Couldn't find Y position loading");
                    return;
                }
                // now let's add our own amount to the thing
                c.EmitDelegate(() =>
                {
                    if (ITDChestTE.IsActiveForLocalPlayer)
                        return 4 * ITDChestTE.FullSlotDim;
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
