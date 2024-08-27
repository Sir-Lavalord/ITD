using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

using ITD.Systems;
using ITD.Players;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Text;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Dunebarrel : ModItem
    {
		public int attackCycle = 0;
		
		public override void SetStaticDefaults()
        {
            FrontGunLayer.RegisterData(Item.type);
			BackGunLayer.RegisterData(Item.type);
        }
        public override void SetDefaults()
        {

            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 20;
            Item.height = 12;

            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = -1;//ItemUseStyleID.Shoot;
            Item.knockBack = 0.5f;
            Item.value = Item.buyPrice(gold: 12, silver: 60);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item40;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;

            Item.scale = 0.75f;

            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
			attackCycle = ++attackCycle % 2;
			if (attackCycle == 1)
			{
				modPlayer.recoilFront = 0.2f;
			}
			else
			{
				modPlayer.recoilBack = 0.2f;
			}
            return true;
        }

		public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
			if (Main.MouseWorld.X < player.Center.X)
				player.direction = -1;
			else
				player.direction = 1;
			
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
			
			float rotation = (Vector2.Normalize(Main.MouseWorld - player.MountedCenter)*player.direction).ToRotation();
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
			player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
        }

		public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
			if (Main.MouseWorld.X < player.Center.X)
				player.direction = -1;
			else
				player.direction = 1;
			
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
			
			float rotation = (Vector2.Normalize(Main.MouseWorld - player.MountedCenter)*player.direction).ToRotation();
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
			player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
        }
    }
}
