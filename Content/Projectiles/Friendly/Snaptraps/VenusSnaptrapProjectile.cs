using ITD.Content.Items;
using ITD.Physics;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
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

namespace ITD.Content.Projectiles.Friendly.Snaptraps
{
      
    public class VenusSnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
   
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(VenusSnaptrapProjectile)}.OneTimeLatchMessage"));
            shootRange = 16f * 10f;
            retractAccel = 1.9f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 27;
            minDamage = 1;
            maxDamage = 25;
            fullPowerHitsAmount = 13;
            warningFrames = 60;
            chompDust = DustID.JungleSpore;
          toChainTexture = "ITD/Content/Projectiles/Friendly/Snaptraps/VenusSnaptrapChain";
          //maybe there's a way to optimize sound swapping?
           snaptrapMetal = new SoundStyle("ITD/Content/Sounds/VenusClose", SoundType.Sound);
           snaptrapForcedRetract = new SoundStyle("ITD/Content/Sounds/VenusRetract", SoundType.Sound);
           snaptrapChain = new SoundStyle("ITD/Content/Sounds/VenusChain", SoundType.Sound);
           snaptrapWarning = new SoundStyle("ITD/Content/Sounds/VenusWarning", SoundType.Sound);
        }
        public override void OneTimeLatchEffect()
        {
      Main.npc[TargetWhoAmI].AddBuff(20, 80);
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
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