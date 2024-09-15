using System;
using System.Linq;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Summoner
{
    public class AsterKnuckle : ModItem
    {
        internal static Asset<Texture2D> InvSprite;
        public override string Texture => "ITD/Content/Items/Weapons/Summoner/AsterKnuckle_Arm";

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 500;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 5;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AsterBlasterBlast>();
            Item.shootSpeed = 18f;
            Item.holdStyle = -1;
            Item.master = true;
        }

        public override void HoldItem(Player player)
        {
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraExplode"), player.Center);
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraPunch"), player.Center);
            ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            modPlayer.recoilFront = 0.1f;
            modPlayer.Screenshake = 10;
            Projectile Blast = Projectile.NewProjectileDirect(player.GetSource_FromThis(), Item.Center, Vector2.Zero,
    ModContent.ProjectileType<AsterBlasterBlast>(), (int)(Item.damage), Item.knockBack, player.whoAmI);
            Blast.ai[1] = 100f; // Randomize the maximum radius.
            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // And the interpolation step.
            Blast.netUpdate = true;
            return false;
        }


        public void SetItemInHand(Player player, Rectangle heldItemFrame)
        {
            float animProgress = 1 - player.itemTime / (float)player.itemTimeMax;

            ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
                player.direction = -1;
            else
                player.direction = 1;

            //Default
            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -1f * player.gravDir);
            float itemRotation = (mouse - itemPosition).ToRotation();
            itemRotation = itemRotation * player.gravDir - modPlayer.recoilFront * player.direction;

            //Adjust for animation

            if (animProgress < 0.7f)
                itemPosition -= itemRotation.ToRotationVector2() * (1 - (float)Math.Pow(1 - (0.7f - animProgress) / 0.7f, 4)) * 4f;

            if (animProgress < 0.4f)
                itemRotation += -0.45f * (float)Math.Pow((0.4f - animProgress) / 0.4f, 2) * player.direction * player.gravDir;

            Vector2 itemSize = new Vector2(30, 20);
            Vector2 itemOrigin = new Vector2(-8, 0);
            PlayerHelpers.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);

        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (InvSprite == null)
                InvSprite = ModContent.Request<Texture2D>("ITD/Content/Items/Weapons/Summoner/AsterKnuckle");

            Texture2D properSprite = InvSprite.Value;
            spriteBatch.Draw(properSprite, position, null, drawColor, 0f, origin, scale, 0, 0);
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (InvSprite == null)
                InvSprite = ModContent.Request<Texture2D>("ITD/Content/Items/Weapons/Summoner/AsterKnuckle");

            Texture2D properSprite = InvSprite.Value;
            spriteBatch.Draw(properSprite, Item.position, null, lightColor, rotation, properSprite.Size(), scale, 0, 0);
            return false;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
    }
    public partial class AsterKnuckleArmPlayer : ModPlayer
    {
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (Player is null)
                return;

            if (Player.HeldItem.ModItem is AsterKnuckle)
            {
                PlayerDrawLayers.ArmOverItem.Hide();
            }
        }
    }
}