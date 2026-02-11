using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using static ITD.Utilities.MiscHelpers;
namespace ITD.Content.NPCs.Bosses;

[AutoloadBossHead]
public class MotherWisp : ModNPC
{

    public override void SetStaticDefaults()
    {
    }
    public ParticleEmitter emitter;
    public override void SetDefaults()
    {
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = 1000;
    }
    private enum ActionState
    {
        Spawning,
        Jump,
        Fireburst,
        SingNote,
        WispStomp,
        ToTarget
    }
    private ActionState AI_State;
    public float aiTimer0 = 0;
    public float aiTimer1 = 0;
    public float aiTimer2 = 0;
    public float attackCount = 0;
    //.ai[0] is owner
    //.ai[1] is face anim
    //.ai[2] is grand wisp counter
    public float GrandWispsLost
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }


    public override void OnSpawn(IEntitySource source)
    {
        NPC Candle = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<WispCandle>());
        if (Candle == null)
        {
            Candle.timeLeft = 0;
            Candle.active = false;
            return;
        }
        Candle.netUpdate = true;
        NPC.scale = 0f;
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public int OwnerIndex => (int)NPC.ai[0];
    public override void AI()
    {
        NPC Candle = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<WispCandle>());
        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
        {
            NPC.TargetClosest();
        }
        speedMult = 1f + (GrandWispsLost * 0.2f);
        scaleMult = 1.25f - (GrandWispsLost * 0.1f);
        pulseScale = BetterEssScale(4, 1.5f);

        Player player = Main.player[NPC.target];
        if (emitter != null)
            emitter.keptAlive = true;
        switch (AI_State)
        {
            case ActionState.Spawning:
                Spawning(Candle);
                break;
            case ActionState.Jump:
                SpiritJump(player, Candle);
                break;
            case ActionState.Fireburst:
                FireRain(player, Candle);
                break;
            case ActionState.ToTarget:
                TargetJump(player, Candle);
                break;
        }

        if (Main.rand.NextBool(1))
        {
            emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(NPC.width/2,NPC.height/2) * NPC.scale, -Vector2.UnitY * Main.rand.NextFloat(3, 5) * (NPC.scale + 1), 0f, 90);
        }
    }
    bool expertMode = Main.expertMode;
    bool masterMode = Main.masterMode;
    public int ProjectileDamage(int damage)
    {
        if (expertMode)
            return (int)(damage / 2.5f);
        if (masterMode)
            return (int)(damage / 3.5f);
        return (int)(damage / 1);
    }
    float speedMult = 1;
    float scaleMult = 2;
    float pulseScale = 0;
    public void Spawning(NPC candle)
    {
        float morphTime = 60;
        float progress = Utils.Clamp(aiTimer0 / morphTime, 0f, 1f);
        NPC.scale = MathHelper.Lerp(0f, scaleMult, progress);
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Center.Y - ((candle.Center.Y - candle.Top.Y)) * NPC.scale), 0.1f);

        aiTimer0++;
        if (aiTimer0 >= morphTime)
        {
            aiTimer0 = 0;
            AI_State = ActionState.Jump;
        }
    }
    float maxDist = 600;//if farther than this, force jump
    public void SpiritJump(Player player,NPC candle)
    {
        float maxAttack = 2;
        NPC.Center = Vector2.Lerp(NPC.Center,new Vector2(candle.Center.X, candle.Top.Y - (NPC.height/8) * (pulseScale)),0.1f);
        NPC.velocity = Vector2.Zero;
        NPC.scale = scaleMult;
        aiTimer0++;

        if (aiTimer0 >= 180 / speedMult)
        {
            if (attackCount++ > maxAttack)
            {
                ResetStats();
                AI_State = ActionState.Fireburst;
            }
            else
            {
                candle.ai[0] = 1;//spawn shockwave
                candle.ai[1] = 1;//activate jump
                candle.ai[2] = player.whoAmI;
                aiTimer0 = 0;
            }
            NPC.netUpdate = true;
        }
    }

    public void FireRain(Player player, NPC candle)
    {
        float splitTime = 180;
        if (aiTimer0 == 0)
        {
            NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 8)), 0.5f);
        }
        
        NPC.velocity = Vector2.Zero;
        NPC.scale = scaleMult;
        aiTimer0++;
        Vector2 dropPos = player.Center + new Vector2(0, -800);
        int dropRange = 800;
        switch (aiTimer1)
        {
            case 0://breath

                if (aiTimer0 >= splitTime / speedMult && aiTimer0 <= splitTime * 2)
                {
                    NPC.ai[1] = 2;
                    if (Main.rand.NextBool(2))
                    {
                        Projectile breath = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 40), (-Vector2.UnitY * Main.rand.NextFloat(6, 8)).RotatedByRandom(MathHelper.ToRadians(20)), ModContent.ProjectileType<WispFireRain>(), ProjectileDamage(NPC.damage), 0, -1);
                    }
                }
                else if (aiTimer0 > splitTime * 2)
                {
                    NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 8) - 40), 0.1f);
                    aiTimer0 = -90;//let fire decay
                    aiTimer1++;
                }
                else
                {
                    NPC.ai[1] = 1;
                    NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 8) - Math.Min(aiTimer0, 40)), 0.1f);
                }
                break;
            case 1://precise droplets
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - ((NPC.height / 8)) * pulseScale), 0.1f);
                if (aiTimer0 % (120 / speedMult) == 0)
                {
                    SpawnRain(dropPos, Vector2.UnitY, ProjectileDamage(NPC.damage));
                    attackCount++;
                }
                if (attackCount >= 3)
                {
                    attackCount = 0;
                    aiTimer0 = -50;
                    aiTimer1++;
                }
                break;
            case 2://fast droplets
                if (aiTimer0 % ((120) - Math.Clamp(attackCount * 10,0,100)) == 0)
                {
                    attackCount++;
                    if (attackCount % 6 == 0)
                    {
                        dropPos = player.Center + new Vector2(0, -800);
                    }
                    else
                    {
                        dropPos = player.Center + new Vector2(Main.rand.Next(-dropRange, dropRange), -800);
                    }
                    SpawnRain(dropPos, Vector2.UnitY, ProjectileDamage(NPC.damage));
                }
                if (attackCount >= 40)
                {
                    attackCount = 0;
                    aiTimer0 = -50;
                    aiTimer1++;
                }
                break;
            case 3://slows down 
                if (aiTimer0 % ((20) + Math.Clamp(attackCount * 10, 0, 90)) == 0)
                {
                    dropRange = 800 - (int)(attackCount * 30);
                    attackCount++;
                    if (attackCount % 4 == 0)
                    {
                        dropPos = player.Center + new Vector2(0, -800);
                    }
                    else
                    {

                        dropPos = player.Center + new Vector2(Main.rand.Next(-dropRange, dropRange), -800);
                    }
                    SpawnRain(dropPos, Vector2.UnitY, ProjectileDamage(NPC.damage));
                }
                if (attackCount >= 10)
                {
                    NPC.ai[1] = 0;
                    attackCount = 0;
                    aiTimer0 = -50;
                    aiTimer1 = 0;
                    AI_State = ActionState.Jump;
                }
            break;
        }
        void SpawnRain(Vector2 pos, Vector2 vel, int damage)
        {
            RaycastData data = Helpers.QuickRaycast(pos, vel, (point) => { return (player.Bottom.Y >= point.ToWorldCoordinates().Y + 20); }, 120);
            Projectile rain = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                pos, vel,
                ModContent.ProjectileType<WispTelegraph>(), damage, 0, -1, 0, data.End.Y);
        }
    }

    public void TargetJump(Player player, NPC candle)
    {
        float maxAttack = 1;
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 8) * (pulseScale)), 0.1f);
        NPC.velocity = Vector2.Zero;
        NPC.scale = scaleMult;
        aiTimer0++;

        if (aiTimer0 >= 60 / speedMult)
        {
            if (attackCount++ > maxAttack)
            {
                ResetStats();
                AI_State = ActionState.Fireburst;
            }
            candle.ai[0] = 0;//spawn shockwave
            candle.ai[1] = 1;//activate jump
            candle.ai[2] = player.whoAmI;//target
            aiTimer0 = 0;

            NPC.netUpdate = true;
        }
    }
    
    public void ResetStats()
    {
        aiTimer0 = 0;
        aiTimer1 = 0;
        attackCount = 0;
    }

    int faceFrameTotal = 4;
    int faceFrameCurrent = 0;

    public override void FindFrame(int frameHeight)//need anim
    {
        switch ((int)NPC.ai[1])
        {
            case 0://default face
                {
                    if (NPC.frameCounter++ >= 8)
                    {
                        faceFrameCurrent++;
                        NPC.frameCounter = 0;
                        if (faceFrameCurrent >= faceFrameTotal)
                        {
                            faceFrameCurrent = 0;
                        }
                    }
                }
                break;
            case 1://fireburst start
                faceFrameCurrent = 0;
                break;
            case 2://fireburst breath face start
                faceFrameCurrent = 1;
                break;
            default:
                goto case 0;
        }
        base.FindFrame(frameHeight);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 position = NPC.Center - Main.screenPosition;

        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
        Texture2D face = ModContent.Request<Texture2D>(Texture + "_Face").Value;
        Rectangle frame = texture.Frame(1, 1, 0, 0);
        Rectangle frameFace = face.Frame(1, faceFrameTotal, 0, faceFrameCurrent);
        void DrawAtNPC(Texture2D tex)
        {
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, frame, Color.White, NPC.rotation, 
                new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), 
                NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => 
        Main.EntitySpriteDraw(face, NPC.Center + new Vector2(0,20 * NPC.scale) - Main.screenPosition, frameFace,
        Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() /2, NPC.scale, SpriteEffects.None));

        return false;
    }
}