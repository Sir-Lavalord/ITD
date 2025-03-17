using Terraria;
using Terraria.ModLoader;
using System.IO;
using System.Linq;

namespace ITD.Content.Items
{
    public class ITDTermporaryItem : GlobalItem
    {
		public static readonly int[] Applicable = {
			ItemID.Heart,
			ItemID.CandyApple,
			ItemID.CandyCane,
			ItemID.Star,
			ItemID.SoulCake,
			ItemID.SugarPlum,
		};
		
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(Item item, bool lateInstantiation)
		{
			return Applicable.Contains(item.type);
		}
		
		public bool temporary;
		public int lifespan = 2400;
		
		public override void PostUpdate(Item item)
		{
			if (temporary && item.timeSinceItemSpawned >= lifespan) {
				switch (item.type) {
					case ItemID.Heart:
					case ItemID.CandyApple:
					case ItemID.CandyCane:
						for (int i = 0; i < 10; i++)
						{
							int dust = Dust.NewDust(item.position, item.width, item.height, DustID.Blood,0, 0, 0, default, 1.5f);
							Main.dust[dust].noGravity = true;
						}
						break;
					case ItemID.Star:
					case ItemID.SoulCake:
					case ItemID.SugarPlum:
						for (int i = 0; i < 10; i++)
						{
							int dust = Dust.NewDust(item.position, item.width, item.height, DustID.ManaRegeneration ,0, 0, 0, default, 1.5f);
							Main.dust[dust].noGravity = true;
						}
						break;
				}
				item.active = false;
				item.type = 0;
				item.stack = 0;
				return;
			}
		}
		
		public override void NetSend(Item item, BinaryWriter writer)
        {
			writer.Write(item.timeSinceItemSpawned);
            writer.Write(temporary);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
			item.timeSinceItemSpawned = reader.ReadInt32();
            temporary = reader.ReadBoolean();
        }
    }
}
