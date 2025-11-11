using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace ITD.Systems.Recruitment;

public struct RecruitData(byte whoAmI, ushort originalType, bool shimmered, NetworkText fullName) : TagSerializable
{
    public byte WhoAmI = whoAmI;
    public ushort OriginalType = originalType;
    public bool Shimmered = shimmered;
    public NetworkText FullName = fullName;
    // important: you can't just store the original type of a modded NPC as it may change after mods are reloaded.
    // so store the source mod, and serialize the name
    public Mod SourceMod = originalType > NPCID.Count ? NPCLoader.GetNPC(originalType).Mod : null;
    public bool INVALIDDATA = false;

    public static readonly Func<TagCompound, RecruitData> DESERIALIZER = Load;
    public readonly TagCompound SerializeData()
    {
        return new TagCompound
        {
            ["sourceMod"] = SourceMod is null ? string.Empty : $"{SourceMod.Name}:{NPCLoader.GetNPC(OriginalType).Name}",
            ["whoAmI"] = WhoAmI,
            ["originalType"] = OriginalType,
            ["shimmered"] = Shimmered,
            ["fullName"] = FullName,
        };
    }
    public static RecruitData Load(TagCompound tag)
    {
        // the first element of this will be the mod. second element is the internal name of the NPC
        string[] sourceModData = tag.GetString("sourceMod").Split(':');
        // explanation: if the "sourceMod" tag doesn't contain a valid mod, it will not have the : character, so the array will only be of Length one
        // the problem is that this doesnt check for if the mod exists before loading, but we handle just under.
        bool isVanilla = sourceModData.Length == 1;
        Mod mod = null;
        bool modActuallyExists = !isVanilla && ModLoader.TryGetMod(sourceModData[0], out mod);
        var data = new RecruitData
        {
            SourceMod = mod,
            OriginalType = isVanilla ? tag.Get<ushort>("originalType") : (!modActuallyExists || !mod.TryFind<ModNPC>(sourceModData[1], out var modNPC)) ? (ushort)0 : (ushort)modNPC.Type,
            WhoAmI = tag.GetByte("whoAmI"),
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
public class ExternalRecruitmentData(Action<NPC, Player> aiDelegate, Mod modInstance, string texturePath)
{
    public Action<NPC, Player> AIDelegate { get; set; } = aiDelegate;
    public Mod ModInstance { get; set; } = modInstance;
    public string TexturePath { get; set; } = texturePath;
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
        ITD.Instance.Logger.Info("Trying to add recruitment data for NPC of ID " + npcType + " from mod " + mod.Name);
        recruitmentDataRegistry[npcType] = new ExternalRecruitmentData(recruitmentAI, mod, texturePath);
    }
    public static ExternalRecruitmentData GetExternalRecruitmentData(int npcType)
    {
        return recruitmentDataRegistry.TryGetValue(npcType, out ExternalRecruitmentData data) ? data : null;
    }
    public static bool CanBeRecruited(int type) => NPCsThatCanBeRecruited.Contains(type) || recruitmentDataRegistry.ContainsKey(type);
    public static void QueueRecruit(NPC npc, Player player)
    {
        ITDSystem.recruitment.Enqueue(new QueuedRecruitment(npc.whoAmI, npc.type, player.ITD().guid));
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
        RecruitData data = ITDSystem.recruitmentData[player.ITD().guid];
        npc.Transform(data.OriginalType);
        npc.GivenName = data.FullName.ToString().Split(' ')[0];
        ITDSystem.recruitmentData.Remove(player.ITD().guid);
    }
}
