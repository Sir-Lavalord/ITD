using ITD.Content.Items;
using ITD.Content.Items.Accessories.Misc;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Physics;
using ITD.Utilities;
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
    public class ChainwhipSnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 4;
        int constantEffectTimer = 0;
        float constantEffect = 0f;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(ChainwhipSnaptrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 18f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 22;
            MinDamage = 22;
            MaxDamage = 66;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/ChainwhipChain";
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
                Color = Color.Silver,
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
                Buff();
            }
        }
        private void Buff()
        {
            Player player = Main.player[Projectile.owner];
            constantEffect += 0.01f;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += constantEffect;
            player.GetDamage(DamageClass.Summon) += constantEffect;
            player.moveSpeed += constantEffect;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            ITDSnaptrap snaptrap = player.GetSnaptrapPlayer().GetActiveSnaptrap();
            if (snaptrap.retracting == false)
            {
                player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) -= constantEffect;
                player.GetDamage(DamageClass.Summon) -= constantEffect;
                player.moveSpeed -= constantEffect;
                constantEffect = 0f;
            }

            return true;
        }
    }
}