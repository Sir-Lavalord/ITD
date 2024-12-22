using System.IO;
using Terraria.ID;
using Terraria;
using ITD.Systems.Recruitment;
using Terraria.Localization;
using System;

namespace ITD.Networking
{
    public static class NetExtensions // from overhaul. credits to Mirsario
    {
        public static void WriteGuid(this BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }
        public static Guid ReadGuid(this BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }
        public static void WriteRecruitmentData(this BinaryWriter writer, RecruitData recruitData)
        {
            if (Main.dedServ)
            {
                writer.Write(recruitData.WhoAmI);
                writer.Write(recruitData.OriginalType);
                writer.Write(recruitData.Shimmered);
                writer.WriteNetworkText(recruitData.FullName);
            }
        }
        public static RecruitData ReadRecruitmentData(this BinaryReader reader)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return new RecruitData
                {
                    WhoAmI = reader.ReadInt32(),
                    OriginalType = reader.ReadInt32(),
                    Shimmered = reader.ReadBoolean(),
                    FullName = reader.ReadNetworkText()
                };
            }
            return RecruitData.Invalid;
        }
        public static void WriteNetworkText(this BinaryWriter writer, NetworkText text)
        {
            text.Serialize(writer);
        }
        public static NetworkText ReadNetworkText(this BinaryReader reader)
        {
            return NetworkText.Deserialize(reader);
        }
        public static void TryWriteSenderPlayer(this BinaryWriter writer, Player player)
        {
            if (Main.dedServ)
            {
                writer.Write((byte)player.whoAmI);
            }
        }
        public static bool TryReadSenderPlayer(this BinaryReader reader, int sender, out Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                sender = reader.ReadByte();
            }

            player = Main.player[sender];

            return player != null && player.active;
        }
    }
}
