using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.CosJel;
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
        NPC.boss = true;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

        NPC.damage = (int)(NPC.damage * 0.7f);
    }
    private enum ActionState
    {
        Die = -3,
        ToP2 = -2,
        Split = -1,
        Spawning,
        Jump,
        Fireburst,
        SingNote,
        Stomp,
        ToTarget,
    }
    bool expertMode = Main.expertMode;
    bool masterMode = Main.masterMode;
    bool legendaryMode = (Main.getGoodWorld || Main.zenithWorld) && Main.masterMode;
    private ActionState AI_State;
    public ref float aiTimer0 => ref NPC.localAI[0];
    public ref float aiTimer1 => ref NPC.localAI[1];
    public ref float aiTimer2 => ref NPC.localAI[2];
    public ref float Phase => ref NPC.localAI[3];
    public float attackCount = 0;
    public int maxWispCount = 10;
    //.ai[0] is owner
    //.ai[1] is face anim
    //.ai[2] is grand wisp counter
    public float GrandWispsLost
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    public float CurrentAttack
    {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
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
        maxWispCount = legendaryMode ? 25 : (masterMode && !legendaryMode) ? 20 : expertMode ? 15 : 10;
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
        if (GrandWispsLost >= maxWispCount)
        {
            CheckDead();
        }
        speedMult = 1f + (GrandWispsLost * 0.025f);
        scaleMult = Math.Max(0.5f, scaleMultMax - (GrandWispsLost * (0.75f / maxWispCount)));

        pulseScale = BetterEssScale(4, 0.25f);
        Player player = Main.player[NPC.target];
        if (emitter != null)
            emitter.keptAlive = true;
        switch (AI_State)
        {
            case ActionState.Die:
                WispPop(player);
                break;
            case ActionState.ToP2:
                WispUnleashed(player, Candle);
                break;
            case ActionState.Spawning:
                if (P2Check())
                    break;
                Spawning(Candle);
                break;
            case ActionState.Jump:
                if (P2Check())
                    break;
                SpiritJump(player, Candle);
                break;
            case ActionState.Fireburst:
                if (P2Check())
                    break;
                FireRain(player, Candle);
                break;
            case ActionState.SingNote:
                if (P2Check())
                    break;
                SpiritSong(player, Candle);
                break;
            case ActionState.Stomp:
                if (P2Check())
                    break;
                SpiritStomp(player, Candle);
                break;
            case ActionState.ToTarget:
                if (P2Check())
                    break;
                TargetJump(player, Candle);
                break;
            case ActionState.Split:
                if (P2Check())
                    break;
                GrandWispDance(player, Candle);
                break;
        }
        if (AI_State != ActionState.Split)
        {
            if (Main.rand.NextBool(Math.Max((int)(1 / scaleMult),1)))
            {
                emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2) * NPC.scale, -Vector2.UnitY * (Main.rand.NextFloat(3, 5) * (NPC.scale + 1)) * scaleMult, 0f, 90);
            }
        }
    }
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
    float scaleMultMax = 1.5f;
    float pulseScale = 0;
    public void Spawning(NPC candle)
    {
        float morphTime = 60;
        float progress = Utils.Clamp(aiTimer0 / morphTime, 0f, 1f);
        NPC.scale = MathHelper.Lerp(0f, scaleMult, progress);
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - ((NPC.height / 1.5f)) * NPC.scale), 0.1f);

        aiTimer0++;
        if (aiTimer0 >= morphTime)
        {
            aiTimer0 = 0;
            AI_State = ActionState.SingNote;
        }
    }
    float maxDist = 600;//if farther than this, force jump
    public void SpiritJump(Player player,NPC candle)
    {
        float maxAttack = 2;
        NPC.Center = Vector2.Lerp(NPC.Center,new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * (pulseScale)),0.1f);
        NPC.velocity = Vector2.Zero;
        NPC.scale = scaleMult;
        aiTimer0++;

        if (aiTimer0 >= 120 / speedMult)
        {
            if (attackCount++ > maxAttack)
            {
                ResetStats();
                GetNextAttack(player, ActionState.Fireburst);
                aiTimer0 = 0;
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
        float splitTime = 120;
        if (aiTimer0 <= 0)
        {
            NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 2) * scaleMult), 0.2f);
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
                        Projectile breath = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 40) * scaleMult, (-Vector2.UnitY * Main.rand.NextFloat(6, 8)).RotatedByRandom(MathHelper.ToRadians(20)), ModContent.ProjectileType<WispFireRain>(), ProjectileDamage(NPC.damage), 0, -1);
                    }
                }
                else if (aiTimer0 > splitTime * 2)
                {
                    NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height/2) * scaleMult - 20 * scaleMult), 0.1f);
                    aiTimer0 = -90;//let fire decay
                    aiTimer1++;
                    NPC.ai[1] = 0;
                }
                else
                {
                    NPC.ai[1] = 1;
                    NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height/2) * scaleMult - Math.Max(aiTimer0, 20 * scaleMult)), 0.1f);
                }
                break;
            case 1://precise droplets
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - ((NPC.height) * scaleMult) * pulseScale), 0.1f);
                if (aiTimer0 % (120 / speedMult) == 0)
                {
                    SpawnRain(dropPos, Vector2.UnitY, ProjectileDamage(NPC.damage));
                    attackCount++;
                }
                if (attackCount >= 2)
                {
                    attackCount = 0;
                    aiTimer0 = -50;
                    aiTimer1++;
                }
                break;
            case 2://fast droplets
                float pulseFast = BetterEssScale(6, 0.5f);
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - ((NPC.height) * scaleMult) * pulseFast), 0.1f);

                if (aiTimer0 % ((120) - Math.Clamp(attackCount * 15,0,105)) == 0)
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
                float pulseSlow = BetterEssScale(1, 0.5f);
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - ((NPC.height) * scaleMult) * pulseSlow), 0.1f);
                if (aiTimer0 % ((15) + Math.Clamp(attackCount * 10, 0, 90)) == 0)
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
                    GetNextAttack(player, ActionState.SingNote);
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
    public void SpiritSong(Player player, NPC candle)
    {

        NPC.ai[1] = 0; // Reset Face
        ResetStats();
        GetNextAttack(player, ActionState.Stomp); // Chain to next attack

    }
    public void SpiritStomp(Player player, NPC candle)
    {
        float maxAttack = 1;
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * scaleMult * (pulseScale)), 0.1f);
        NPC.velocity = Vector2.Zero;
        NPC.scale = scaleMult;
        aiTimer0++;

        if (aiTimer0 >= 180 / speedMult)
        {
            if (attackCount++ > maxAttack)
            {
                ResetStats();
                GetNextAttack(player, ActionState.Jump);
            }
            else
            {
                candle.ai[0] = 2;//spawn wisps
                candle.ai[1] = 1;//activate jump
                candle.ai[2] = player.whoAmI;
                aiTimer0 = 0;
            }
            NPC.netUpdate = true;
        }
    }
    public void TargetJump(Player player, NPC candle)
    {
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * scaleMult * (pulseScale)), 0.1f);
        NPC.scale = scaleMult;
        aiTimer0++;

        if (aiTimer0 >= 60 / speedMult)
        {
            candle.ai[0] = 0;
            candle.ai[1] = 1;
            candle.ai[2] = player.whoAmI;
        }
        if (aiTimer0 >= 120 / speedMult)
        {
            ResetStats();

            ActionState nextState = (ActionState)CurrentAttack;
            if (nextState == 0)
                nextState = ActionState.Jump;

            AI_State = nextState;
            CurrentAttack = 0;

            NPC.netUpdate = true;
        }
    }
    bool anyWispsAlive = false;
    public void GrandWispDance(Player player, NPC candle)
    {
        float danceTime = 300 / speedMult;
        aiTimer0++;
        NPC.scale = scaleMult;
        if (aiTimer0 < 120)
        {
            NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * scaleMult), 0.1f);
            NPC.Center += Main.rand.NextVector2Circular(2, 2);
            NPC.ai[1] = 3;
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.Opacity = 0;
            NPC.ShowNameOnHover = false;
            NPC.velocity = Vector2.Zero;
            NPC.Center = candle.Center + new Vector2(0, -50);
            if (aiTimer1 == 0)
            {
                candle.ai[1] = 2;
                candle.ai[2] = player.whoAmI;
                GrandWispsLost = Math.Max(GrandWispsLost, 5);

                int CurrentPool = maxWispCount - (int)GrandWispsLost;
                int countToSpawn = Math.Max(5, CurrentPool);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < countToSpawn; i++)
                    {
                        int wispID = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GrandWisp>(), 0, NPC.whoAmI);

                        if (Main.npc[wispID].active)
                        {
                            Main.npc[wispID].velocity = (Vector2.UnitY * Main.rand.NextFloat(4, 7)).RotatedByRandom(MathHelper.ToRadians(360));
                            Main.npc[wispID].netUpdate = true;
                        }
                    }
                }
            }
            aiTimer1++;
            bool forceReform = aiTimer1 >= danceTime;
            anyWispsAlive = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<GrandWisp>() && (int)Main.npc[i].ai[0] == NPC.whoAmI)
                {
                    anyWispsAlive = true;
                    if (forceReform && Main.npc[i].ai[1] == 0)
                    {
                        Main.npc[i].ai[1] = 1;
                        Main.npc[i].netUpdate = true;
                    }
                }
            }
            if (!anyWispsAlive)
            {
                if (forceReform)
                {
                    for (int i = 0; i <= 18; i++)
                    {
                        emitter?.Emit(NPC.Center, (-Vector2.UnitY * Main.rand.NextFloat(7, 10)).RotatedByRandom(MathHelper.ToRadians(360)), 0f, 90);
                    }
                    NPC.Opacity = 1;
                    NPC.dontTakeDamage = false;
                    NPC.ShowNameOnHover = true;
                    NPC.life = NPC.lifeMax;
                    GrandWispsLost = Math.Min(GrandWispsLost, maxWispCount - 5);
                    NPC.ai[1] = 0;
                    candle.ai[1] = 0;
                    candle.netUpdate = true;

                    ResetStats();
                    AI_State = ActionState.Jump;
                    NPC.netUpdate = true;
                }
            }
        }
    }

    public void ResetStats()
    {
        aiTimer0 = 0;
        aiTimer1 = 0;
        attackCount = 0;
    }

    private void GetNextAttack(Player player, ActionState nextState)
    {
        float distance = Vector2.Distance(player.Center, NPC.Center);

        if (distance > maxDist)
        {
            AI_State = ActionState.ToTarget;
            CurrentAttack = (float)nextState;
        }
        else
        {
            AI_State = nextState;
        }

        ResetStats();
        NPC.netUpdate = true;
    }
    public void WispUnleashed(Player player, NPC candle)
    {
        float morphTime = 120;
        NPC.dontTakeDamage = true;
        if (NPC.buffType[0] != 0)
            NPC.DelBuff(0);
        if (++aiTimer0 > morphTime)
        {
            scaleMultMax = 2f;
            Phase = 1;
            candle.hide = true;
            NPC.Center = player.Center;
            NPC.dontTakeDamage = false;
        }
        else if (aiTimer0 == morphTime)
        {
            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
        }
        else
        {
            NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * scaleMult), 0.1f);
            NPC.velocity = Vector2.Zero;
            NPC.scale = scaleMult;
            scaleMultMax = MathHelper.Lerp(1.5f, 2f, 0.02f);
        }
    }

    public void WispPop(Player player)
    {
        float morphTime = 300;
        NPC.dontTakeDamage = true;
        if (NPC.buffType[0] != 0)
            NPC.DelBuff(0);
        if (++aiTimer0 > morphTime)
        {
            for (int i = 0; i <= 30; i++)
            {
                emitter?.Emit(NPC.Center, (-Vector2.UnitY * Main.rand.NextFloat(7, 10)).RotatedByRandom(MathHelper.ToRadians(360)), 0f, 90);
            }
            NPC.life = 0;
            NPC.checkDead();
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.Opacity = 1f;
            NPC.life = 1;
            NPC.velocity = Vector2.Zero;
            NPC.scale = scaleMult;
            scaleMultMax = MathHelper.Min(scaleMultMax + 0.01f, 4);
        }
    }

    public bool P2Check()
    {
        if (Phase > 0)
            return false;
        if (AI_State == ActionState.Split)
            return false;
        if (Phase <= 0)
        {
            if (GrandWispsLost >= maxWispCount - 5)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AI_State = ActionState.ToP2;
                    ResetStats();
                    NPC.netUpdate = true;
                }
                return true;
            }
        }
        return false;
    }

    public override bool CheckDead()
    {
        if (Phase <= 0)
        {
            if (AI_State != ActionState.Split)
            {
                if (GrandWispsLost < maxWispCount)
                {
                    NPC.dontTakeDamage = true;
                    NPC.Opacity = 1f;
                    NPC.life = 1;
                    AI_State = ActionState.Split;
                    NPC.ai[1] = 3;
                    ResetStats();
                    NPC.netUpdate = true;
                    return false;
                }
            }
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.Opacity = 1f;
            NPC.life = 1;
            if (AI_State != ActionState.Die)
            {
                ResetStats();
                AI_State = ActionState.Die;
            }
            if (AI_State == ActionState.Die && aiTimer0 >= 300)
                return true;
        }
        if (GrandWispsLost >= maxWispCount)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath10, NPC.Center);
            if (emitter != null)
                emitter.keptAlive = false;
            return true;
        }
        return false;
    }
    int faceFrameTotal = 7;
    int faceFrameCurrent = 0;
    const int faceStartPop = 4;
    const int faceEndPop = 7;
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
                        if (faceFrameCurrent >= 4)
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
                faceFrameCurrent = 2;
                break;
            case 3://pop
                if (faceFrameCurrent < faceStartPop)
                {
                    faceFrameCurrent = faceStartPop;
                }
                if (faceFrameCurrent < faceFrameTotal)
                {
                    if (NPC.frameCounter++ >= 10)
                    {
                        faceFrameCurrent++;
                        NPC.frameCounter = 0;
                    }
                }
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
        void DrawAtNPC(Texture2D tex,float scale)
        {
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, frame, Color.White * NPC.Opacity, NPC.rotation, 
                new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), 
                scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline,NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => 
        Main.EntitySpriteDraw(face, NPC.Center + new Vector2(0,20 * NPC.scale) - Main.screenPosition, frameFace,
        Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() /2, NPC.scale, SpriteEffects.None));

        if (anyWispsAlive)
        {
            float specialScale = 1;
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline, NPC.scale));
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture, NPC.scale));
        }

        return false;
    }
}