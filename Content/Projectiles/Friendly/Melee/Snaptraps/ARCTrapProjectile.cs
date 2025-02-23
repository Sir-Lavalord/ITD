using ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class ARCTrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 60;
        int constantEffectTimer = 0;
        private int percentage;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(ARCTrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 10f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 5;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/ARCTrapChain";
            ChompDust = DustID.Sand;
            DrawOffsetX = -16;
        }
        public override bool OneTimeLatchEffect()
        {
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.SandyBrown,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            return true;
        }

        public override void ConstantLatchEffect()
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                Electrocute();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }
            float num1 = 0f;
            if (hasDoneLatchEffect)
            {
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.Center, Main.player[Projectile.owner].Center,
                    22f * Projectile.scale, ref num1))
                {

                    return true;
                }
            }
            return false;
        }
        private void Electrocute()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 magVec = Projectile.Center - player.MountedCenter;
                magVec.Along(Owner.MountedCenter, 10, v =>
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        Dust dust = Dust.NewDustDirect(new Vector2(v.X, v.Y), 0, 0, DustID.Electric);
                        dust.noGravity = true;
                    }
                });
            }
        }
    }
}