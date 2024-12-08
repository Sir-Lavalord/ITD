using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using ITD.Content.Projectiles.Friendly.Mage;

namespace ITD.Content.Items.Weapons.Mage
{
    public class RoyalJellyScepter : ModItem
    {
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            Item.staff[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 10;
            Item.mana = 8;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.knockBack = 0.1f;
            Item.value = Item.buyPrice(silver: 10);
            Item.master = true;
            Item.UseSound = SoundID.NPCDeath1;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<RoyalJellyProj>(), damage, knockback, player.whoAmI);
            return false;
        }
    }
}
