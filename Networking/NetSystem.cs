using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using System.Runtime.CompilerServices;
using Terraria.Net;
using System.IO;

namespace ITD.Networking
{
    // many parts of this system are from terraria overhaul. credits to Mirsario!
    public class NetSystem : ModSystem 
    {
        public static NetSystem Instance => ModContent.GetInstance<NetSystem>();

        private static readonly List<ITDPacket> packets = new();

        private static readonly Dictionary<Type, ITDPacket> packetsByType = new();

        public override void Load()
        {
            // registers the packets into the packets list and the dictionary
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDPacket))))
            {
                var instance = (ITDPacket)RuntimeHelpers.GetUninitializedObject(type);
                instance.ID = packets.Count;
                packetsByType[type] = instance;

                packets.Add(instance);

                ContentInstance.Register(instance);
            }
        }
        public override void Unload()
        {
            packets?.Clear();
            packetsByType?.Clear();
        }
        public static ITDPacket GetPacket(byte id)
            => packets[id];

        public static ITDPacket GetPacket(Type type)
            => packetsByType[type];

        public static T GetPacket<T>() where T : ITDPacket
            => ModContent.GetInstance<T>();
        public static void SendPacket<T>(T packet, int ignoreClient = -1) where T : ITDPacket
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }

            ModPacket packetToSend = ITD.Instance.GetPacket();

            packetToSend.Write((byte)packet.ID);
            packet.WriteAndDispose(packetToSend);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                packetToSend.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                // Send to all clients except ignoreClient
                packetToSend.Send(ignoreClient: ignoreClient);
            }
        }
        internal static void HandlePacket(BinaryReader reader, int sender)
        {
            byte packetId = reader.ReadByte();

            if (packetId > packets.Count)
            {
                return;
            }

            var packet = packets[packetId];

            packet.Read(reader, sender);
        }
    }
}
