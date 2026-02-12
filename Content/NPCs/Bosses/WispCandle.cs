using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

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

    private int wispID = -1;
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
        NPC.noTileCollide = false;
        NPC.knockBackResist = 0f;
        NPC.dontTakeDamage = true;
        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
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
    float gravity = 0.4f;
    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;
        NPC Wisp = MiscHelpers.NPCExists(wispID, ModContent.NPCType<MotherWisp>());
        if (hasWisp && Wisp == null)
        {
            NPC.timeLeft = 0;
            NPC.active = false;
            return;
        }
        switch (AI_State)
        {
            case ActionState.Spawning:
                float spawnTime = 240;
                if (Main.rand.NextBool(1))
                {
                    emitter?.Emit(NPC.Center + new Vector2(0, 20), (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(30)), 0f, 20);
                }
                if (AITimer++ >= spawnTime)
                {
                    if (wispID == -1 || !Main.npc[wispID].active || Main.npc[wispID].type != ModContent.NPCType<MotherWisp>())
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ModContent.NPCType<MotherWisp>();
                            wispID = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Bottom.Y, type, 0, NPC.whoAmI);
                            AITimer = 0;
                            AI_State = ActionState.Controlled;
                            hasWisp = true;
                        }
                    }
                }
                NPC.velocity.Y += gravity;
                break;
            case ActionState.Controlled:
                //trojan sq jump code
                if (Main.rand.NextBool(1))
                {
                    emitter?.Emit(NPC.Center + new Vector2(0, 20), (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(120)), 0f, 20);
                }

                if (NPC.ai[1] == 1)
                {
                    ExecuteJump(Main.player[(int)(NPC.ai[2])], NPC.ai[0]);
                }
                else
                {
                    NPC.velocity.X *= 0.8f;
                    if (Math.Abs(NPC.velocity.X) < 0.1f) NPC.velocity.X = 0;
                    NPC.velocity.Y += 0.4f;
                }
        
                break;
            case ActionState.Detaching:

                break;
        }
    }
    float time = 0; 
    private void ExecuteJump(Player player, float attackID)
    {
        NPC Wisp = MiscHelpers.NPCExists(wispID, ModContent.NPCType<MotherWisp>());
        if (hasWisp && Wisp == null)
        {
            NPC.timeLeft = 0;
            NPC.active = false;
            return;
        }
        if (NPC.localAI[0] == 0)
        {
            Vector2 distance = player.Top - NPC.Bottom;
            switch (attackID)
            {
                case 0:
                    distance = player.Top - NPC.Bottom;
                    break;
                case 1:
                    time = 90f - 30f * (1 - (Wisp.life / Wisp.lifeMax));
                    distance = player.Top - NPC.Bottom;
                    break;
                case 2:
                    time = 60f - 30f * (1 - (Wisp.life / Wisp.lifeMax));
                    distance = Wisp.Bottom - NPC.Bottom;
                    break;
            }
            for (int i = 0; i < 4; i++)
            {
                int side = i % 2 == 0 ? 1 : -1;
                float speed = Main.rand.NextFloat(4, 6);
                Vector2 vel = (Vector2.UnitX * side * speed).RotatedByRandom(MathHelper.Pi / 11);
                emitter?.Emit(NPC.Bottom, vel, 0f, 20);
            }
            distance.X /= time;
            distance.Y = distance.Y / time - 0.5f * gravity * time;
            NPC.noTileCollide = true;
            NPC.velocity = distance;
            NPC.netUpdate = true;
        }
        else
        {
            NPC.noTileCollide = true;
            NPC.velocity.Y += gravity;
        }

        NPC.localAI[0]++;
        if (NPC.localAI[0] > time)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                switch (attackID)
                {
                    case 0:
                        break;
                    case 1:
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Projectile shockwave = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom + new Vector2(30 * j, +40), new Vector2(1 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), ProjectileDamage(NPC.damage), 0, -1);
                            shockwave.spriteDirection = j;
                        }
                        break;
                    case 2:
                        for (int j = -1; j <= 1; j += 2)
                        {
                            NPC wisp = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Bottom.Y + 50, NPCID.DungeonSpirit, 0, NPC.whoAmI);
                            wisp.velocity = (Vector2.UnitX * j * 4f).RotatedByRandom(MathHelper.ToRadians(30));
                        }
                        break;
                }
            }
            player.GetITDPlayer().BetterScreenshake(10, 10, 10, true);
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            NPC.ai[1] = 0f;
            NPC.localAI[0] = 0f;
            NPC.netUpdate = true;
            NPC.noTileCollide = false;
        }
    }
    
    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }
}