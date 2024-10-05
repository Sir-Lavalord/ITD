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
    public class CalciumProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 44;
        int constantEffectTimer = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(CalciumProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 22;
            MinDamage = 14;
            MaxDamage = 35;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/CalciumChain";
            ChompDust = DustID.Bone;
            DrawOffsetX = -16;
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.DarkGray,
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
                Heal();
            }
        }
        private void Heal()
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                player.Heal(2);
            }
        }
    }
}