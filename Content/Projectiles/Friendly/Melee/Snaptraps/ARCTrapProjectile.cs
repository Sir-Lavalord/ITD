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
        int constantEffectFrames = 30;
        int constantEffectTimer = 0;
        private int percentage;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(ARCTrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 20f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 22;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/ARCTrapChain_Anim";
            //You've gotta make better sprite, senator! you've gotta make it, not me, especially not me
            ChompDust = DustID.Electric;
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
            return base.Colliding(projHitbox, targetHitbox);
        }
        private void Electrocute()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 magVec = Projectile.Center - player.MountedCenter;
                magVec.Along(Owner.MountedCenter, 10, v =>
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        Dust dust = Dust.NewDustDirect(new Vector2(v.X, v.Y), 0, 0, DustID.Electric);
                        dust.noGravity = true;
                    }
                });
                for (int i = 0; i <= 2; i++)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, DustID.Electric);//horrifying...
                    dust.noGravity = true;
                }
            }
        }
        private int frameCounter = 0;
        private readonly int frameTimer = 5;
        private int currentFrame = 0;
        private readonly int frameCount = 4;

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ToChainTexture);
            if (hasDoneLatchEffect)
            {
                if (frameCounter++ >= frameTimer)
                {
                    frameCounter = 0;
                    currentFrame = (currentFrame + 1) % frameCount;
                }
            }
            Rectangle chainSourceRectangle = chainTexture.Frame(verticalFrames: frameCount, frameY: currentFrame % frameCount);
            float chainHeightAdjustment = 0f;

            Vector2 chainOrigin = chainSourceRectangle.Size() / 2f;
            Vector2 chainDrawPosition = Projectile.Center;
            Vector2 vectorFromProjectileToPlayer = player.Center.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
            Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayer.SafeNormalize(Vector2.Zero);
            float chainSegmentLength = chainSourceRectangle.Height + chainHeightAdjustment;
            if (chainSegmentLength == 0)
            {
                chainSegmentLength = 10;
            }
            float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
            int chainCount = 0;
            float chainLengthRemainingToDraw = vectorFromProjectileToPlayer.Length() + chainSegmentLength / 2f;
            while (chainLengthRemainingToDraw > 0f)
            {
                ExtraChainEffects(ref chainDrawPosition, chainCount);
                Color chainDrawColor = GetChainColor(chainDrawPosition, chainCount);
                var chainTextureToDraw = GetChainTexture(chainTexture, chainDrawPosition, chainCount);
                Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

                chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
                chainCount++;
                chainLengthRemainingToDraw -= chainSegmentLength;
            }
            return true;
        }
    }
}