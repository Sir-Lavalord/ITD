using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
namespace ITD.Content.NPCs.Bosses;

public class GrandWisp : ModNPC
{
    public ParticleEmitter emitter;
    public override string Texture => "ITD/Content/NPCs/Bosses/MotherWisp";
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }
    public int OwnerIndex => (int)NPC.ai[0];
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance * bossAdjustment);

        NPC.damage = (int)(NPC.damage * 0.7f);
    }
    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale = 0.5f;
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null)
        {
            Mom.timeLeft = 0;
            Mom.active = false;
            return;
        }
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public override void AI()
    {
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null)
        {
            Mom.timeLeft = 0;
            Mom.active = false;
            return;
        }
        if (emitter != null)
            emitter.keptAlive = true;
        if (Main.rand.NextBool(4))
        {
            emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2) * NPC.scale, -NPC.velocity, 0f, 90);
        }
        if (NPC.ai[1] == 0)
        {
            if (NPC.localAI[0]++ >= 30)
            {
                NPC.TargetClosest(false);
                if (Main.rand.NextBool(20))
                {
                    Vector2 randomVel = Main.rand.NextVector2Circular(8f, 8f);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, randomVel, 0.5f);
                }
                if (Mom.active)
                {
                    float dist = Vector2.Distance(NPC.Center, Mom.Center);
                    if (dist > 1000f)
                    {
                        Vector2 backDir = NPC.DirectionTo(Mom.Center);
                        NPC.velocity += backDir * 0.2f;
                    }
                }
                NPC.velocity *= 0.98f;
            }
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.damage = 0;


            if (NPC.ai[2]++ == 0)
            {
                NPC.velocity = (Mom.Center - NPC.Center) * 2f / 90f;
                NPC.localAI[1] = NPC.velocity.Length() / 90f;
            }
            else
            {
                NPC.velocity = Vector2.Normalize(NPC.velocity) * (NPC.velocity.Length() - NPC.localAI[1]);
            }

            if (NPC.Distance(Mom.Center) < 10f)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

        }
    }

    public override void OnKill()
    {
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null)
        {
            Mom.timeLeft = 0;
            Mom.active = false;
            return;
        }
        if (NPC.ai[1] == 0)
        {
            Mom.ai[2] += 1;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, OwnerIndex);
        }
    }
    int faceFrameTotal = 7;
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
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, frame, Color.White, NPC.rotation,
                new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f),
                NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () =>
        Main.EntitySpriteDraw(face, NPC.Center + new Vector2(0, 20 * NPC.scale) - Main.screenPosition, frameFace,
        Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() / 2, NPC.scale, SpriteEffects.None));

        return false;
    }
}
