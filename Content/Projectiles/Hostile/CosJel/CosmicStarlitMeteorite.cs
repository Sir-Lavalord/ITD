using ITD.Content.NPCs.Bosses;
using ITD.Particles;
using ITD.Particles.Projectile;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicStarlitMeteorite : ModProjectile
    {   
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
            Main.projFrames[Projectile.type] = 1;
        }

        int defaultWidthHeight = 8;
        public ParticleEmitter emitter;
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            DrawOffsetX = -16;
            DrawOriginOffsetY = -16;
            Projectile.hide = true;
            Projectile.alpha = 0;
            emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;
            if (emitter != null)
                emitter.keptAlive = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num1 = 0f;
            NPC owner = MiscHelpers.NPCExists(ownerIndex, ModContent.NPCType<CosmicJellyfish>());
            if (owner == null)
            {
                Projectile.timeLeft = 0;
                Projectile.active = false;
                return base.Colliding(projHitbox, targetHitbox);
            }
            return base.Colliding(projHitbox, targetHitbox);

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 stretch = new Vector2(1f, 1f);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 center = Projectile.Size / 2f;
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];
                Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

            }
            Vector2 miragePos = Projectile.position - Main.screenPosition + center;
            Vector2 origin = new(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f);
            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;

            time %= 4f;
            time /= 2f;

            if (time >= 1f)
            {
                time = 2f - time;
            }

            time = time * 0.5f + 0.5f;
            if ((Projectile.localAI[0] >= 300))
            {
                for (float i = 0f; i < 1f; i += 0.35f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 4).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                }

                for (float i = 0f; i < 1f; i += 0.5f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode; 
        public int ownerIndex => (int)Projectile.ai[0];
        public override bool? CanDamage()
        {
            return Projectile.localAI[0] >= 120;
        }
        public override void AI()
        {
            NPC owner = MiscHelpers.NPCExists(ownerIndex, ModContent.NPCType<CosmicJellyfish>());
            if (owner == null)
            {
                Projectile.timeLeft = 0;
                Projectile.active = false;
                return;
            }
            if (++Projectile.localAI[0] < 120) //feed us and we'll grow
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, (owner.Center + new Vector2(Projectile.ai[1], Projectile.ai[2])), 0.05f);
                if (!Main.dedServ)
                {
                    if (emitter is null)
                    {
                        emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
                        emitter.additive = true;
                    }
                    emitter.keptAlive = true;
                    emitter.timeLeft = 180;

                    for (int i = 0; i <= 2; i++)
                    {
                        emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), Vector2.Zero);
                    }

                }
            }
            else
            {
                if (!Main.dedServ)
                {
                    if (emitter is null)
                    {
                        emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
                        emitter.additive = true;
                    }
                    emitter.keptAlive = true;
                    emitter.timeLeft = 180;
                    if (Projectile.localAI[0] == 120)
                    {

                        for (int i = 0; i < 18; i++)
                        {
                            emitter?.Emit(Projectile.Center, (Vector2.UnitX).RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10);
                        }

                    }
                }
            
                Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.1f, 0, 2);
                Projectile.velocity *= 0.5f;

            }
        }
        public override void OnKill(int timeLeft)
        {
            NPC owner = MiscHelpers.NPCExists(ownerIndex, ModContent.NPCType<CosmicJellyfish>());
            if (owner == null)
            {
                Projectile.timeLeft = 0;
                Projectile.active = false;
                return;
            }
            int amount = 20;
            for (int i = 0; i < amount; i++)
            {

                double rad = Math.PI / (amount / 2) * i;
                int damage = (int)(Projectile.damage * 0.28f);
                int knockBack = 3;
                float speed = 12f;
                Vector2 vector = Vector2.Normalize(Vector2.UnitY.RotatedBy(rad)) * speed;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ModContent.ProjectileType<CosmicStar>(), damage, knockBack, Main.myPlayer, 0, 1);
                }
            }
/*            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CosmicGlowStar>(),
                Projectile.damage,
                0f,
                -1, Main.player[owner.target].whoAmI, 30);
            proj.localAI[0] = 1.5f;*/
        }
    }
}
