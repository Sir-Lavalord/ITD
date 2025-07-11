using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Summoner;

namespace ITD.Content.Items.Weapons.Summoner
{
	public class CicadianStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
		{
			Item.damage = 100;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 8;
			Item.width = 30;
			Item.height = 34;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<CicadianSentry>();
		}
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                player.FindSentryRestingSpot(type, out int XPosition, out int YPosition, out int YOffset);
                position = new Vector2((float)XPosition, (float)(YPosition - YOffset));
                int p = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = Item.damage;

                player.UpdateMaxTurrets();
            }
            return false;
        }
	}
}