using System;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class TheScorpionProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapDefaults()
        {
            ShootRange = 16f * 20f;
            RetractAccel = 6.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 30;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.GemAmber;
        }

        public override bool OneTimeLatchEffect()
        {
            AdvancedPopupRequest popupSettings = new()
            {
                Text = "GET OVER HERE!",
                Color = Color.Goldenrod,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            // pull em
            Vector2 towardsPlayer = Owner.Center - Projectile.Center;
            Vector2 normalized = Vector2.Normalize(towardsPlayer);
            float strength = (towardsPlayer.Length() / 10f)*(Main.npc[TargetWhoAmI].knockBackResist <= 0.1f? 0.1f : Main.npc[TargetWhoAmI].knockBackResist);
            Main.npc[TargetWhoAmI].velocity += new Vector2(0f, -2.5f)+(normalized*strength);
            retracting = true;
            return true;
        }

        public override void PostAI()
        {
            Projectile.spriteDirection = -Math.Sign((Owner.Center - Projectile.Center).X);
        }
    }
}