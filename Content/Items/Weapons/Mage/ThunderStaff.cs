using ITD.Content.Projectiles.Friendly;
using ITD.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Mage
{
    public class ThunderStaff : ModItem
    {
        public override void SetDefaults()
        {
            //Still available for revert
            /*            Item.DefaultToMagicWeapon(ProjectileID.None, 30, 0f, true);
                        Item.damage = 300;
                        Item.staff[Type] = true;
                        Item.useTurn = true;
                        Item.shoot = ModContent.ProjectileType<LightningStaffStrike2>();*/
            Item.DamageType = DamageClass.Magic;
            Item.damage = 300;
            Item.mana = 10;
            Item.width = 24;
            Item.height = 98;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 5;
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
            Item.useTurn = true;
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*			Vector2 startPos = new(Main.MouseWorld.X + Main.rand.NextFloat(-256f, 256f), Main.screenPosition.Y);
                        Vector2 theoreticalEndPos = Helpers.QuickRaycast(new Vector2 (Main.MouseWorld.X, Main.screenPosition.Y), Vector2.UnitY);
                        Vector2 endPos = Helpers.QuickRaycast(startPos, (theoreticalEndPos - startPos).SafeNormalize(Vector2.Zero), true);
                        MiscHelpers.CreateLightningEffects(startPos, endPos);
                        Projectile.NewProjectile(source, endPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
                        SoundEngine.PlaySound(SoundID.Item89, endPos);
                        Collision.HitTiles(endPos - new Vector2(32, 0), Vector2.UnitY, 64, 16);
                        for (int i = 0; i < 5; i++)
                        {
                            Gore.NewGore(Item.GetSource_FromThis(), endPos, new Vector2(Main.rand.NextFloat(-2, 2), -1), Main.rand.Next(61, 64));
                        }*/
            Vector2 vSpawnpos = new Vector2(Main.MouseWorld.X,player.Center.Y - 500);
            Vector2 vMousepos = Main.MouseWorld;
            
            Vector2 rotationVector2 =  vMousepos - vSpawnpos;
            rotationVector2.Normalize();

            float ai = Main.rand.Next(200,400);

                int projID = Projectile.NewProjectile(Item.GetSource_FromThis(),vSpawnpos,new Vector2(0,20),
                  ModContent.ProjectileType<Projectiles.Friendly.LightningStaffStrike2>(), Item.damage, Item.knockBack, player.whoAmI,
                  rotationVector2.ToRotation(), ai);
            Main.projectile[projID].tileCollide = false;
            Main.projectile[projID].scale = 2f;
            Main.projectile[projID].penetrate = 4;
            Main.projectile[projID].timeLeft = 190;
            Main.projectile[projID].CritChance = player.GetWeaponCrit(player.HeldItem);
            Main.projectile[projID].DamageType = DamageClass.Magic;
                return false;
        }
    }
}
