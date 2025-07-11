using Terraria.DataStructures;

using ITD.Systems;
using ITD.Players;
using ITD.Utilities;
using ITD.Content.Projectiles;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Fwoomstick : ModItem
    {
        public override void SetStaticDefaults()
        {
            FrontGunLayer.RegisterData(Item.type);
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 100;
            Item.height = 20;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.useStyle = -1;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 5f;
            Item.useAmmo = AmmoID.Bullet;
			Item.shoot = 1;
            Item.autoReuse = false;
            Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item36;

            Item.scale = 0.75f;
        }

        public void Hold(Player player)
		{
			ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
				player.direction = -1;
			else
				player.direction = 1;
			
			float rotation = (Vector2.Normalize(mouse - player.MountedCenter)*player.direction).ToRotation();
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
			player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
		}

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(12f, -10f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 4; i++) {
				Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(20));
				newVelocity *= 1f - Main.rand.NextFloat(0.3f);
				Projectile proj = Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
                proj.GetGlobalProjectile<ITDInstancedGlobalProjectile>().ProjectileSource = ITDInstancedGlobalProjectile.ProjectileItemSource.Fwoomstick;
			}
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
			modPlayer.recoilFront = 0.2f;
			modPlayer.recoilBack = 0.2f;
            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = velocity.SafeNormalize(Vector2.Zero) * 60f;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }
    }
}