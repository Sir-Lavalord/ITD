using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Projectiles;
using ITD.Content.Buffs;

namespace ITD.Content.Items
{
    public class CosmicJam : ModItem
    {
        // Names and descriptions of all ExamplePetX classes are defined using .hjson files in the Localization folder
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.

            Item.shoot = ModContent.ProjectileType<CosmicJamPet>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<CosmicJamBuff>(); // Apply buff upon usage of the Item.
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            return true;
        }
    }
}