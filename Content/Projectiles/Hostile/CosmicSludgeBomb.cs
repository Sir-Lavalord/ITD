using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ReLogic.Content;
using ITD.Content.NPCs.Bosses;
using SteelSeries.GameSense;
using Microsoft.Build.Evaluation;

namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicSludgeBomb : ModProjectile
    {
        private readonly Asset<Texture2D> effect = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Hostile/CosmicSludgeBomb_Effect");
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
            Main.projFrames[Projectile.type] = 7;
        }

        int defaultWidthHeight = 8;
        int explosionWidthHeight = 200;
        public override void SetDefaults()
        {
            Projectile.width = defaultWidthHeight; Projectile.height = defaultWidthHeight;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            DrawOffsetX = -16;
            DrawOriginOffsetY = -16;
            Projectile.hide = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f-(Projectile.alpha/255f));
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        bool isStuck = false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] != 0)
            {
                return true;
            }
            Projectile.velocity = Vector2.Zero;
            isStuck = true;
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[CosJel.target];
                width = 15;
                height = 15;
                fallThrough = player.Center.Y >= Projectile.Bottom.Y + 20;
            }
            return true;
                
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = effect.Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY + DrawOriginOffsetX) + new Vector2(DrawOffsetX, DrawOriginOffsetY) + new Vector2(4, 4);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        public int pulseSpeed;
        public int pulseTime;
        float Distance;
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        public override void AI()
        {
                if (expertMode || masterMode)
                {
                NPC CosJel = Main.npc[(int)Projectile.ai[0]];
                if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
                {
                    Player player = Main.player[CosJel.target];
                    if (player.Distance(Projectile.Center) < 20)
                    {
                        if (pulseTime++ >= 5)
                        {
                            pulseTime = 0;
                            pulseSpeed++;
                        }
                    }
                    if (pulseSpeed >= 8)
                    {
                        Projectile.Kill();
                    }
                }
            }
            if (isStuck == false)
            {
                Projectile.velocity.Y += 0.2f;
            }
            if (++Projectile.frameCounter >= 10 - pulseSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, Color.Purple, 2f);
                dust.velocity *= 1.4f;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<CosmicLightningBlast>(), (int)(Projectile.damage), Projectile.knockBack);
                explosion.ai[1] = 100f;
                explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                explosion.netUpdate = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0f, 0f, 100, default, 3f);
                dust.noGravity = true;
                dust.velocity *= 5f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.ShimmerSpark, 0f, 0f, 100, default, 2f);
                dust.velocity *= 3f;
            }
            for (int g = 0; g < 2; g++)
            {
                var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X += 1.5f;
                gore.velocity.Y += 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X -= 1.5f;
                gore.velocity.Y += 1.5f;
            }
        }
    }
}
