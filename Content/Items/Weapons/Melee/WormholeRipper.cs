using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Players;
using Terraria.DataStructures;
using Terraria.Audio;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Melee
{
    public class WormholeRipper : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.knockBack = 6f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 26;
            Item.shoot = ModContent.ProjectileType<WRipperSlash>();
			Item.shootSpeed = 4;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 1);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override bool AltFunctionUse(Player player)
        {
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            return modPlayer.itemVar[0] == 3;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            { //wormhole dash
				ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
				modPlayer.itemVar[0] = 0;
				modPlayer.dashTime = 16;
				modPlayer.dashVelocity = Vector2.Normalize(Main.MouseWorld - player.Center) * 16f;
				
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.shoot = ModContent.ProjectileType<WRipperDash>();
                SoundStyle wRipperRip = new SoundStyle("ITD/Content/Sounds/WRipperRip");
                SoundEngine.PlaySound(wRipperRip, player.Center);
				
                return true;
            }
            else
            { //regular swing
                Item.useStyle = ItemUseStyleID.Swing;
                Item.shoot = ModContent.ProjectileType<WRipperSlash>();
                return true;
            }
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (Item.shoot == ModContent.ProjectileType<WRipperDash>())
			{
				knockback *= 0.5f;
				Projectile.NewProjectileDirect(source, player.Center, new Vector2(), ModContent.ProjectileType<WRipperRift>(), damage, knockback * 0.5f, player.whoAmI);
			}
			else
				velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
            Projectile proj = Projectile.NewProjectileDirect(source, player.Center, velocity, Item.shoot, damage, knockback, player.whoAmI);
			proj.spriteDirection = player.direction;
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.MeteoriteBar, 3)
                .AddIngredient(ModContent.ItemType<StarlitBar>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}