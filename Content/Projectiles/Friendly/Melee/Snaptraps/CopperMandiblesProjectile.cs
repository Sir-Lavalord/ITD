using ITD.Content.Items;
using ITD.Content.Items.Accessories.Misc;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
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
        int constantEffectFrames = 22;
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
                ChangeDamage();
            }
        }
        private void ChangeDamage()
        {
            NPC target = Main.npc[TargetWhoAmI];

            if (target != null)
            {
                percentage = (target.life / target.lifeMax) * 100;
                target.life -= 30 + (30 * percentage);
            }
        }
    }
}