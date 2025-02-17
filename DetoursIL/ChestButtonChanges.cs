using ITD.Content.TileEntities;
using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.UI;
using MonoMod.Cil;
using Terraria.Localization;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using UtfUnknown.Core.Models.SingleByte.Finnish;

namespace ITD.DetoursIL
{
    public class ChestButtonChanges : DetourGroup
    {
        private const int SpecialKey = 9512;
        public override void Load()
        {
            // fix the resetti
            On_ChestUI.Draw += ButtonValuesITDChestAdjust;

            // button functions!
            On_ChestUI.LootAll += LootAllITDChest;
            On_ChestUI.DepositAll += DepositAllITDChest;
            On_ChestUI.QuickStack += QuickStackITDChest;
            On_IngameFancyUI.OpenVirtualKeyboard += HijackOpenVirtualKeyboard;
            IL_IngameFancyUI.OpenVirtualKeyboard += OpenVirtualKeyboardITDChest;
            On_ChestUI.RenameChestSubmit += RenameChestSubmitITDChest;
            On_ChestUI.Restock += RestockITDChest;
            On_ItemSorting.SortChest += SortItemsITDChest;

            // fix another three resettis
            // who the fuck thought of this???
            IL_Main.DoUpdate_WhilePaused += UpdatePausedITDChest;
            IL_Player.Update += UpdatePlayerITDChest;
            IL_UIVirtualKeyboard.DrawSelf += DrawUIVirtualKeyboardITDChest;

            // fix this to allow for larger chests
            IL_ChestUI.TryPlacingInChest += TryPlacingInITDChest;
            On_ChestUI.GetContainerUsageInfo += GetContainerUsageInfoITDChest;

            // there's hardcoded checks for context 3 and chest here that crash the game in multiplayer
            IL_ItemSlot.LeftClick_ItemArray_int_int += LeftClickITDChest;
        }

        private static void LeftClickITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                
                // this is the start of the senddata call. now we can skip
                if (!c.TryGotoNext(i => i.MatchLdcI4(32)))
                {
                    LogError("Couldn't find start of NetMessage.SendData variable loading");
                }

                var skipLabel = il.DefineLabel();

                // decide whether or not to skip
                c.EmitDelegate(() => ITDChestTE.IsActiveForLocalPlayer);

                c.EmitBrtrue(skipLabel);

                // now let's move after it for actually skipping
                if (!c.TryGotoNext(MoveType.After, i => i.MatchCall<NetMessage>("SendData")))
                {
                    LogError("Couldn't find SendData call");
                }

                // we will jump here to avoid both calls being calledeleld
                var endLabel = il.DefineLabel();

                // jamp
                c.EmitBr(endLabel);

                // we wanna skip to here
                c.MarkLabel(skipLabel);

                // load the slot onto the stack
                c.EmitLdarg2();
                c.EmitDelegate<Action<int>>(slot =>
                {
                    NetSystem.SendPacket(new SyncITDChestItemPacket(ITDChestTE.GetITDChest().ID, slot));
                });

                // end
                c.MarkLabel(endLabel);
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void GetContainerUsageInfoITDChest(On_ChestUI.orig_GetContainerUsageInfo orig, out bool sync, out Item[] chestinv)
        {
            orig(out sync, out chestinv);
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                chestinv = ITDChestTE.GetITDChest().items;
                sync = true;
            }
        }

