using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Content.Buffs.PetBuffs;

namespace ITD.Content.Items.PetSummons
{
    public class CosmicJam : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish);

            Item.shoot = ModContent.ProjectileType<CosmicJamPet>();
            Item.buffType = ModContent.BuffType<CosmicJamBuff>();
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