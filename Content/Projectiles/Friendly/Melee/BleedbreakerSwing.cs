using ITD.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using ITD.Particles.Projectile;
using ITD.Particles;
using ITD.Content.NPCs.Friendly;

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
                    modifiers.FinalDamage.Flat = (int)(Projectile.damage * (swingCharge/120));
                    break;
                case 3:
                    modifiers.FinalDamage.Flat = (int)(Projectile.damage * (swingCharge / 240));
                    break;
            }
        }
        public int swingCharge;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hitFrame != Projectile.frame && Projectile.frame <= 5)
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
                                target.velocity.X = Projectile.spriteDirection * 18f;

                                break;
                            case 3:
                                target.velocity.Y = Main.rand.NextFloat(-2, -1);
                                target.velocity.X = Projectile.spriteDirection * 12f;
                                break;
                        }
                    }
                    else if (!target.Gimmickable())
                    {
/*                        switch (Projectile.frame)
                        {
                            case 2:

                                damageDone = (int)(Projectile.damage * 2);
                                break;
                            case 3:

                                damageDone = (int)(Projectile.damage * 1.25f);
                                break;
                        }*/
                    }
                }
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        int hitreg;
        int hitFrame = 0;
        public override void AI()
        {
            Main.NewText((Projectile.frame, hitFrame,Projectile.direction,Projectile.spriteDirection));
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
            if (hitreg-- <= 0)
            {
                Projectile.frameCounter++;
            }
            else
            {
                Projectile.Center += Main.rand.NextVector2Circular(5, 5);
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
            if (!player.channel)
            {
                Projectile.direction = Projectile.spriteDirection;

                player.direction = Projectile.spriteDirection;
                if (Projectile.frameCounter >= 7)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                    {

                        Projectile.Kill();
                    }
                }
            }
            else
            {
                Projectile.spriteDirection = player.direction;
                if (swingCharge < 120)
                swingCharge++;
                if (Main.rand.NextBool(4))
                {
                    int dust = Dust.NewDust(Projectile.position, 1, 1, DustID.RedTorch, 0, 0, 0, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = new Vector2(0, 4).RotatedByRandom(3f) * Main.rand.NextFloat(0.9f, 1.1f);
                }
            }
        }
        public override bool? CanDamage()
        {
            return Projectile.frame > 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            sb.Draw(texture, new Vector2(Projectile.Center.X, Projectile.Center.Y - 80) - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(frame.Width * 0.5f, (frame.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }
    }
}