        private static void TryPlacingInITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                // we can do this in a couple passes. getting rid of all of these magic numbers is the easiest part.
                while (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(40)))
                {
                    c.EmitDelegate(() =>
                    {
                        if (ITDChestTE.IsActiveForLocalPlayer)
                            return ITDChestTE.GetITDChest().TotalSlots - 40;
                        return 0;
                    });
                    c.EmitAdd();
                }
                // now that that's done, let's go back and hijack the senddata calls
                c.Index = 0;

                for (int i = 0; i < 2; i++)
                {
                    // find the beginning of when the variables start to load for the SendData call
                    if (!c.TryGotoNext(i => i.MatchLdcI4(32)))
                    {
                        LogError("Couldn't find NetMessage.SendData loading start");
                    }

                    // first label
                    var skipLabel0 = il.DefineLabel();

                    // get this bool
                    c.EmitDelegate(() =>
                    {
                        return ITDChestTE.IsActiveForLocalPlayer;
                    });

                    // if ITDChestTE.IsActiveForLocalPlayer, then skip the SendData call.
                    c.EmitBrtrue(skipLabel0);

                    if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc2()))
                    {
                        LogError("Couldn't find local variable 2 loading");
                    }

                    int localI = 0;

                    if (!c.TryGotoNext(i => i.MatchLdloc(out localI)))
                    {
                        LogError("Couldn't find for loop variable i loading");
                    }

                    if (!c.TryGotoNext(MoveType.After, i => i.MatchCall<NetMessage>("SendData")))
                    {
                        LogError("Couldn't find SendData call");
                    }

                    c.MarkLabel(skipLabel0);

                    // here's a problem. if our bool is false, then the original code will run, but so will our code. so let's branch once more

                    var skipLabel01 = il.DefineLabel();

                    // not exactly the most graceful way of doing this, we could've introduced a new local instead, but this works
                    c.EmitDelegate(() =>
                    {
                        return ITDChestTE.IsActiveForLocalPlayer;
                    });

                    c.EmitBrfalse(skipLabel01);

                    c.EmitLdloc(localI);
                    c.EmitDelegate<Action<int>>(item =>
                    {
                        NetSystem.SendPacket(new SyncITDChestItemPacket(ITDChestTE.GetITDChest().ID, item));
                    });

                    if (!c.TryGotoNext(i => i.MatchLdloc(out _)))
                    {
                        LogError("Couldn't find branch to loop condition");
                    }

                    c.MarkLabel(skipLabel01);
                }
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void SortItemsITDChest(On_ItemSorting.orig_SortChest orig)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                ITDChestTE chest = ITDChestTE.GetITDChest();

                Item[] item = chest.items;
                Tuple<int, int, int>[] array = new Tuple<int, int, int>[chest.TotalSlots];
                for (int i = 0; i < chest.TotalSlots; i++)
                {
                    array[i] = Tuple.Create(item[i].netID, item[i].stack, item[i].prefix);
                }
                UIAccessors.CallSort(null, item);
                Tuple<int, int, int>[] array2 = new Tuple<int, int, int>[chest.TotalSlots];
                for (int j = 0; j < chest.TotalSlots; j++)
                {
                    array2[j] = Tuple.Create(item[j].netID, item[j].stack, item[j].prefix);
                }
                if (!Main.dedServ)
                {
                    for (int k = 0; k < chest.TotalSlots; k++)
                    {
                        if (array2[k] != array[k])
                        {
                            NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, k));
                        }
                    }
                }
                return;
            }
            orig();
        }

        private static void RestockITDChest(On_ChestUI.orig_Restock orig)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                ITDChestTE chest = ITDChestTE.GetITDChest();
                Player player = Main.LocalPlayer;
                Item[] inventory = player.inventory;
                Item[] item = chest.items;
                HashSet<int> hashSet = [];
                List<int> list = [];
                List<int> list2 = [];
                for (int num = 57; num >= 0; num--)
                {
                    if ((num < 50 || num >= 54) && (inventory[num].type < ItemID.CopperCoin || inventory[num].type > ItemID.PlatinumCoin))
                    {
                        if (inventory[num].stack > 0 && inventory[num].maxStack > 1)
                        {
                            hashSet.Add(inventory[num].netID);
                            if (inventory[num].stack < inventory[num].maxStack)
                            {
                                list.Add(num);
                            }
                        }
                        else if (inventory[num].stack == 0 || inventory[num].netID == 0 || inventory[num].type == 0)
                        {
                            list2.Add(num);
                        }
                    }
                }
                bool flag = false;
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[i].stack < 1 || !hashSet.Contains(item[i].netID))
                    {
                        continue;
                    }
                    bool flag2 = false;
                    for (int j = 0; j < list.Count; j++)
                    {
                        int num2 = list[j];
                        int context = 0;
                        if (num2 >= 50)
                        {
                            context = 2;
                        }
                        if (inventory[num2].netID != item[i].netID || ItemSlot.PickItemMovementAction(inventory, context, num2, item[i]) == -1 || !ItemLoader.TryStackItems(inventory[num2], item[i], out var _))
                        {
                            continue;
                        }
                        flag = true;
                        if (inventory[num2].stack == inventory[num2].maxStack)
                        {
                            if (!Main.dedServ)
                                NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, i));
                            list.RemoveAt(j);
                            j--;
                        }
                        if (item[i].stack == 0)
                        {
                            item[i] = new Item();
                            flag2 = true;
                            if (!Main.dedServ)
                                NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, i));
                            break;
                        }
                    }
                    if (flag2 || list2.Count <= 0 || item[i].ammo == 0)
                    {
                        continue;
                    }
                    for (int k = 0; k < list2.Count; k++)
                    {
                        int context2 = 0;
                        if (list2[k] >= 50)
                        {
                            context2 = 2;
                        }
                        if (ItemSlot.PickItemMovementAction(inventory, context2, list2[k], item[i]) != -1)
                        {
                            Utils.Swap(ref inventory[list2[k]], ref item[i]);
                            if (!Main.dedServ)
                                NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, i));
                            list.Add(list2[k]);
                            list2.RemoveAt(k);
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                return;
            }
            orig();
        }

        private static void DrawUIVirtualKeyboardITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                if (!c.TryGotoNext(i => i.MatchCall<ChestUI>("RenameChestCancel")))
                {
                    LogError("ChestUI RenameChestCancel call not found");
                }

                var skipLabel = il.DefineLabel();

                c.EmitDelegate(() => ITDChestTE.IsActiveForLocalPlayer);

                c.EmitBrtrue(skipLabel);

                if (!c.TryGotoNext(i => i.MatchLdsfld<Main>("inputTextEscape")))
                {
                    LogError("Load static field inputTextEscape not found");
                }

                c.MarkLabel(skipLabel);
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void UpdatePlayerITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // this method is gigantic so let's match as many isntructions as possible
                if (!c.TryGotoNext(MoveType.After,
                    i => i.MatchLdsfld<Main>("player"),
                    i => i.MatchLdsfld<Main>("myPlayer"),
                    i => i.MatchLdelemRef(),
                    i => i.MatchLdfld<Player>("chest"),
                    i => i.MatchLdcI4(-1),
                    i => i.MatchBneUn(out _),
                    i => i.MatchLdcI4(0),
                    i => i.MatchStsfld<Main>("editChest")))
                {
                    LogError("Couldn't find correct place in Player.Update");
                    return;
                }

                c.EmitDelegate(() =>
                {
                    if (ITDChestTE.IsActiveForLocalPlayer)
                    {
                        Main.editChest = true;
                    }
                });
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void UpdatePausedITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // get to the general area where we wanna reset the bool
                if (!c.TryGotoNext(i => i.MatchLdsfld<Main>("editChest"), i => i.MatchBrfalse(out _)))
                {
                    LogError("Couldn't find start of condition");
                }

                // get to where the branch happens
                if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(0), i => i.MatchStsfld<Main>("editChest")))
                {
                    LogError("Couldn't find end of branch");
                }

                // reset back to true if our flag is true
                c.EmitDelegate(() =>
                {
                    if (ITDChestTE.IsActiveForLocalPlayer)
                    {
                        Main.editChest = true;
                    }
                });
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void OpenVirtualKeyboardITDChest(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // try to find this instruction, which is right before the switch case
                if (!c.TryGotoNext(MoveType.After, i => i.MatchStloc0()))
                {
                    LogError("Storing string.Empty in Loc0 not found");
                    return;
                }

                // load the context from args
                c.EmitLdarg0();
                // this delegate takes in the context, and outputs the text for the label. returning that string here is clever as we don't have to do much afterwards
                c.EmitDelegate<Func<int, string>>(context =>
                {
                    if (context == SpecialKey)
                    {
                        TileEntity te = Main.LocalPlayer.tileEntityAnchor.GetTileEntity();
                        if (te != null)
                        {
                            if (te is ITDChestTE chest)
                            {
                                Main.npcChatText = chest.StorageName;
                            }
                            Tile t = Framing.GetTileSafely(te.Position);
                            Main.defaultChestName = TileLoader.DefaultContainerName(t.TileType, t.TileFrameX, t.TileFrameY);
                        }
                        Main.editChest = true;
                        return Language.GetTextValue("UI.EnterNewName");
                    }
                    return "";
                });
                // assign that string into labelText
                c.EmitStloc0();

                // now for the second part. we can actually just set context back to 2 for this. let's go to the Main.clrInput call
                if (!c.TryGotoNext(MoveType.After, i => i.MatchCall<Main>("clrInput")))
                {
                    LogError("clrInput call not found");
                    return;
                }

                // load 2 onto the stack
                c.EmitLdcI4(2);
                // reassign context as 2
                c.EmitStarg(0);
            }
            catch
            {
                DumpIL(il);
            }
        }

        private static void HijackOpenVirtualKeyboard(On_IngameFancyUI.orig_OpenVirtualKeyboard orig, int keyboardContext)
        {
            if (keyboardContext == 2 && ITDChestTE.IsActiveForLocalPlayer)
                keyboardContext = SpecialKey;
            orig(keyboardContext);
        }

        private static void RenameChestSubmitITDChest(On_ChestUI.orig_RenameChestSubmit orig, Player player)
        {
            TileEntity te = player.tileEntityAnchor.GetTileEntity();
            if (te != null && te is ITDChestTE chest)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                if (Main.npcChatText == Main.defaultChestName)
                {
                    Main.npcChatText = "";
                }
                if (chest.StorageName != Main.npcChatText)
                {
                    chest.StorageName = Main.npcChatText;
                    if (!Main.dedServ)
                        NetSystem.SendPacket(new SyncITDChestNamePacket(chest.ID));
                }
                return;
            }
            orig(player);
        }

        private static void QuickStackITDChest(On_ChestUI.orig_QuickStack orig, ContainerTransferContext context, bool voidStack)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                Player player = Main.LocalPlayer;
                ITDChestTE chest = ITDChestTE.GetITDChest();
                Item[] array = player.inventory;
                if (voidStack)
                {
                    array = player.bank4.item;
                }
                Vector2 center = player.Center;
                Vector2 containerWorldPosition = context.GetContainerWorldPosition();
                bool canVisualizeTransfers = context.CanVisualizeTransfers;
                Item[] item = player.bank.item;
                item = chest.items;
                List<int> list = [];
                List<int> list2 = [];
                List<int> list3 = [];
                Dictionary<int, int> dictionary = [];
                List<int> list4 = [];
                bool[] array2 = new bool[item.Length];
                for (int i = 0; i < 40; i++)
                {
                    if (item[i].type > ItemID.None && item[i].stack > 0 && (item[i].type < ItemID.CopperCoin || item[i].type > ItemID.PlatinumCoin))
                    {
                        list2.Add(i);
                        list.Add(item[i].netID);
                    }
                    if (item[i].type == 0 || item[i].stack <= 0)
                    {
                        list3.Add(i);
                    }
                }
                int num = 50;
                int num2 = 10;
                if (player.chest <= -2)
                {
                    num += 4;
                }
                if (voidStack)
                {
                    num2 = 0;
                    num = 40;
                }
                for (int j = num2; j < num; j++)
                {
                    if (list.Contains(array[j].netID) && !array[j].favorited)
                    {
                        dictionary.Add(j, array[j].netID);
                    }
                }
                for (int k = 0; k < list2.Count; k++)
                {
                    int num3 = list2[k];
                    int netID = item[num3].netID;
                    foreach (KeyValuePair<int, int> item2 in dictionary)
                    {
                        if (item2.Value == netID && array[item2.Key].netID == netID)
                        {
                            int num4 = array[item2.Key].stack;
                            int num5 = item[num3].maxStack - item[num3].stack;
                            if (num5 == 0)
                            {
                                break;
                            }
                            if (num4 > num5)
                            {
                                num4 = num5;
                            }
                            if (!canVisualizeTransfers)
                                SoundEngine.PlaySound(SoundID.Grab);
                            ItemLoader.TryStackItems(item[num3], array[item2.Key], out num4);
                            if (canVisualizeTransfers && num4 > 0)
                            {
                                chest.OpenToReceiveParticles();
                                Chest.VisualizeChestTransfer(center, containerWorldPosition, item[num3], num4);
                            }
                            array2[num3] = true;
                        }
                    }
                }
                foreach (KeyValuePair<int, int> item3 in dictionary)
                {
                    if (array[item3.Key].stack == 0)
                    {
                        list4.Add(item3.Key);
                    }
                }
                foreach (int item4 in list4)
                {
                    dictionary.Remove(item4);
                }
                for (int l = 0; l < list3.Count; l++)
                {
                    int num6 = list3[l];
                    bool flag = true;
                    int num7 = item[num6].netID;
                    if (num7 >= 71 && num7 <= 74)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<int, int> item5 in dictionary)
                    {
                        if ((item5.Value != num7 || array[item5.Key].netID != num7) && (!flag || array[item5.Key].stack <= 0))
                        {
                            continue;
                        }
                        SoundEngine.PlaySound(SoundID.Grab);
                        if (flag)
                        {
                            num7 = item5.Value;
                            item[num6] = array[item5.Key];
                            array[item5.Key] = new Item();
                            if (canVisualizeTransfers)
                            {
                                chest.OpenToReceiveParticles();
                                Chest.VisualizeChestTransfer(center, containerWorldPosition, item[num6], item[num6].stack);
                            }
                        }
                        else
                        {
                            int num8 = array[item5.Key].stack;
                            int num9 = item[num6].maxStack - item[num6].stack;
                            if (num9 == 0)
                            {
                                break;
                            }
                            if (num8 > num9)
                            {
                                num8 = num9;
                            }
                            ItemLoader.TryStackItems(item[num6], array[item5.Key], out num8);
                            if (canVisualizeTransfers && num8 > 0)
                            {
                                chest.OpenToReceiveParticles();
                                Chest.VisualizeChestTransfer(center, containerWorldPosition, item[num6], num8);
                            }
                            if (array[item5.Key].stack == 0)
                            {
                                array[item5.Key] = new Item();
                            }
                        }
                        array2[num6] = true;
                        flag = false;
                    }
                }
                if (!Main.dedServ)
                {
                    for (int m = 0; m < array2.Length; m++)
                    {
                        NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, m));
                    }
                }
                list.Clear();
                list2.Clear();
                list3.Clear();
                dictionary.Clear();
                list4.Clear();

                return;
            }
            orig(context, voidStack);
        }

        private static void DepositAllITDChest(On_ChestUI.orig_DepositAll orig, Terraria.DataStructures.ContainerTransferContext context)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                Player player = Main.LocalPlayer;

                ITDChestTE chest = player.tileEntityAnchor.GetTileEntity() as ITDChestTE;

                ITDChestTE.MoveCoinsITD(player.inventory, chest.items, context);

                for (int num = 49; num >= 10; num--)
                {
                    if (player.inventory[num].stack > 0 && player.inventory[num].type > ItemID.None && !player.inventory[num].favorited)
                    {
                        if (player.inventory[num].maxStack > 1)
                        {
                            for (int i = 0; i < chest.TotalSlots; i++)
                            {
                                if (chest[i].stack >= chest[i].maxStack || !player.inventory[num].IsTheSameAs(chest[i]) || !ItemLoader.TryStackItems(chest[i], player.inventory[num], out _))
                                {
                                    continue;
                                }
                                SoundEngine.PlaySound(SoundID.Grab);
                                if (player.inventory[num].stack <= 0)
                                {
                                    player.inventory[num].SetDefaults();
                                    if (!Main.dedServ)
                                        NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, i));
                                    break;
                                }
                                if (chest[i].type == ItemID.None)
                                {
                                    chest[i] = player.inventory[num].Clone();
                                    player.inventory[num].SetDefaults();
                                }
                                if (!Main.dedServ)
                                    NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, i));
                            }
                        }
                        if (player.inventory[num].stack > 0)
                        {
                            for (int j = 0; j < chest.TotalSlots; j++)
                            {
                                if (chest[j].stack == 0)
                                {
                                    SoundEngine.PlaySound(SoundID.Grab);
                                    chest[j] = player.inventory[num].Clone();
                                    player.inventory[num].SetDefaults();
                                    if (!Main.dedServ)
                                        NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, j));
                                    break;
                                }
                            }
                        }
                    }
                }
                return;
            }
            orig(context);
        }

        private static void LootAllITDChest(On_ChestUI.orig_LootAll orig)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
            {
                Player player = Main.LocalPlayer;

                ITDChestTE chest = player.tileEntityAnchor.GetTileEntity() as ITDChestTE;
                GetItemSettings lootAllSettingsRegularChest = GetItemSettings.LootAllSettingsRegularChest;

                for (int i = 0; i < chest.TotalSlots; i++)
                {
                    if (chest[i].Exists())
                    {
                        chest[i].position = player.Center;
                        // debug full chest later
                        chest[i] = player.GetItem(Main.myPlayer, chest[i], lootAllSettingsRegularChest);
                        if (!Main.dedServ)
                            NetSystem.SendPacket(new SyncITDChestItemPacket(chest.ID, (byte)i));
                    }
                }

                return;
            }
            orig();
        }

        private static void ButtonValuesITDChestAdjust(On_ChestUI.orig_Draw orig, SpriteBatch spritebatch)
        {
            if (ITDChestTE.IsActiveForLocalPlayer)
                return;
            orig(spritebatch);
        }
    }
}
