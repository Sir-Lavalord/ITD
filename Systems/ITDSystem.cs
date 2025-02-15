using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Tiles.DeepDesert;
using System;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;
using ITD.Systems.Recruitment;
using System.Collections.Generic;
using ITD.Utilities;
using System.Reflection;
using Terraria.Localization;
using Terraria.Chat;
using ITD.Networking;
using ITD.Networking.Packets;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Items.DevTools;
using ReLogic.Content;
using Terraria.UI;
using ITD.Content.Tiles;

namespace ITD.Systems
{
    public struct QueuedRecruitment(int npc, int npcType, Guid player)
    {
        public int NPC = npc;
        public int NPCType = npcType;
        public Guid player = player;
    }
    public struct QueuedUnrecruitment(Guid plr)
    {
        public Guid player = plr;
    }
    public class ITDSystem : ModSystem
    {
        public static readonly Queue<QueuedRecruitment> recruitment = [];
        public static readonly Queue<QueuedUnrecruitment> unrecruitment = [];
        public static readonly Dictionary<Guid, RecruitData> recruitmentData = [];
        internal static bool _hasMeteorFallen;
        public static bool hasMeteorFallen
        {
            get => _hasMeteorFallen;
            set
            {
                if (!value)
                    _hasMeteorFallen = false;
                else
                    NPC.SetEventFlagCleared(ref _hasMeteorFallen, -1);
            }
        }
        public int bluegrassCount;
        public int deepdesertTileCount;
        public override void Load()
        {
            On_Main.UpdateTime_SpawnTownNPCs += PreventTownNPCSpawns;
        }
        public override void SetStaticDefaults()
        {
            NaturalSpawns.SetStaticDefaults();
        }
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            bluegrassCount = tileCounts[ModContent.TileType<Bluegrass>()];
            deepdesertTileCount = tileCounts[ModContent.TileType<DioriteTile>()] + tileCounts[ModContent.TileType<PegmatiteTile>()];
        }
        public override void SaveWorldData(TagCompound tag)
        {
            // avoid bloating world size by saving useless values
            if (hasMeteorFallen)
            {
                tag["hasMeteorFallen"] = true;
            }
            // we only register a new recruitmentdata if recruitment is successful anyway, so we don't have to account for the same thing as we do above
            foreach (var pair in recruitmentData)
            {
                tag[$"recdata:{pair.Key}"] = pair.Value;
            }
            recruitmentData.Clear();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            hasMeteorFallen = tag.ContainsKey("hasMeteorFallen");
            recruitmentData.Clear();
            // i wonder why this dictionary is private

            var dictionary =
                ReflectionHelpers.Get<FieldInfo, Dictionary<string, object>>("dict", tag, flags: BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var key in dictionary.Keys)
            {
                if (key.StartsWith("recdata:"))
                {
                    string guidString = key["recdata:".Length..];
                    if (Guid.TryParse(guidString, out Guid guid))
                    {
                        RecruitData rD = recruitmentData[guid] = tag.Get<RecruitData>(key);
                    }
                }
            }

            if (Main.dedServ)
                NetSystem.SendPacket(new SyncRecruitmentPacket());
        }
        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = hasMeteorFallen;
            writer.Write(flags);
        }
        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            hasMeteorFallen = flags[0];
        }
        public override void PostUpdateDusts()
        {
            BlueshroomTree.opac = ((float)Math.Sin(Main.GameUpdateCount / 40f) + 1f) / 2f;
        }
        public static void Log(object message, Color? color = null)
        {
            color ??= Color.White;
            if (Main.dedServ)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message.ToString()), color.Value);
            else
                Main.NewText(message, color.Value);
        }
        public static void LogForPlayer(object message, int player, Color? color = null)
        {
            color ??= Color.White;
            if (Main.dedServ)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(message.ToString()), color.Value, player);
            else
                Main.NewText(message, color.Value);
        }
        public override void PreUpdateWorld()
        {
            // PreUpdateWorld is perfect for queueing recruitments bc it doesn't run on clients (no networking problems, we can safely send our own packet to all clients)
            //Log(recruitment.Count.ToString());
            if (recruitment.Count > 0)
            {
                QueuedRecruitment q = recruitment.Dequeue();
                // let's do some error handling! We need to check two things here:
                // 1 - Make sure there's a player whose Guid actually matches the Guid from QueuedRecruitment
                // 2 - Make sure the NPC's type from Main.npc[q.NPC].type matches q.NPCType, and that that NPC actually exists in the world.
                // No need to use Queue.Peek here, because we would just throw that QueuedRecruitment instance away anyway if it doesn't match our rules.
                NPC npc = Main.npc[q.NPC];
                Player player = PlayerHelpers.FromGuid(q.player);
                bool playerCheck = player != null;
                bool npcCheck = npc.type == q.NPCType && npc.Exists();
                if (playerCheck && npcCheck)
                {
                    if (TownNPCRecruitmentLoader.CanBeRecruited(npc.type))
                    {
                        //if (Main.ShopHelper.GetShoppingSettings(player, npc).PriceAdjustment < 0.82f) // if happiness is max
                        if (true is true || true is !false && true) // just for testing yaehahh
                        {
                            recruitmentData[q.player] = new RecruitData((byte)npc.whoAmI, (ushort)npc.type, npc.IsShimmerVariant, npc.GetFullNetName());

                            npc.Transform(ModContent.NPCType<RecruitedNPC>());

                            if (npc.ModNPC is RecruitedNPC rNpc)
                            {
                                rNpc.Recruiter = player.GetITDPlayer().guid;
                                rNpc.recruitmentData = recruitmentData[q.player];
                                if (Main.dedServ)
                                    NetSystem.SendPacket(new SingleNPCRecruitmentPacket((byte)npc.whoAmI, rNpc.Recruiter, rNpc.recruitmentData));

                            }

                            if (Main.dedServ)
                                NetSystem.SendPacket(new SyncRecruitmentPacket());

                            LogForPlayer(ITD.Instance.GetLocalization("RecruitmentSystem.RecruitmentAccepted").Format(npc.FullName), player.whoAmI, Color.LimeGreen);
                        }
                        else
                        {
                            LogForPlayer(ITD.Instance.GetLocalization("RecruitmentSystem.NotHappyEnough").Format(npc.FullName), player.whoAmI, Color.Red);
                        }
                    }
                    else
                    {
                        LogForPlayer(ITD.Instance.GetLocalization("RecruitmentSystem.RecruitmentDenied").Format(npc.FullName), player.whoAmI, Color.Red);
                    }
                }
            }
            if (unrecruitment.Count > 0)
            {
                QueuedUnrecruitment q = unrecruitment.Dequeue();
                RecruitData rD = recruitmentData[q.player];
                NPC npc = Main.npc[rD.WhoAmI];

                npc.Transform(rD.OriginalType);
                npc.GivenName = rD.FullName.ToString().Split(' ')[0];

                if (recruitmentData.Remove(q.player))
                {
                    if (Main.dedServ)
                        NetSystem.SendPacket(new SyncRecruitmentPacket());
                }
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int invIndex = layers.FindIndex(l => l.Name == "Vanilla: Inventory");
            if (invIndex != 1)
            {
                layers.Insert(invIndex, new LegacyGameInterfaceLayer("ITD: Custom Chests", delegate
                {
                    ITDChest.DrawCustomChests();
                    return true;
                }, InterfaceScaleType.UI));
            }
        }
        private static void PreventTownNPCSpawns(On_Main.orig_UpdateTime_SpawnTownNPCs orig)
        {
            orig();
            PreventRecruitedNPCSpawn();
        }
        private static void PreventRecruitedNPCSpawn()
        {
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.ModNPC is not RecruitedNPC rNPC)
                    continue;
                int originalType = rNPC.recruitmentData.OriginalType;
                if (originalType > -1)
                    Main.townNPCCanSpawn[originalType] = false;
            }
        }
    }
}
