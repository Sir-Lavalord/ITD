using ITD.Content.Dusts;
using ITD.Particles;
using ITD.Particles.Projectiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicFistBarrier : ModProjectile
{
    private enum ActionState
    {
        Phasing,
        Ramming,
        Spamming
    }
    public VertexStrip TrailStrip = new();
    public ParticleEmitter emitter;

    public override string Texture => "ITD/Content/Projectiles/Hostile/CosJel/CosmicFistBump";

    private ActionState AI_State;
    private float distFromPlayer => Projectile.ai[2];
    private Vector2 playerPos = Vector2.Zero;
    private Vector2 defaultPos = Vector2.Zero;

    public bool isMainHand => Projectile.ai[1] == 0;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        Main.projFrames[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.scale = 1.25f;
        Projectile.Opacity = 0;
        emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }
    public override bool? CanDamage()
    {
        return AI_State != ActionState.Phasing || Projectile.alpha <= 0;
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
    }
    static bool isExpert => Main.expertMode;
    static bool isMaster => Main.masterMode;
    public override void AI()
    {
        Player player = Main.player[(int)Projectile.ai[0]];
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
                emitter?.Emit(Projectile.Top + Main.rand.NextVector2Circular(Projectile.width / 3, Projectile.height / 3), -Vector2.UnitY * 3f);
            }

        }
        switch (AI_State)
        {

            case ActionState.Phasing://phasing into existence, can't damage player
                defaultPos = Projectile.TopLeft;
                Projectile.Opacity = Projectile.localAI[1] / 30;
                Projectile.frame = 1;
                if (Projectile.localAI[1]++ >= 30)
                {
                    AI_State = ActionState.Ramming;
                    Projectile.localAI[1] = 0;
                }
                int dustRings = 3;
                for (int h = 0; h < dustRings; h++)
                {
                    float distanceDivisor = h + 1.5f;
                    float dustDistance = 200 / distanceDivisor;
                    int numDust = (int)(0.1f * MathHelper.TwoPi * dustDistance);
                    float angleIncrement = MathHelper.TwoPi / numDust;
                    Vector2 dustOffset = new(dustDistance, 0f);
                    dustOffset = dustOffset.RotatedByRandom(MathHelper.TwoPi);

                    int var = (int)dustDistance;
                    float dustVelocity = 20f / distanceDivisor;
                    for (int i = 0; i < numDust; i++)
                    {
                        if (Main.rand.NextBool(var))
                        {
                            dustOffset = dustOffset.RotatedBy(angleIncrement);
                            int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<CosJelDust>());
                            Main.dust[dust].position = Projectile.Center + dustOffset;
                            Main.dust[dust].fadeIn = 1f;
                            Main.dust[dust].velocity = Vector2.Normalize(Projectile.Center - Main.dust[dust].position) * dustVelocity;
                            Main.dust[dust].scale = 1.5f - h;
                        }
                    }
                }
                Projectile.rotation = 0;
                Projectile.Center = player.Center + new Vector2(isMainHand ? distFromPlayer : -distFromPlayer, -500);
                playerPos = Projectile.Center;
                playerPos.Y += 1000;
                break;
            case ActionState.Ramming:
                if (Projectile.localAI[1]++ <= 20)
                {
                    Projectile.Center += Vector2.Normalize(Projectile.Center - playerPos) * 3;
                }
                else
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, playerPos, 0.3f);
                }
                if (Projectile.timeLeft <= 60)
                {
                    Projectile.alpha += 5;

                    if (Projectile.alpha > 255)
                    {
                        Projectile.Kill();
                    }
                }
                else
                {
                    Rectangle rect = new((int)Projectile.TopLeft.X, (int)Projectile.TopLeft.Y, (int)Vector2.Distance(Projectile.BottomLeft, Projectile.BottomRight), (int)(-200 - Projectile.localAI[1] * 8));
                    Vector2 spawnPos = Main.rand.NextVector2FromRectangle(rect);
                    if (Vector2.Distance(Projectile.Center, playerPos) <= 10)
                    {
                        if (Main.rand.NextBool(2))
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int damage = (int)(Projectile.damage * 0.28f);
                                int knockBack = 3;
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<CosmicStar>(), damage, knockBack, Main.myPlayer, 0, 1);
                            }

                            for (int i = 0; i < 10; i++)
                            {
                                Dust.NewDust(spawnPos, 2, 2, ModContent.DustType<StarlitDust>(), Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1);
                            }
                        }
                        if (!Main.dedServ)
                        {
                            if (emitter is null)
                            {
                                emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
                                emitter.additive = true;
                            }
                            emitter.keptAlive = true;
                            emitter.timeLeft = 180;
                            emitter.scale = 2;
                            for (int i = 0; i <= 6; i++)
                            {
                                emitter?.Emit(spawnPos + Main.rand.NextVector2Circular(5, 5), -Vector2.UnitY * 3f);
                            }

                        }
                    }
                }
                break;
        }
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 center = Projectile.Size / 2f;
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

        }
        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);
        //old treasure bag draw code, augh
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 1f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) *
                time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, !isMainHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) *
                time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, !isMainHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, !isMainHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}