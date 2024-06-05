using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Dusts;
using Terraria.Localization;

namespace ITD.Content.Projectiles
{
    public class VocalZeroProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 60;
        int constantEffectTimer = 0;
        public int maxDamageStatic { get; set; } = 3200; // This is specific to VocalZero
        float percentageToAdd = 10;
        int effectCount = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(VocalZeroProjectile)}.OneTimeLatchMessage"));
            shootRange = 16f * 12f;
            retractAccel = 1.8f;
            framesUntilRetractable = 10;
            extraFlexibility = 16f * 4f;
            framesBetweenHits = 16;
            minDamage = 1280;
            maxDamage = 3200;
            fullPowerHitsAmount = 10;
            warningFrames = 80;
            chompDust = DustID.Blood;
            toChainTexture = "ITD/Content/Projectiles/Friendly/VocalZeroChain";
            DrawOffsetX = -22;
            DrawOriginOffsetY = -22;
        }
        public override void ConstantLatchEffect()
        {
            constantEffectTimer += 1;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                if (effectCount < 10)
                {
                    effectCount += 1;
                    float damageToAdd = (float)maxDamageStatic / (100/percentageToAdd);
                    maxDamage = maxDamageStatic + ((int)damageToAdd * effectCount);
                    AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
                    {
                        //Text = "+10% damage!",
                        Text = OneTimeLatchMessage.WithFormatArgs(percentageToAdd).Value,
                        Color = Color.Red,
                        DurationInFrames = 60 * 2,
                        Velocity = Projectile.velocity,
                    };
                    PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
                }
            }
        }

        public override void PostAI()
        {
            Dust.NewDust(Projectile.Center, 6, 6, chompDust, 0f, 0f, 0, default(Color), 1);
        }
    }
}