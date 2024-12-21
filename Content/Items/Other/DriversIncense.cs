using static Terraria.Mount;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Terraria.Localization;

namespace ITD.Content.Items.Other
{
    public class DriversIncense : ModItem
    {
        public static LocalizedText MunchiesExplanation { get; private set; }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[ItemID.CosmicCarKey] = Type;
            MunchiesExplanation = this.GetLocalization("MunchiesExplanation");
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
        private bool statsModified = false;
        private int latestMount = -1;
        public override void PostUpdateEquips()
        {
            if (Player.mount.Active)
            {
                latestMount = Player.mount._type;
                if (DriversIncenseConsumed)
                {
                    var mount = mounts[latestMount];
                    mount.runSpeed *= 1.1f;
                    mount.dashSpeed *= 1.1f;
                    mount.swimSpeed *= 1.1f;
                    mount.jumpSpeed *= 1.1f;
                    statsModified = true;
                }
            }
            /*
            if (Player.mount.Active && latestMount > -1)
            {
                Main.NewText(mounts[latestMount].runSpeed);
            }
            */
        }
        public override void ResetEffects()
        {
            if (statsModified)
            {
                var mount = mounts[latestMount];
                mount.runSpeed /= 1.1f;
                mount.dashSpeed /= 1.1f;
                mount.swimSpeed /= 1.1f;
                mount.jumpSpeed /= 1.1f;

                statsModified = false;
            }
            /*
            if (latestMount > -1)
            {
                Main.NewText(mounts[latestMount].runSpeed);
            }
            */
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
