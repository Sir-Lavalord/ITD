using ITD.Content.Buffs.Debuffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class FrostgripProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 1;
        int constantEffectTimer = 0;
        int totalEffectTime = 0;

        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(FrostgripProjectile)}.OneTimeLatchMessage"));
            ShootRange = 18f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 22;
            MinDamage = 25;
            MaxDamage = 55;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/FrostgripChain";
            ChompDust = DustID.Ice;
            DrawOffsetX = -18;
        }
        public override void OneTimeLatchEffect()
        {
            totalEffectTime = 0;
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.LightBlue,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            SoundStyle freeze = new SoundStyle("ITD/Content/Sounds/FrostgripFreeze");
            SoundEngine.PlaySound(freeze, Projectile.Center);
        }

        public override void ConstantLatchEffect()
        {
            NPC target = Main.npc[TargetWhoAmI];
            totalEffectTime += 2;
            constantEffectTimer += 1;

            if (totalEffectTime >= target.width * target.height)
            {
                target.AddBuff(ModContent.BuffType<FrostgripChilledBuff>(), 20);
                retracting = true;
            } 
            else
            {
                if (constantEffectTimer >= constantEffectFrames)
                {
                    constantEffectTimer = 0;
                    Chill();
                }
            }
        }
        private void Chill()
        {
            NPC target = Main.npc[TargetWhoAmI];
            target.AddBuff(ModContent.BuffType<FrostgripChilledBuff>(), 2);
        }
    }
}