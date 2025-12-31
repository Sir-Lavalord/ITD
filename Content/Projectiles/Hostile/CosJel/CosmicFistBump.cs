using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicFistBump : ModProjectile
{
    private enum ActionState
    {
        Phasing,
        Spinning,
        Ramming,
        Spamming,
        Chainbombing
    }
    private ActionState AI_State;
    public int OwnerIndex => (int)Projectile.ai[0];
    private float PhaseTime => Projectile.ai[2];
    private Vector2 playerPos = Vector2.Zero;
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
        Projectile.timeLeft = 900;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.scale = 1.25f;
        Projectile.Opacity = 0;
    }
    public override bool? CanDamage()
    {
        return AI_State != ActionState.Phasing || Projectile.alpha <= 0;
    }
    public override void OnSpawn(IEntitySource source)
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
    }
    static bool isExpert => Main.expertMode;
    static bool isMaster => Main.masterMode;
    public override void AI()
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        if (owner.ai[3] == -1)//5 is current place of meteor slam attack
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        Player player = Main.player[owner.target];
        switch (AI_State)
        {
            
            case ActionState.Phasing://phasing into existence, can't damage player
                Projectile.Opacity = Projectile.localAI[1] / 40;

                if (Projectile.localAI[1]++ >= 40)
                {
                    AI_State = ActionState.Spinning;
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
                Projectile.rotation = (Projectile.Center - player.Center).ToRotation() + MathHelper.PiOver2;
                Projectile.Center = player.Center + new Vector2(isMainHand ? 200 : -200, 0).RotatedBy(Projectile.localAI[0]);
                break;
            case ActionState.Spinning:

                Projectile.frame = 1;
                Projectile.localAI[0] -= (float)Math.PI / 120f;
                if (Projectile.localAI[1]++ >= PhaseTime)
                {
                    AI_State = ActionState.Ramming;
                    Projectile.localAI[1] = 0;
                }
                Projectile.rotation = (Projectile.Center - player.Center).ToRotation() + MathHelper.PiOver2;
                Projectile.Center = player.Center + new Vector2(isMainHand ? 200 : -200, 0).RotatedBy(Projectile.localAI[0]);
                playerPos = player.Center;
                break;
            case ActionState.Ramming:
                if (Projectile.localAI[1]++ <= 30)
                {
                    Projectile.Center += Vector2.Normalize(Projectile.Center - playerPos) * 3;
                }
                else
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, playerPos, 0.3f);
                    if (Vector2.Distance(Projectile.Center, playerPos) <= 10)
                    {

                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                                ModContent.ProjectileType<CosmicFistTelegraph>(), 0, 0f, Main.myPlayer, Projectile.rotation, Projectile.whoAmI, 0f);
                        

                        Projectile.localAI[1] = 0;
                        player.GetITDPlayer().BetterScreenshake(10, 10, 10, true);

                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 velo = Projectile.rotation.ToRotationVector2() * 10;
                            Dust dust = Dust.NewDustDirect(Projectile.Center, 10, 10, ModContent.DustType<CosJelDust>(), 0, 0, 60, default, Main.rand.NextFloat(1.5f, 2f));
                            dust.noGravity = false;
                            dust.velocity = velo.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.75f, 1.25f);
                        }
                        if (isMainHand)
                        {
                            if (owner.localAI[2] != 0)
                            {
                                int amount = 6;
                                for (int i = 0; i < amount; i++)
                                {
                                    double rad = Math.PI / (amount / 2) * i;
                                    int damage = (int)(Projectile.damage * 0.28f);
                                    int knockBack = 3;
                                    float speed = 12f;
                                    Vector2 vector = Vector2.Normalize(Vector2.UnitY.RotatedBy(rad)) * speed;

                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ModContent.ProjectileType<CosmicStar>(), damage, knockBack, Main.myPlayer, 0, 1);

                                }
                            }
                        }
                        SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);

                        AI_State = ActionState.Spamming;
                    }
                }
                break;
            case ActionState.Spamming:
                if (Projectile.localAI[1]++ >= 30 && Projectile.localAI[1] < 180)
                {
                    player.GetITDPlayer().BetterScreenshake(4, 2, 2, true);
                    Vector2 velo = Projectile.rotation.ToRotationVector2() * 8;
                    SoundEngine.PlaySound(SoundID.Item103, Projectile.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        if (Main.rand.NextBool(2))
                        {
                            Vector2 veloDelta = Projectile.velocity;
                            Projectile proj1 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, -((velo * 2f) + veloDelta).RotatedByRandom(0.75f), ModContent.ProjectileType<CosmicWave>(), Projectile.damage, 0, Main.myPlayer);
                            proj1.tileCollide = false;
                            proj1.friendly = false;
                            proj1.hostile = true;
                        }
                    }
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 10, 10, ModContent.DustType<CosJelDust>(), 0, 0, 60, default, Main.rand.NextFloat(1.5f, 2f));
                    dust.noGravity = false;
                    dust.velocity = velo.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.75f, 1.25f);


                }
                else
                if (Projectile.localAI[1] >= 180)
                {
                    if (isExpert || isMaster)
                    {
                        Projectile.localAI[1] = 0;
                        AI_State = ActionState.Chainbombing;
                    }
                    Projectile.alpha += 2;

                    if (Projectile.alpha > 255)
                    {
                        Projectile.Kill();
                    }

                }
                break;
            case ActionState.Chainbombing:
                if (Projectile.localAI[1]++ <= 30)
                {
                    Projectile.Center += Vector2.Normalize(Projectile.Center - playerPos) * 4;
                }
                else
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, playerPos, 0.5f);
                    if (Vector2.Distance(Projectile.Center, playerPos) <= 10)
                    {
                        if (isMainHand)
                        {
                            if (owner.localAI[2] != 0)
                            {
                                int amount = 6;
                                for (int i = 0; i < amount; i++)
                                {
                                    double rad = Math.PI / (amount / 2) * i;
                                    int damage = (int)(Projectile.damage * 0.28f);
                                    int knockBack = 3;
                                    float speed = 12f;
                                    Vector2 vector = Vector2.Normalize(Vector2.UnitY.RotatedBy(rad)) * speed;

                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ModContent.ProjectileType<CosmicStar>(), damage, knockBack, Main.myPlayer, 0, 1);

                                }
                            }
                        }
                        Projectile.Kill();
                        player.GetITDPlayer().BetterScreenshake(10, 10, 10, true);
                    }
                }
                break;
        }
    }
    public override void OnKill(int timeLeft)
    {
        if (AI_State == ActionState.Chainbombing)
        {
            if (isMainHand)
            {
            }


            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center +
            new Vector2(Projectile.width / 2, 0).RotatedBy(Projectile.rotation), Vector2.Zero, ModContent.ProjectileType<CosmicChainBomb>(),
            (int)(Projectile.damage), 0f, Main.myPlayer, 40, Projectile.rotation + MathHelper.PiOver2, 0f);
        

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
            }
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

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
}