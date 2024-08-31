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

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
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
            ShootRange = 16f * 12f;
            RetractAccel = 1.8f;
            FramesUntilRetractable = 10;
            ExtraFlexibility = 16f * 4f;
            FramesBetweenHits = 16;
            MinDamage = 1280;
            MaxDamage = 3200;
            FullPowerHitsAmount = 10;
            WarningFrames = 80;
            ChompDust = DustID.Blood;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/VocalZeroChain";
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
                    MaxDamage = maxDamageStatic + ((int)damageToAdd * effectCount);
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
            Dust.NewDust(Projectile.Center, 6, 6, ChompDust, 0f, 0f, 0, default(Color), 1);
        }
    }
}