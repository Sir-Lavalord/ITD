﻿using ITD.Content.Items;
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

namespace ITD.Content.Projectiles
{
    public class SnaptrapProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapProperties()
        {
            shootRange = 16f * 16f;
            retractAccel = 1.5f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 22;
            minDamage = 20;
            maxDamage = 50;
            fullPowerHitsAmount = 10;
            warningFrames = 60;
            chompDust = DustID.Titanium;
        }
        public override void OneTimeLatchEffect()
        {
            Projectile.CritChance += 4;
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = "+4% crit chance!",
                Color = Color.White,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }
    }
}