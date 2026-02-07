using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Misc;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.NPCs.Bosses;

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
    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public override void AI()
    {
        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
        {
            NPC.TargetClosest();
        }
        Player player = Main.player[NPC.target];
        NPC.Center = Vector2.Lerp(NPC.Center, player.Center + new Vector2(0,-200 * Main.essScale), 0.1f);
        if (emitter != null)
            emitter.keptAlive = true;

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
            emitter?.Emit(Main.rand.NextVector2FromRectangle(NPC.Hitbox), -Vector2.UnitY * Main.rand.NextFloat(4, 6), 0f, 60);
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