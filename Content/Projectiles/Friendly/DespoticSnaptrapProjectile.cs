using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Dusts;

namespace ITD.Content.Projectiles
{
    public class DespoticSnaptrapProjectile : ITDSnaptrap
    {
        private const string ChainTextureExtraPath = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain1";
        private const string ChainTextureExtra2Path = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain2";
        public override void SetSnaptrapProperties()
        {
            shootRange = 16f * 20f;
            retractAccel = 1.5f;
            framesUntilRetractable = 10;
            extraFlexibility = 16f * 6f;
            framesBetweenHits = 16;
            minDamage = 3560;
            maxDamage = 8900;
            fullPowerHitsAmount = 10;
            warningFrames = 200;
            chompDust = DustID.IceTorch;
            toChainTexture = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain";
            DrawOffsetX = -20;
            DrawOriginOffsetY = -24;
            Projectile.extraUpdates = 1;
        }

        public override void PostAI()
        {
            Dust.NewDust(Projectile.Center, 6, 6, chompDust, 0f, 0f, 0, default, 1f);
            Dust.NewDust(Projectile.Center, 6, 6, DustID.MushroomTorch, 0f, 0f, 0, Color.Blue, 2f);
            if (!retracting)
            {
                if (Main.rand.NextBool() == true)
                {
                    Dust.NewDust(Projectile.Center, 4, 4, ModContent.DustType<DespoticDust>(), 0, 0, 0, default(Color), 1f);
                }
            }
        }
        public override Asset<Texture2D> GetChainTexture(Asset<Texture2D> defaultTexture, Vector2 chainDrawPosition, int chainCount)
        {
            if (chainCount >= 12)
            {
                
            }
            else if (chainCount >= 8)
            {
                return ModContent.Request<Texture2D>(ChainTextureExtraPath);
            }
            else
            {
                return ModContent.Request<Texture2D>(ChainTextureExtra2Path);
            }
            return defaultTexture;
        }
        public override Color GetChainColor(Vector2 chainDrawPosition, int chainCount)
        {
            var chainDrawColor = base.GetChainColor(chainDrawPosition, chainCount);
            if (chainCount >= 12)
            {
                // Use normal chainTexture and lighting, no changes
            }
            else if (chainCount >= 8)
            {
                // Near to the ball, we draw a custom chain texture and slightly make it glow if unlit.
                byte minValue = 140;
                if (chainDrawColor.R < minValue)
                    chainDrawColor.R = minValue;

                if (chainDrawColor.G < minValue)
                    chainDrawColor.G = minValue;

                if (chainDrawColor.B < minValue)
                    chainDrawColor.B = minValue;
            }
            else
            {
                // Close to the ball, we draw a custom chain texture and draw it at full brightness glow.
                return Color.White;
            }
            return chainDrawColor;
        }
    }
}