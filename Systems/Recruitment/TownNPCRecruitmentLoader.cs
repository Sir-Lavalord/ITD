using ITD.Utilities;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.Localization;
using System.IO;
using Terraria.ModLoader.IO;

namespace ITD.Systems.Recruitment
{
    public struct RecruitData(int whoAmI, int originalType, bool shimmered, NetworkText fullName) : TagSerializable
    {
        public int WhoAmI = whoAmI;
        public int OriginalType = originalType;
        public bool Shimmered = shimmered;
        public NetworkText FullName = fullName;
        public bool INVALIDDATA = false;

        public static readonly Func<TagCompound, RecruitData> DESERIALIZER = Load;
        public readonly TagCompound SerializeData()
        {
            return new TagCompound
            {
                ["whoAmI"] = WhoAmI,
                ["originalType"] = OriginalType,
                ["shimmered"] = Shimmered,
                ["fullName"] = FullName,
            };
        }
        public static RecruitData Load(TagCompound tag)
        {
            var data = new RecruitData
            {
                WhoAmI = tag.GetInt("whoAmI"),
                OriginalType = tag.GetInt("originalType"),
                Shimmered = tag.GetBool("shimmered"),
                FullName = tag.Get<NetworkText>("fullName"),
            };
            return data;
        }
        public static RecruitData Invalid => new(0, 0, false, null) { INVALIDDATA = true };
        public override readonly string ToString()
        {
            return $"whoAmI: {WhoAmI}, originalType: {OriginalType}, shimmered: {Shimmered}, fullName: {FullName}, invalid: {INVALIDDATA}";
        }
    }
    public class NetTextSerializer : TagSerializer<NetworkText, TagCompound>
    {
        public override TagCompound Serialize(NetworkText value)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            value.Serialize(writer);
            return new TagCompound
            {
                ["data"] = memoryStream.ToArray()
            };
        }
        public override NetworkText Deserialize(TagCompound tag)
        {
            byte[] data = tag.Get<byte[]>("data");
            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);
            return NetworkText.Deserialize(reader);
        }
    }
    public class ExternalRecruitmentData (Action<NPC, Player> aiDelegate, Mod modInstance, string texturePath)
    {
        public Action<NPC, Player> AIDelegate { get; set; } = aiDelegate;
        public Mod ModInstance { get; set; } = modInstance;
        public string TexturePath {  get; set; } = texturePath;
    }
    public static class TownNPCRecruitmentLoader
    {
        public readonly static Dictionary<int, ExternalRecruitmentData> recruitmentDataRegistry = [];
        public readonly static int[] NPCsThatCanBeRecruited =
        [
            NPCID.Merchant
        ];
        public static void RegisterRecruitmentData(Mod mod, int npcType, Action<NPC, Player> recruitmentAI, string texturePath)
        {
            ITD.Instance.Logger.Info("Trying to add recruitment data for NPC of ID " +  npcType + " from mod " + mod.Name);
            recruitmentDataRegistry[npcType] = new ExternalRecruitmentData(recruitmentAI, mod, texturePath);
        }
        public static ExternalRecruitmentData GetExternalRecruitmentData(int npcType)
        {
            return recruitmentDataRegistry.TryGetValue(npcType, out ExternalRecruitmentData data) ? data : null;
        }
        public static bool CanBeRecruited(int type) => NPCsThatCanBeRecruited.Contains(type) || recruitmentDataRegistry.ContainsKey(type);
        public static void QueueRecruit(NPC npc, Player player)
        {
            ITDSystem.recruitment.Enqueue(new QueuedRecruitment(npc.whoAmI, npc.type, player.GetITDPlayer().guid));
        }
        public static void QueueUnrecruit(Guid player)
        {
            ITDSystem.unrecruitment.Enqueue(new QueuedUnrecruitment(player));
        }
        /// <summary>
        /// Bypasses unrecruit queue. Recruited NPCs call this if they can't find their recruiter for 2000 ticks (around 30 seconds)
        /// </summary>
        /// <param name="whoAmI"></param>
        /// <param name="player"></param>
        public static void ServerUnrecruit(int whoAmI, Player player = null)
        {
            NPC npc = Main.npc[whoAmI];
            if (player is null && npc.ModNPC is RecruitedNPC rNPC)
            {
                npc.Transform(rNPC.recruitmentData.OriginalType);
                return;
            }
            RecruitData data = ITDSystem.recruitmentData[player.GetITDPlayer().guid];
            npc.Transform(data.OriginalType);
            npc.GivenName = data.FullName.ToString().Split(' ')[0];
            ITDSystem.recruitmentData.Remove(player.GetITDPlayer().guid);
        }
    }
}
