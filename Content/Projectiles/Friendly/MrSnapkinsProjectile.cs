using ITD.Content.Items;
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
using Terraria.Map;
using Terraria.ModLoader;
using ITD.Content.Projectiles;
using Terraria.Localization;

namespace ITD.Content.Projectiles
{
    public class MrSnapkinsProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }

        int constantEffectFrames = 100;
        int constantEffectTimer = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(MrSnapkinsProjectile)}.OneTimeLatchMessage"));
            shootRange = 16f * 16f;
            retractAccel = 1.5f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 22;
            minDamage = 20;
            maxDamage = 50;
            fullPowerHitsAmount = 5;
            warningFrames = 60;
            chompDust = DustID.Titanium;
        }

        private void LaunchBowties()
        {
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 2f, (float)Math.Sin(MathHelper.PiOver4 * i) * 2f), ModContent.ProjectileType<SnapkinsBowtie>(), minDamage, 0.1f);
                }
            }
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                Color = Color.DarkSlateGray,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            LaunchBowties();
        }

        public override void ConstantLatchEffect()
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                LaunchBowties();
            }
        }

        public override void PostAI()
        {
            Projectile.spriteDirection = -Math.Sign((myPlayer.Center - Projectile.Center).X);
        }
    }
}