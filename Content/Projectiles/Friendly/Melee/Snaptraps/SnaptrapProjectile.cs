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
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class SnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int addCritChance = 4;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(SnaptrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 22;
            MinDamage = 20;
            MaxDamage = 50;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.Titanium;
        }
        public override void OneTimeLatchEffect()
        {
            Projectile.CritChance += addCritChance;
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.WithFormatArgs(addCritChance).Value,
                //Text = "+4% crit chance!",
                Color = Color.White,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }
    }
}