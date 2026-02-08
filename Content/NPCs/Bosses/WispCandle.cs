using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Particles;
using ITD.Particles.Misc;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.NPCs.Bosses;

public class WispCandle : ModNPC
{
    private enum ActionState
    {
        Spawning,
        Controlled,
        Detaching //it's bye bye baby
    }
    private ActionState AI_State;
    public override void SetStaticDefaults()
    {
    }
    public ParticleEmitter emitter;
    private float AITimer = 0;
    public bool hasWisp = false;
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
        NPC.noTileCollide = false;
        NPC.knockBackResist = 0f;
        NPC.dontTakeDamage = true;
        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = 1000;
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
    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public override void AI()
    {
        switch (AI_State)
        {
            case ActionState.Spawning:
                if (AITimer++ >= 300)
                {
                    int type = ModContent.NPCType<MotherWisp>();
                    NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, type, 0, NPC.whoAmI);
                    AITimer = 0;
                    AI_State = ActionState.Controlled;
                }
                break;
            case ActionState.Controlled:
                //trojan champ jump code
                if (NPC.ai[0] == 0)
                {
                    if (NPC.ai[1] == 1)
                    {
                        ExecuteJump(Main.player[(int)(NPC.ai[2])]);
                    }
                    else
                    {
                        NPC.velocity.X *= 0.8f;
                        if (Math.Abs(NPC.velocity.X) < 0.1f) NPC.velocity.X = 0;
                        NPC.velocity.Y += 0.4f;
                    }
                }
                break;
            case ActionState.Detaching:

                break;
        }
    }
    private void ExecuteJump(Player player)
    {
        const float gravity = 0.4f;
        float time = 60f;
        if (NPC.localAI[0] == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                int side = i % 2 == 0 ? 1 : -1;
                float speed = Main.rand.NextFloat(4, 6);
                Vector2 vel = (Vector2.UnitX * side * speed).RotatedByRandom(MathHelper.Pi / 11);
                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Bottom - Vector2.UnitY * 10, vel, Main.rand.Next(11, 14), Scale: 2f);
            }
            Vector2 distance = player.Top - NPC.Bottom;
            distance.X /= time;
            distance.Y = distance.Y / time - 0.5f * gravity * time;

            NPC.velocity = distance;
            NPC.netUpdate = true;
        }
        else
        {
            NPC.velocity.Y += gravity;
        }

        NPC.localAI[0]++;
        if (NPC.localAI[0] > time)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Projectile shockwave = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom + new Vector2(30 * j, +40), new Vector2(1 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), ProjectileDamage(NPC.damage), 0, -1);
                    shockwave.spriteDirection = j;
                }
            }
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            NPC.ai[1] = 0f;
            NPC.localAI[0] = 0f;
            NPC.netUpdate = true;
        }
    }
    
    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }
}