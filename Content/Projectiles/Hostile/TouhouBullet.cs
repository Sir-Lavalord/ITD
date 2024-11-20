
using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SteelSeries.GameSense;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile
{
    public class TouhouBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Tofu");
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;

        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {

                if (Projectile.ai[1] != 2)
                    Projectile.velocity = (CosJel.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f;
                else
                {
                    scaleX = 0.2f;
                    scaleY = 0.2f;
                    Projectile.velocity = (CosJel.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.05f;
                }
            }
            else
            {
                Projectile.Kill();
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
        public override bool? CanDamage()
        {
            if (Projectile.ai[1] == 2)
            {
                if (Projectile.ai[2] >= 30)
                    return true;
                else return false;
            }
            return true;
        }
        public override void AI()
        {

            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                if (Vector2.Distance(Projectile.Center, CosJel.Center) < 30)
                {
                    if (Projectile.ai[1] == 2)
                    {
                        Projectile.Kill();
                    }
                    Projectile.Kill();
                }
            }
            if (Projectile.ai[1] == 2)
            {
                if (Projectile.ai[2]++ >= 30)
                {
                    Projectile.extraUpdates = 1;
                    Projectile.velocity *= 1.03f;
                }
                else
                {
                     scaleX += .05f;
                    scaleY += .05f;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        }
        float scaleX = 1f;
        float scaleY = 2.5f;
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            Main.instance.DrawCacheProjsBehindNPCs.Add(index);

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D effectTexture = TextureAssets.Extra[98].Value;
            Vector2 effectOrigin = effectTexture.Size() / 2f;
            lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;;
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(255, 255, 255, 127), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
            return false;
        }
    }
}

