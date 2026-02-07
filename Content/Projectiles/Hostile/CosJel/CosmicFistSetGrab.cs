using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicFistSetGrab : ModProjectile
{
    public override string Texture => "ITD/Content/Projectiles/Hostile/CosJel/CosmicFistBump";

    private enum ActionState
    {
        Phasing,
        GetWater,
        Grabbing,
        Releasing
    }
    private ActionState AI_State;
    public int OwnerIndex => (int)Projectile.ai[0];
    //time to release
    private float DragTime => Projectile.ai[2];
    public bool isLeftHand => Projectile.ai[1] <= 0;
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
        if (owner.ai[3] == -1)//trout
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        Player player = Main.player[owner.target];
        float dir = (isLeftHand ? -1 : 1);
        switch (AI_State)
        {
            case ActionState.Phasing://phasing into existence, can't damage player
                Projectile.Opacity = Projectile.localAI[1] / 40;

                if (Projectile.localAI[1]++ >= 40)
                {
                    AI_State = ActionState.GetWater;
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
                Projectile.Center = owner.Center + new Vector2(dir * 300, 0);
                Projectile.rotation = MathHelper.PiOver2 * -dir;
                break;
            case ActionState.GetWater:
                Projectile.frame = 1;
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2((Main.screenWidth) / dir, 0), 0.015f);
                if (Projectile.localAI[1]++ >= 60)
                {
                    AI_State = ActionState.Grabbing;
                    Projectile.localAI[1] = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile water1 = Projectile.NewProjectileDirect(owner.GetSource_FromAI(),
                            Projectile.Center, Vector2.Zero,
                            ModContent.ProjectileType<CosmicWaterWall>(), (int)(Projectile.damage * 0.25f), 0, -1, owner.whoAmI, Projectile.whoAmI);
                        water1.rotation = MathHelper.PiOver2 * -dir;
                    }
                }
                Projectile.rotation = MathHelper.PiOver2 * -dir;
                break;
            case ActionState.Grabbing:
                if (Projectile.localAI[1]++ <= 90)
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2((Main.screenWidth / 4) / dir, 0), 0.015f);
                }
                else
                {
                    Projectile.Center += Main.rand.NextVector2Circular(2, 2);
                    if (Projectile.localAI[0]++ >= 90)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
                        }
                        player.GetITDPlayer().BetterScreenshake(30, 10, 0, true);
                        Projectile.ai[2] = 1;
                        AI_State = ActionState.Releasing;
                        Projectile.localAI[1] = 0;
                        Projectile.localAI[0] = 0;
                        Projectile.Opacity = 0;
                    }
                }
                Projectile.rotation = MathHelper.PiOver2 * -dir;

                break;
            case ActionState.Releasing:
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2((Main.screenWidth) * 2 / dir, 0), 0.015f);
                if (Projectile.localAI[0]++ >= 30)
                {
                    Projectile.Kill();
                }
                break;
        }
    }
    public override void OnKill(int timeLeft)
    {
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

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame, new Color(253, 241, 186, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame,
                new Color(253, 241, 186, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale,
            isLeftHand? SpriteEffects.None:SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}