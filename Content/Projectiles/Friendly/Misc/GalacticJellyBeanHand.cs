using ITD.Content.Items.Accessories.Expert;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class GalacticJellyBeanHand : ModProjectile
{
    private NPC HomingTarget
    {
        get => Projectile.ai[2] == -1 ? null : Main.npc[(int)Projectile.ai[2]];
        set
        {
            Projectile.ai[2] = value == null ? -1 : value.whoAmI;
        }
    }

    private const float homingDistance = 500;
    private const float chargeDistance = 150f;

    public float rotation = 0f;
    public float handCharge = 0f;
    public float handSling = 0f;
    public float handFollowThrough = 0f;
    private enum HandState
    {
        Default,
        Charging,
        Slinging,
    }

    private HandState handState = HandState.Default;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // The recording mode
        Main.projFrames[Type] = 6;
    }
    public ParticleEmitter emitter;
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.height = Projectile.width = 56;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
        emitter = ParticleSystem.NewEmitter<BeanMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }
    private Vector2 handTarget = Vector2.Zero;

    readonly bool expertMode = Main.expertMode;
    readonly bool masterMode = Main.masterMode;
    float damagescale;
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (expertMode)//is good
        {
            damagescale = 1.5f;
        }
        else if (masterMode)
        {
            damagescale = 2f;
        }
        else
        {
            damagescale = 1;
        }
        Projectile.damage = (int)player.GetDamage(DamageClass.Generic).ApplyTo(50 * damagescale);
        if (!CheckActive(player))
        {
            return;
        }
        float attackSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Generic);
        if (attackSpeedMultiplier >= 3f)
        {
            attackSpeedMultiplier = 3;
        }
        if (HomingTarget != null)
        {
            if (Projectile.ai[0]++ >= 200 - (60 * attackSpeedMultiplier))
            {
                Projectile.ai[0] = 0;
                if (handState == HandState.Default)
                    handState = HandState.Charging;
            }
        }
        HomingTarget ??= Projectile.FindClosestNPC(homingDistance);

        if (HomingTarget != null  && handState != HandState.Default && (player.Distance(HomingTarget.Center) > homingDistance || !HomingTarget.CanBeChasedBy()))
        {
            HomingTarget = null;
        }
        Target();
        if (emitter != null)
            emitter.keptAlive = true;
        if (Main.rand.NextBool(3)/* && handState == HandState.Default*/)
        {
            Vector2 velo = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
            Vector2 veloDelta = Projectile.position - Projectile.oldPosition; // i can't use projectile.velocity here because we're manually changing the position for most of its existence
            Vector2 sideOffset = new Vector2(-16f, 0f) * Projectile.spriteDirection; // so the dust appears visually from the wrists
            emitter?.Emit(Projectile.Center + new Vector2(0f, Projectile.height / 2 - 14) + sideOffset, ((velo * 2f) + veloDelta).RotatedByRandom(0.6f));
        }
    }
    Vector2 toTarget;
    Vector2 chargedPosition;
    public void Target()
    {
        Player player = Main.player[Projectile.owner];
        Vector2 offset = new(-50, -30f + player.gfxOffY);
        offset.X *= player.direction;
        Vector2 normalCenter = player.Center + offset + new Vector2(0f, player.velocity.Y);
        if (HomingTarget != null)
        {
            toTarget = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            chargedPosition = player.Center + offset + new Vector2(0f, player.velocity.Y) - toTarget * chargeDistance;
            if (!HomingTarget.active || HomingTarget.life <= 0)
            {
                HomingTarget = null;
            }
        }
        switch (handState)
        {
            case HandState.Default:
                if (++Projectile.frameCounter >= 5)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = ++Projectile.frame % (Main.projFrames[Type] - 1);
                }
                Projectile.spriteDirection = player.direction;
                handSling = 0f;
                handCharge = 0f;
                handFollowThrough = 0f;
                Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                break;
            case HandState.Charging:

                if (HomingTarget == null)
                {
                    handState = HandState.Default;
                    break;
                }
                Projectile.frame = Main.projFrames[Type] - 1;
                if (handCharge < 0.6f)
                {
                    handCharge += 0.05f;
                }
                else
                {
                    handState = HandState.Slinging;
                    handTarget = HomingTarget.Center + toTarget * 100f;
                }
                Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                break;
            case HandState.Slinging:
                handCharge = 0f;
                if (handSling < 1f)
                {
                    handSling += 0.1f;
                }
                else
                {
                    if (handFollowThrough < 0.2f)
                    {
                        handFollowThrough += 0.05f;
                    }
                    else
                    {
                        handState = HandState.Default;
                        HomingTarget = null;
                    }
                }
                Projectile.Center = Vector2.Lerp(Projectile.Center, handTarget, (float)Math.Sin(handSling * Math.PI));
                break;
        }
    }
    private bool CheckActive(Player owner)
    {
        if (owner.dead || !owner.active)
        {
            owner.GetModPlayer<CosmicHandMinionPlayer>().Active = false;

            return false;
        }

        if (owner.GetModPlayer<CosmicHandMinionPlayer>().Active)
        {
            Projectile.timeLeft = 2;
        }

        return true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ScalingArmorPenetration += 1f;

        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.SlapHand,
            new ParticleOrchestraSettings { PositionInWorld = target.Center },
            Projectile.owner);
    }
    public override bool? CanHitNPC(NPC target)
    {
        return true;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.NPCHit1, target.Center);//Sloppy toppy
        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
        {
            PositionInWorld = target.Center,
        }, target.whoAmI);

    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 10; i++)
        {
            emitter?.Emit(Projectile.Center, (-Projectile.velocity).RotatedByRandom(3f));
        }
        SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        void DrawAtProj(Texture2D tex)
        {
            sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        if (handState == HandState.Slinging || handState == HandState.Charging)
        {
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 center = Projectile.Size / 2f;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + center;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Vector2 origin = new(outline.Width * 0.5f, outline.Height / Main.projFrames[Type] * 0.5f);
                sb.Draw(outline, drawPos, frame, color, Projectile.oldRot[k], origin, Projectile.scale, SpriteEffects.None, 0f);
            }
        }
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtProj(texture));
        return false;
    }
}
