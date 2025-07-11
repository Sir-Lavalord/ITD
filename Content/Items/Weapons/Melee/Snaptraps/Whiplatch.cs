using System;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Players;
using ITD.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Whiplatch : ITDSnaptrapItem
    {
        internal static Asset<Texture2D> InvSprite;
        public override string Texture => "ITD/Content/Items/Weapons/Melee/Snaptraps/Whiplatch_ArmA";
        //help
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

        }
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 40;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<WhiplatchProjectile>();
            Item.shootSpeed = 18f;
            Item.holdStyle = -1;
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraHookThrow"), player.Center);
            player.GetITDPlayer().recoilFront = 0.15f;
            //failsafe!
            int index = player.FindItemInInventoryOrOpenVoidBag(Type, out _);
            return base.Shoot(player,source,position,velocity,type,damage,knockback);
        }
        public override void HoldItem(Player player)
        {
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public void SetItemInHand(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer)
            {

                float animProgress = 1 - player.itemTime / (float)player.itemTimeMax;

                ITDPlayer modPlayer = player.GetITDPlayer();
                Vector2 mouse = modPlayer.MousePosition;

                if (mouse.X < player.Center.X)
                    player.direction = -1;
                else
                    player.direction = 1;
                Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -1f * player.gravDir);
                float itemRotation = (mouse - itemPosition).ToRotation();
                itemRotation = itemRotation * player.gravDir - modPlayer.recoilFront * player.direction;
                if (animProgress < 0.7f)
                    itemPosition -= itemRotation.ToRotationVector2() * (1 - (float)Math.Pow(1 - (0.7f - animProgress) / 0.7f, 4)) * 4f;

                if (animProgress < 0.4f)
                    itemRotation += -0.45f * (float)Math.Pow((0.4f - animProgress) / 0.4f, 2) * player.direction * player.gravDir;

                Vector2 itemSize = new Vector2(30, 20);
                Vector2 itemOrigin = new Vector2(-8, 0);
                PlayerHelpers.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
            }
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (InvSprite == null)
                InvSprite = ModContent.Request<Texture2D>("ITD/Content/Items/Weapons/Melee/Snaptraps/Whiplatch");

            Texture2D properSprite = InvSprite.Value;
            position.Y -= 10;
            position.X -= 4;

            spriteBatch.Draw(properSprite, position, null, drawColor, 0f, origin, scale, 0, 0);
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (InvSprite == null)
                InvSprite = ModContent.Request<Texture2D>("ITD/Content/Items/Weapons/Melee/Snaptraps/Whiplatch");

            Texture2D properSprite = InvSprite.Value;
            spriteBatch.Draw(properSprite, Item.position - Main.screenPosition + new Vector2(0, 22), null, lightColor, rotation, properSprite.Size() / 2f, scale, 0, 0);
            return false;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WhiplatchProjectile>()] > 0)
            {
                if (player.GetITDPlayer().recoilFront <= 0.4f)
                    player.GetITDPlayer().recoilFront += 0.1f;
            }
            SetItemInHand(player, heldItemFrame);
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
    }
    public partial class WhiplatchArmPlayer : ModPlayer
    {
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (Player is null)
                return;

            if (Player.HeldItem.ModItem is Whiplatch)
            {
                PlayerDrawLayers.ArmOverItem.Hide();
            }
        }
    }
}