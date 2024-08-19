using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Items;
using ITD.Content.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;
using ITD.Content;
using Microsoft.Xna.Framework;
using System.Drawing.Text;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Dunebarrel : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsDrill[Type] = false;
        }

        public override void SetDefaults()
        {

            Item.damage = 27;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 20;
            Item.height = 12;

            Item.useTime = 4;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.5f;
            Item.value = Item.buyPrice(gold: 12, silver: 60);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item23;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 65f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.channel = true;
            Item.autoReuse = true;

            Item.noMelee = false;
            Item.scale = 0.65f;

            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DunebarrelRightGun>(), damage, knockback, player.whoAmI);

            return false;
        }
        public void TrackPosition(Player player)
        {
            Vector2 position = player.itemLocation;
            DunebarrelRightGun.SetPosition(position);
        }

        public override void HoldItem(Player player)
        {
            TrackPosition(player);
        }

    }
}
