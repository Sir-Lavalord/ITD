using ITD.Content.Projectiles.Friendly.Mage;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Mage
{
    public class ThunderStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 300;
            Item.mana = 10;
            Item.width = 24;
            Item.height = 98;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item20;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
            Item.crit = 20;
            Item.staff[Type] = true;
        }
        //lazy af fix
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 vMousepos = Main.MouseWorld;
            Vector2 vSpawnpos = new Vector2(vMousepos.X, player.Center.Y - 600);
            //magic code please do not touch
            if (vMousepos.Y <= player.Center.Y - 600)
            {
                vMousepos.Y = player.Center.Y - 599;
            }

            Vector2 rotationVector2 =  vMousepos - vSpawnpos;
            rotationVector2.Normalize();

            float ai = Main.rand.Next(200,400);

                Projectile proj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(),vSpawnpos,new Vector2(0,20),
                  ModContent.ProjectileType<LightningStaffStrike>(), Item.damage, Item.knockBack, player.whoAmI,
                  rotationVector2.ToRotation(), ai);
            proj.tileCollide = false;
            proj.scale = 2f;
            proj.penetrate = 4;
            proj.timeLeft = 190;
            proj.CritChance = player.GetWeaponCrit(player.HeldItem);
            proj.DamageType = DamageClass.Magic;
                return false;
        }
    }
}
