using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ITD.Content.Items.Accessories.Movement
{
    public class DashingFeather : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FeatherDashPlayer>().DashAccessoryEquipped = true;
        }
    }
    public class FeatherDashPlayer : ModPlayer
    {
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;

        public const int DashCooldown = 50;
        public const int DashDuration = 35;

        public const float DashVelocity = 9f;

        public int DashDir = -1;

        public bool DashAccessoryEquipped;
        public int DashDelay = 0;
        public int DashTimer = 0;
        public Vector2 DashDirection = Vector2.Zero;

        public override void ResetEffects()
        {
            DashDirection = Vector2.Zero;
            DashAccessoryEquipped = false;
            bool leftDashing = Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15;
            bool rightDashing = Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15;
            bool upDashing = Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15;
            bool downDashing = Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15;
            bool Dashing = leftDashing || rightDashing || upDashing || downDashing;
            if (Dashing)
                DashDir = 1;
            else
            {
                DashDir = -1;
            }
        }
        public override void PreUpdateMovement()
        {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                DashDelay = DashCooldown;
                DashTimer = DashDuration;
            }

            if (DashDelay > 0)
                DashDelay--;
            if (DashTimer > 0)
            {
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                float x = 0;
                float y = 0;
                if (Player.controlLeft)
                {
                    x = -1;
                }
                else if (Player.controlRight)
                {
                    x = 1;
                }
                if (Player.controlUp)
                {
                    y = -1;
                }
                else if (Player.controlDown)
                {
                    y = 1;
                }
                DashDirection.X = x;
                DashDirection.Y = y;
                Player.velocity += DashDirection;
                Player.velocity.X = Math.Clamp(Player.velocity.X, -7f, 7f);
                Player.velocity.Y = Math.Clamp(Player.velocity.Y, -7f, 9f);
                DashTimer--;
            }
        }

        private bool CanUseDash()
        {
            //Main.NewText(Player.velocity.Y);
            return DashAccessoryEquipped
                && Player.dashType == DashID.None // player doesn't have Tabi or EoCShield equipped (give priority to those dashes)
                && !Player.setSolar // player isn't wearing solar armor
                && !Player.mount.Active // player isn't mounted
                && Player.IsOnStandableGround();
        }
    }
}