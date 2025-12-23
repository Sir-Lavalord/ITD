using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Particles;
using ITD.Particles.CosJel;
using ITD.Particles.Projectiles;
using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicSwarmBlackhole : ITDProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        Main.projFrames[Projectile.type] = 1;
    }
    public VertexStrip TrailStrip = new();
    readonly int defaultWidthHeight = 8;
    public ParticleEmitter emitter;
    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 100;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.hide = false;
        Projectile.alpha = 0;
        Projectile.scale = 1; 
        Projectile.netImportant = true;
        emitter = ParticleSystem.NewEmitter<SpaceMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }
    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);

        Projectile.scale = 0;
        if (emitter != null)
            emitter.keptAlive = true;
    }
    private enum ActionState
    {
        Opening,
        Aligning,
        Spamming,
        Closing
    }
    private ActionState AI_State;
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * (1f - Projectile.alpha / 255f);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return base.Colliding(projHitbox, targetHitbox);
        }
        return base.Colliding(projHitbox, targetHitbox);

    }
    float innerScale = 1f;

    public override int ProjectileShader(int originalShader)
    {
        return default;
    }
    public int OwnerIndex => (int)Projectile.ai[0];
    public override bool? CanDamage()
    {
        return Projectile.localAI[0] >= 120;
    }
    public override void AI()
    {
        if (++Projectile.frameCounter >=8 )
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        Player player = Main.player[owner.target];
        switch (AI_State)
        {
            case ActionState.Opening:
                Projectile.Center = new Vector2(player.Center.X, MathHelper.Lerp(Projectile.Center.Y, player.Center.Y + Main.screenHeight / 3, 0.1f));

                Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.02f, 0, 1);
                if (Projectile.scale >= 1)
                {
                    AI_State = ActionState.Aligning;
                    
                }
                break;
            case ActionState.Aligning:
                if (Projectile.localAI[0]++ <= 45)
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, new Vector2(player.Center.X + player.velocity.X * 60, player.Center.Y + Main.screenHeight / 3) ,0.2f);
                }
                else
                {
                    for (int i = 0; i < 20; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
                    }
                    SoundEngine.PlaySound(SoundID.Item109, Projectile.Center);

                    Projectile.localAI[0] = 0;
                    spawnGlow = 1;
                    AI_State = ActionState.Spamming;
                }
                break;
            case ActionState.Spamming:
                if (Projectile.localAI[0]++ <= 200)
                {
                    if (Projectile.scale > 1)
                    Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.05f, 1, 2);
                    if (Main.essScale >= 1)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
                        }
                    }
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile other = Main.projectile[i];

                        if (other.type == ModContent.ProjectileType<CosmicSwarm>() && other.active && other.timeLeft > 0
                            && Math.Abs(Projectile.Center.X - other.Center.X) < Projectile.width 
                            && Math.Abs(Projectile.Center.Y - other.Center.Y) < Projectile.height)
                        {
                            Projectile.scale += 0.1f;
                            player.GetModPlayer<ITDPlayer>().BetterScreenshake(2, 2, 2, true);
                            other.Kill();
                            other.active = false;
                            for (int j = 0; j < 20; j++)
                            {
                                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
                                dust.noGravity = true;
                                dust.velocity = new Vector2(0, -20).RotatedByRandom(MathHelper.ToRadians(10));

                            }

                            int amount = 11;
                            for (int k = 0; k < amount; k++)
                            {

                                float rad = MathHelper.PiOver2 / (amount / 2) * k + MathHelper.PiOver2;
                                int damage = (int)(Projectile.damage * 0.28f);
                                int knockBack = 3;
                                float speed = 18f;
                                Vector2 vector = Vector2.Normalize(Vector2.UnitY.RotatedBy(rad)) * speed;
                                vector = vector.RotatedByRandom(0.06f);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ModContent.ProjectileType<CosmicSwarmGib>(), damage, knockBack, Main.myPlayer, 0, 1);
                                }
                            }

                        }
                    }
                    if (Projectile.localAI[0] % 5 == 0 && Projectile.localAI[0] <= 140)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int damage = (int)(Projectile.damage * 0.28f);
                            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), 
                                Projectile.Center - new Vector2(Main.rand.NextFloat(-600,600), Main.rand.NextFloat(2000,2600)),  Vector2.Zero,
                                ModContent.ProjectileType<CosmicSwarm>(), damage, 0, Main.myPlayer, 0, 1);
                            proj.velocity = Vector2.Normalize(Projectile.Center - proj.Center) * 24 * Main.rand.NextFloat(1.25f,1.75f);
                            proj.rotation = proj.velocity.ToRotation();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 20; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
                    }
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
                    AI_State = ActionState.Closing;
                }
                break;
            case ActionState.Closing:

                Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.1f, 0, 1);
                if (Projectile.scale <= 0)
                {
                    Projectile.Kill();
                }
                break;

        }
        if (pulseGlow <= 0)
        {
            pulseGlow = 1;
        }
        pulseGlow -= 0.01f;
        spawnGlow -= 0.025f;
    }

    public override void OnKill(int timeLeft)
    {
    }
    float scaleX = 3f;
    float scaleY = 3f;
    float spawnGlow = 0;
    float pulseGlow = 0;

    public override bool PreDraw(ref Color lightColor)
    {
        float slowPulse = 1 + (Main.essScale / 3);

        Player player = Main.player[Projectile.owner];
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.PortalGateHalo].Value;
        Texture2D effectTexture2 = TextureAssets.Extra[ExtrasID.PortalGateHalo2].Value;
        Texture2D telegraphThorn = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;


        Vector2 effectOrigin = effectTexture.Size() / 2f;
        Vector2 effectOrigin2 = effectTexture2.Size() / 2f;
        Vector2 telegraphOrigin = telegraphThorn.Size() / 2f;

        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(effectTexture2, drawPosition + new Vector2(0,-60), null, new Color(90, 70, 255, 0), Projectile.rotation, effectTexture2.Size() / 2f, new Vector2(3 * Projectile.scale, 6) * slowPulse, SpriteEffects.None, 0);
       
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;
        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.5f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(0f, 6).RotatedBy(radians) * time, null, new Color(90, 70, 255, 0) * Projectile.Opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(4 * Projectile.scale, 1), SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(0f, 2).RotatedBy(radians) * time, null, new Color(90, 70, 255, 0) * Projectile.Opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(4 * Projectile.scale, 1), SpriteEffects.None, 0);
        }
        if (spawnGlow > 0)//fargo eridanus epic
        {
            float scale = 2.5f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(90, 70, 255, 0), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(4 * Projectile.scale, 1) * scale, SpriteEffects.None, 0);
        }
        if (pulseGlow > 0)//fargo eridanus epic
        {
            float scale = 2.25f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * pulseGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(pulseGlow);
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(90, 70, 255, 0), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(4 * Projectile.scale, 1) * slowPulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(10, 70, 255, 0) * opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(4 * Projectile.scale, 1) * scale, SpriteEffects.None, 0);
        }
        return false;
    }
}
