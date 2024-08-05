using static Terraria.Mount;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace ITD.Content.Items.Accessories.Misc
{
    public class DriversIncense : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[ItemID.CosmicCarKey] = Type;
        }
        public override void SetDefaults()
        {
            Item.UseSound = SoundID.Item92;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.useAnimation = Item.useTime = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 34;
            Item.height = 32;
        }
        public override bool CanUseItem(Player player)
        {
            return !player.GetModPlayer<DriversIncensePlayer>().DriversIncenseConsumed;
        }
        public override bool? UseItem(Player player)
        {
            player.GetModPlayer<DriversIncensePlayer>().DriversIncenseConsumed = true;
            return true;
        }
    }
    public class DriversIncensePlayer : ModPlayer
    {
        public bool DriversIncenseConsumed = false;
        private void ResetMountSpeeds()
        {
            for (int i = 0; i < mounts.Length - 1; i++)
            {
                MountData mount = mounts[i];
                mount.runSpeed = ITD.defaultMountData[i].runSpeed;
                mount.dashSpeed = ITD.defaultMountData[i].dashSpeed;
                mount.swimSpeed = ITD.defaultMountData[i].swimSpeed;
                mount.jumpSpeed = ITD.defaultMountData[i].jumpSpeed;
            }
        }
        private void ApplyDriversIncenseBonus()
        {
            for (int i = 0; i < mounts.Length - 1; i++)
            {
                MountData mount = mounts[i];
                mount.runSpeed = ITD.defaultMountData[i].runSpeed * 1.1f;
                mount.dashSpeed = ITD.defaultMountData[i].dashSpeed * 1.1f;
                mount.swimSpeed = ITD.defaultMountData[i].swimSpeed * 1.1f;
                mount.jumpSpeed = ITD.defaultMountData[i].jumpSpeed * 1.1f;
            }
        }
        public override void PostUpdateEquips()
        {
            ResetMountSpeeds();
            if (DriversIncenseConsumed)
                ApplyDriversIncenseBonus();
        }
        public override void SaveData(TagCompound tag)
        {
            tag["consumedDriversIncense"] = DriversIncenseConsumed;
        }
        public override void LoadData(TagCompound tag)
        {
            DriversIncenseConsumed = tag.GetBool("consumedDriversIncense");
        }
    }
}
