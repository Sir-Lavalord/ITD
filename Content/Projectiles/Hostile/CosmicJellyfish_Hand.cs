using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.DataStructures;
using ITD.Content.NPCs.Bosses;
using ITD.Particles.CosJel;
using ITD.Particles;
using System.Linq;


namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicJellyfish_Hand : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // The recording mode
            Main.projFrames[Projectile.type] = 7;//I'm very mature
        }
        public override void SetDefaults()
        {
            Projectile.width = 64; Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        //AI
        //0.0 is normal
        //0.1 is smackdown
        //1.1 is call to stop spawn explosion
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 15;
            height = 15;
            NPC NPC = Main.npc[(int)Projectile.ai[2]];
            if (NPC.active && NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[NPC.target];
                fallThrough = player.Center.Y >= Projectile.Bottom.Y - 10 && Projectile.ai[1] != 1;
            }

            return true;


        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 1)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                if (Projectile.tileCollide)
                {
                    if (Projectile.ai[1] != 1)
                    {
                        if (expertMode || masterMode)
                        {

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile Blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<CosmicLightningBlast>(), (int)(Projectile.damage), 2f, -1, Projectile.owner);
                                Blast.ai[1] = 100f;
                                Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                                Blast.netUpdate = true;

                            }
                        }
                        for (int i = 0; i < 12; i++)
                        {
                            Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerTorch, new Vector2(Main.rand.NextFloat() * 6f, -8f + 8f * Main.rand.NextFloat()), 0, default(Color), 1.5f).noGravity = true;
                        }
                        Projectile.ai[1] = 1;

                        Projectile.position += Projectile.velocity;
                        Projectile.velocity = Vector2.Zero;
                    }
                }
                Projectile.netUpdate = true;

            }

            return false;

        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0.1f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {

                    ITDParticle spaceMist = ParticleSystem.NewParticle<SpaceMist>(Projectile.Center, (-Projectile.velocity).RotatedByRandom(3f), 5f);
                    spaceMist.tag = Projectile;
                
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                ITDParticle spaceMist = ParticleSystem.NewParticle<SpaceMist>(Projectile.Center, (-Projectile.velocity).RotatedByRandom(1f), 0f);
                spaceMist.tag = Projectile;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, SpriteEffects.None, 0f);
            }
            DrawAtProj(outline);
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                if (mist is SpaceMist sMist)
                {
                    sMist.DrawOutline(sb);
                }
            }
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                mist.DrawCommon(sb, mist.texture, mist.CanvasOffset);
            }
            DrawAtProj(texture);
            return false;
        }
    }
}