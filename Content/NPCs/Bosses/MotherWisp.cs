using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;

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
        NPC.damage = 0;
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
        WispStomp
    }
    private ActionState AI_State;
    public float aiTimer0 = 0;
    public float aiTimer1 = 0;

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

        Player player = Main.player[NPC.target];
        if (emitter != null)
            emitter.keptAlive = true;
        switch (AI_State)
        {
            case ActionState.Spawning:
                float morphTime = 60;
                NPC.scale = MathHelper.Lerp(NPC.scale,scaleMult,scaleMult/morphTime);
                aiTimer0++;
                if (aiTimer0 >= morphTime)
                { 
                    aiTimer0 = 0;
                    AI_State = ActionState.Jump;
                }
                break;
            case ActionState.Jump:
                NPC.scale = scaleMult;
                SpiritJump(player, Candle);
                break;
        }
        if (NPC.frameCounter++ >= 6)
        {
            faceFrameCurrent++;
            NPC.frameCounter = 0;
            if (faceFrameCurrent >= faceFrameTotal)
            {
                faceFrameCurrent = 0;
            }
        }

        if (Main.rand.NextBool(1))
        {
            emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(NPC.width/2,NPC.height/2) * NPC.scale, -Vector2.UnitY * Main.rand.NextFloat(4, 6), 0f, 60);
        }
    }
    float speedMult = 1;
    float scaleMult = 2;
    public void SpiritJump(Player player,NPC candle)
    {
        NPC.Center = Vector2.Lerp(NPC.Center,new Vector2(candle.Center.X, candle.Top.Y - (NPC.height / 4) * Main.essScale),0.1f);
        NPC.velocity = Vector2.Zero;

        aiTimer0++;

        if (aiTimer0 >= 200 / speedMult)
        {
            candle.ai[1] = 1;
            candle.ai[2] = player.whoAmI;
            aiTimer0 = 0;
            NPC.netUpdate = true;
        }
    }
    int faceFrameTotal = 4;
    int faceFrameCurrent = 0;
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
            sb.Draw(tex, NPC.Center - Main.screenPosition, frame, Color.White, NPC.rotation, 
                new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), 
                NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => 
        Main.EntitySpriteDraw(face, NPC.Center - Main.screenPosition, frameFace,
        Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() /2, NPC.scale, SpriteEffects.None));

        return false;
    }
}