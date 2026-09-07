using ITD.Content.Buffs.MinionBuffs;
using ITD.Content.Projectiles.Friendly.Summoner;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Summoner
{
    public class IncendipedeStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new Terraria.DataStructures.DrawAnimationVertical(7,6));
        }
        public override void SetDefaults()
        {
            Item.damage = 35;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<IncendipedeMinionBuff>();
            Item.shoot = ModContent.ProjectileType<IncendipedeMinionBody>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            if (player.ownedProjectileCounts[ModContent.ProjectileType<IncendipedeMinionHead>()] == 0)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<IncendipedeMinionHead>(), damage, knockback, player.whoAmI);
            }
            var body = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            body.originalDamage = Item.damage;

            return false;
        }
    }
}