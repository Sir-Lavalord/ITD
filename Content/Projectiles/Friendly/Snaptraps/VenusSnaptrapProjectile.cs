using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Snaptraps
{
      
    public class VenusSnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
   
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(VenusSnaptrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 10f;
            RetractAccel = 1.9f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 27;
            MinDamage = 1;
            MaxDamage = 25;
            FullPowerHitsAmount = 13;
            WarningFrames = 60;
            ChompDust = DustID.JungleSpore;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Snaptraps/VenusSnaptrapChain";
            //maybe there's a way to optimize sound swapping?
            // optimized it
            toSnaptrapMetal = "ITD/Content/Sounds/VenusClose";
            toSnaptrapForcedRetract = "ITD/Content/Sounds/VenusRetract";
            toSnaptrapChain = "ITD/Content/Sounds/VenusChain";
            toSnaptrapWarning = "ITD/Content/Sounds/VenusWarning";
            DrawOffsetX = -8;
            DrawOriginOffsetY = -16;
        }
        public override void OneTimeLatchEffect()
        {
            Main.npc[TargetWhoAmI].AddBuff(20, 80);
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new()
            {
                Text = OneTimeLatchMessage.Value,
                
                Color = Color.GreenYellow,
                DurationInFrames = 60,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }

    }
}