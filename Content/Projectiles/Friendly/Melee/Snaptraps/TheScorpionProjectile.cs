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

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class TheScorpionProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapProperties()
        {
            ShootRange = 16f * 20f;
            RetractAccel = 6.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 20;
            MinDamage = 30;
            MaxDamage = 75;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.GemAmber;
        }

        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new()
            {
                Text = "GET OVER HERE!",
                Color = Color.Goldenrod,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            // pull em
            Vector2 towardsPlayer = myPlayer.Center - Projectile.Center;
            Vector2 normalized = Vector2.Normalize(towardsPlayer);
            float strength = (towardsPlayer.Length() / 10f)*(Main.npc[TargetWhoAmI].knockBackResist <= 0.1f? 0.1f : Main.npc[TargetWhoAmI].knockBackResist);
            Main.npc[TargetWhoAmI].velocity += new Vector2(0f, -2.5f)+(normalized*strength);
            retracting = true;
        }

        public override void PostAI()
        {
            Projectile.spriteDirection = -Math.Sign((myPlayer.Center - Projectile.Center).X);
        }
    }
}