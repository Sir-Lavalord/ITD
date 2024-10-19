using ITD.Content.Items;
using ITD.Content.Items.Accessories.Misc;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class CopperMandiblesProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 44;
        int constantEffectTimer = 0;
        private int percentage;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(CopperMandiblesProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 10f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 22;
            MinDamage = 5;
            MaxDamage = 30;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/CopperMandiblesChain";
            ChompDust = DustID.Sand;
            DrawOffsetX = -16;
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.SandyBrown,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }

        public override void ConstantLatchEffect()
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                MiniMandible();
            }
        }
        private void MiniMandible()
        {
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                float speedX = -Projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-8f, 8f);
                float speedY = -Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + speedX, Projectile.position.Y + speedY, speedX, speedY, ModContent.ProjectileType<MiniMandible>(), Projectile.damage, 0f, Projectile.owner);
            }
        }
    }
}