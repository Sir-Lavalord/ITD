using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class StabtrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(StabtrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 20;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.Copper;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/StabtrapProjectileChain";

        }
        public Player player => Main.player[Projectile.owner];
        public override bool OneTimeLatchEffect()
        {
            AdvancedPopupRequest popupSettings = new()
            {
                Text = OneTimeLatchMessage.Value,
                Color = Color.RosyBrown,
                DurationInFrames = 60,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            ExtraFlexibility = 16f * 40f;
            Projectile stinger = MiscHelpers.NewProjectileDirectSafe(Projectile.GetSource_FromThis(), player.Center, Vector2.Zero,
            ModContent.ProjectileType<StabtrapStingerProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            if (stinger != null)
            {
                stinger.localAI[0] = Projectile.identity;
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ToChainTexture);
            Rectangle? chainSourceRectangle = null;
            float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 
                Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
                Vector2 chainDrawPosition = Projectile.Center;
                Vector2 vectorFromProjectileToPlayer = player.Center.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
                Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayer.SafeNormalize(Vector2.Zero);
                float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
                if (chainSegmentLength == 0)
                {
                    chainSegmentLength = 10;
                }
                float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
                int chainCount = 0;
                float chainLengthRemainingToDraw = vectorFromProjectileToPlayer.Length() + chainSegmentLength / 2f;
                while (chainLengthRemainingToDraw > 0f && player.ownedProjectileCounts[ModContent.ProjectileType<StabtrapStingerProjectile>()] <= 0)
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