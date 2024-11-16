using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Systems;
using ITD.Players;
using ITD.Utilities;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Ghostbuster : ModItem
    {
		public override void SetStaticDefaults()
        {
            FrontGunLayer.RegisterData(Item.type);
        }
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 68;
            Item.height = 24;
            Item.useTime = 10;
            Item.useAnimation = 10;
			Item.useStyle = -1;
            Item.noMelee = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<GhostbusterProj>();
            Item.autoReuse = false;
			Item.channel = true;
			Item.noUseGraphic = true;
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
		
		public override Vector2? HoldoutOffset() {
			return new Vector2(12f, -6f);
		}
	}
}
