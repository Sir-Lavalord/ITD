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
            //Why though?
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
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {

                Item.useTime = 1;
                Item.useAnimation = 30;
                Item.useAmmo = 0;
                Item.useTurn = false;
            }
            else
            {
                Item.useTurn = true;
                Item.useTime = 4;
                Item.useAnimation = 15;
                Item.useAmmo = AmmoID.Bullet;

            }
            return true;
        }
        int iDegree;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                velocity = velocity.RotatedBy(MathHelper.ToRadians(iDegree));
            }
        }
        public override void UseItemFrame(Player player)     //this defines what frame the player use when this weapon is used
        {
            if (player.altFunctionUse == 2)
            {
                player.bodyFrame.Y = 3 * player.bodyFrame.Height;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                iDegree += 12;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<DunebarrelRotate>()] < 1)
                {
                    player.GetModPlayer<DunebarrelStackPlayer>().iCurrentStack += 10;

                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DunebarrelRotate>(), damage, knockback, player.whoAmI);
                }
                return false;
            }
            else
            {
                iDegree = 0;
                var vPlayer = player.GetModPlayer<DunebarrelStackPlayer>();
                vPlayer.iCurrentStack--;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<DunebarrelRightGun>()] < 1)
                {
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DunebarrelRightGun>(), (int)(damage * vPlayer.fDamageIncrease), knockback, player.whoAmI);
                }
                return false;
            }
        }
        public void TrackPosition(Player player)
        {
            Vector2 position = player.itemLocation;
            DunebarrelRightGun.SetPosition(position);
            DunebarrelRotate.SetPosition(position);

        }

        public override void HoldItem(Player player)
        {
            TrackPosition(player);
        }

    }
    public class DunebarrelStackPlayer : ModPlayer
    {
        public float fDamageIncrease = 1f;
        public int iCurrentStack;
        public const int iMaxStack = 40;
        public override void UpdateDead()
        {
            iCurrentStack = 0;
        }
        //Actual yandereDev behaviour, i'm sorry
        public override void PostUpdate()
        {
            if (iCurrentStack >= iMaxStack)
            {
                iCurrentStack = iMaxStack;
            }
            if (iCurrentStack <= 0)
            {
                iCurrentStack = 0;
            }
            if (iCurrentStack > 30 && iCurrentStack <= 40)
            {
                fDamageIncrease = 2f;
            }
            else if (iCurrentStack > 20 && iCurrentStack <= 30)
            {
                fDamageIncrease = 1.75f;
            }
            else if (iCurrentStack > 10 && iCurrentStack <= 20)
            {
                fDamageIncrease = 1.5f;
            }
            else if (iCurrentStack > 0 && iCurrentStack <= 10)
            {
                fDamageIncrease = 1.25f;
            }
            else
            {
                fDamageIncrease = 1f;

            }
        }
    }
}
