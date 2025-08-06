using ITD.Content.NPCs.Friendly;
using ITD.Particles;
using ITD.Particles.Projectile;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Map;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class BleedbreakerSwing : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 8;

        }        public ParticleEmitter emitter;
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 178;
            Projectile.height = 166;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 2000;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 15;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.1f;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 20;
            Projectile.noEnchantmentVisuals = true;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            emitter = ParticleSystem.NewEmitter<TheEpicenterFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
            emitter.tag = Projectile;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - (Projectile.alpha / 255f));
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            switch (Projectile.frame)
            {
                case 2:
                    modifiers.FinalDamage.Flat = (int)(Projectile.damage * (swingCharge/60));
                    break;
                case 3:
                    modifiers.FinalDamage.Flat = (int)(Projectile.damage * (swingCharge / 120));
                    break;
            }
        }
        public int swingCharge;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hitFrame != Projectile.frame && Projectile.frame <= 5 && Projectile.frame > 1)
            {
                hitFrame = Projectile.frame;
                if (hitreg <= 0)
                {
                    hitreg = 40;

                    Player player = Main.player[Projectile.owner];
                    player.GetITDPlayer().BetterScreenshake(10, 8, 5, false);
                    SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), target.Center);

                    for (int i = 0; i < 6; i++)
                    {
                        Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.RedTorch, Projectile.direction * 8f, Main.rand.NextFloat(-6, -4), 60, default, Main.rand.NextFloat(1f, 1.2f));
                        dust.noGravity = true;
                        dust.velocity *= 2f;
                        Dust dust1 = Dust.NewDustDirect(target.Center, 1, 1, DustID.Blood, Projectile.direction * 5f, Main.rand.NextFloat(-6, -4), 60, default, Main.rand.NextFloat(1f, 1.2f));
                        dust1.noGravity = false;

                    }
                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                        new ParticleOrchestraSettings { PositionInWorld = target.Center }, target.whoAmI);
                    if (target.Gimmickable() || target.type == ModContent.NPCType<StrawmanDummy>() && target.ai[0] == 6)
                    {
                        switch (Projectile.frame)
                        {
                            case 2:
                                Projectile bomb = Projectile.NewProjectileDirect(target.GetSource_FromThis(), target.Center, Vector2.Zero,
        ModContent.ProjectileType<BleedbreakerKnockbackBomb>(), (int)(Projectile.damage * (1 +swingCharge / 120)), 0f, -1, target.whoAmI, Projectile.spriteDirection);
                                bomb.height = (int)(target.height * 0.5f);
                                bomb.width = (int)(target.width * 0.5f);

                                target.velocity.Y = Main.rand.NextFloat(-6, -4);
                                target.velocity.X = Projectile.spriteDirection * 6 * ((swingCharge/60) + 1);

                                break;
                            case 3:
                                target.velocity.Y = Main.rand.NextFloat(-2, -1);
                                target.velocity.X = Projectile.spriteDirection * 4 * ((swingCharge / 60) + 1);
                                break;
                        }
                    }
                }
            }
        }
        int hitreg;
        int hitFrame = 0;
        float angle;
        public override void AI()
        {
            Projectile.alpha = 255;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter + new Vector2(0,-20);
            if (hitreg-- <= 0)
            {
                Projectile.frameCounter++;
            }
            else
            {
                Projectile.Center += Main.rand.NextVector2Circular(7, 5);
            }
            if (emitter != null)
                emitter.keptAlive = true;

            switch (Projectile.frame)
            {
                case 0:
                    Projectile.Resize((int)(50 * Projectile.scale), (int)(110 * Projectile.scale));
                    break;
                case 1:
                    Projectile.Resize((int)(50 * Projectile.scale), (int)(110 * Projectile.scale));
                    break;
                case 2:
                    Projectile.Resize((int)(300 * Projectile.scale), (int)(200 * Projectile.scale));
                    break;
                case 3:
                    Projectile.Resize((int)(260 * Projectile.scale), (int)(180 * Projectile.scale));
                    break;
                case 4:
                    Projectile.Resize((int)(220 * Projectile.scale), (int)(150 * Projectile.scale));
                    break;
                case 5:
                    Projectile.Resize((int)(180 * Projectile.scale), (int)(100 * Projectile.scale));
                    break;
            }
            player.heldProj = Projectile.whoAmI;
            if (!player.channel)
            {
                if (Projectile.frameCounter >= 8)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                    {

                        Projectile.Kill();
                    }
                }
                if (angle <= MathHelper.Pi * 1.25f)
                    angle += 0.2f;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                        (MathHelper.Pi + angle) * player.direction);
            }
            else
            {
                Projectile.spriteDirection = player.direction;
                if (swingCharge < 120)
                swingCharge++;
                if (Main.rand.NextBool(4))
                {
                    int dust = Dust.NewDust(Projectile.Center + new Vector2(-20 * Projectile.spriteDirection, -Projectile.height/1.25f), 1, 1, DustID.RedTorch, 0, 0, 0, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = new Vector2(0, 4).RotatedByRandom(3f) * Main.rand.NextFloat(0.9f, 1.1f);
                }
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,  MathHelper.Pi);

            }
        }
        public override bool? CanDamage()
        {
            return Projectile.frame > 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 center = Projectile.Size / 2f;
            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;
            Vector2 miragePos = Projectile.position - Main.screenPosition + center;
            Vector2 origin = new(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f);
            time %= 4f;
            time /= 2f;

            if (time >= 1f)
            {
                time = 2f - time;
            }

            time = time * 0.5f + 0.5f;


            if (player.channel)
            {
                for (float i = 0f; i < 1f; i += 0.3f )
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 2 + swingCharge/60).RotatedBy(radians) * time, frame, new Color(255, 71, 71, 50), Projectile.rotation, origin, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                }
            }
            
            Main.EntitySpriteDraw(tex, miragePos, frame, Color.White, Projectile.rotation, origin, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }
    }
}