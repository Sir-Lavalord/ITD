using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace ITD.Content.NPCs.Bosses;

public class WispCandle : ModNPC
{
    private enum ActionState
    {
        Spawning,
        Controlled,
        Detaching,
        Death,//TCD candle death
    }
    private ActionState AI_State;
    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        Main.npcFrameCount[NPC.type] = 1;
    }
    public ParticleEmitter emitter;
    private float AITimer = 0;
    public bool hasWisp = false;

    private int wispID = -1;
    public override void SetDefaults()
    {
        NPC.width = 26;
        NPC.height = 24;
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
        NPC.boss = true;
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
    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
    {
        return AI_State != ActionState.Detaching;
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
                    emitter?.Emit(NPC.Top - new Vector2(0, 10), (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(30)), 0f, 20);
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
                    emitter?.Emit(NPC.Top - new Vector2(0,10), (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(120)), 0f, 20);
                }

                if (NPC.ai[1] == 1)
                {
                    ExecuteJump(Main.player[(int)(NPC.ai[2])], NPC.ai[0]);
                }
                else if (NPC.ai[1] == 2)
                {
                    NPC.localAI[2] = Main.rand.Next(0, 2);
                    NPC.localAI[0] = 0;
                    AITimer = 0;
                    AI_State = ActionState.Detaching;
                }
                else
                {
                    NPC.velocity.X *= 0.8f;
                    if (Math.Abs(NPC.velocity.X) < 0.1f) NPC.velocity.X = 0;
                    NPC.velocity.Y += 0.4f;
                }
                break;
            case ActionState.Detaching:
                if (Main.rand.NextBool(3))
                {
                    emitter?.Emit(NPC.Top - new Vector2(0, 10), (-Vector2.UnitY * Main.rand.NextFloat(3, 4)).RotatedByRandom(MathHelper.ToRadians(120)), 0f, 20);
                }
                if (Main.rand.NextBool(1))
                {
                    emitter?.Emit(NPC.Top - new Vector2(0, 10), (-Vector2.UnitY * Main.rand.NextFloat(8, 9)).RotatedByRandom(MathHelper.ToRadians(10)), 0f, 20);
                }
                if (NPC.ai[1] != 2)
                {
                    NPC.localAI[0] = 0;
                    NPC.localAI[1] = 0;
                    NPC.localAI[2] = 0;
                    AITimer = 0;
                    AI_State = ActionState.Controlled;
                }
                if (AITimer ++ >= 120)
                {
                    ExecuteCrazyMode(Main.player[(int)(NPC.ai[2])]);
                }
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
    float pulseScale = 1;
    float swayScale = 1;
    private void ExecuteCrazyMode(Player player)
    {
        NPC Wisp = MiscHelpers.NPCExists(wispID, ModContent.NPCType<MotherWisp>());
        if (hasWisp && Wisp == null)
        {
            NPC.timeLeft = 0;
            NPC.active = false;
            return;
        }
        NPC.localAI[0]++;
        if (NPC.localAI[0] >= 180)
        {
            switch (NPC.localAI[2])
            {
                case 0:
                    if (NPC.localAI[1]++ % 60 == 0 && NPC.localAI[1] != 0)
                    {
                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                        {
                            PositionInWorld = NPC.Center,

                        }, NPC.whoAmI);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int amount = 4;
                            amount = masterMode ? 9 : expertMode ? 7 : 5;
                            for (int i = -amount; i <= amount; i++)
                            {
                                if (i != 0)
                                {
                                    Vector2 spawnPos = new Vector2(NPC.Center.X, NPC.Center.Y);
                                    Vector2 velocity = new Vector2(i * 0.75f, -2);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos,
                                        velocity, ModContent.ProjectileType<CosmicSludgeBomb>(), ProjectileDamage(NPC.damage), 0f, -1, 0);
                                }
                            }
                        }
                    }
                    swayScale = MiscHelpers.BetterEssScale(2, 0.0035f);
                    pulseScale = 1;
                    break;
                case 1:
                    if (NPC.localAI[1]++ % 6 == 0)
                    {
                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                        {
                            PositionInWorld = NPC.Center,

                        }, NPC.whoAmI);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {

                            Vector2 spawnPos = new Vector2(NPC.Center.X, NPC.Center.Y);
                            Vector2 velocity = new Vector2(Main.rand.Next(-2,3),Main.rand.Next(-3,-1));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos,
                                velocity, ModContent.ProjectileType<CosmicSludgeBomb>(), ProjectileDamage(NPC.damage), 0f, -1, 0);
                        }
                    }
                    swayScale = MiscHelpers.BetterEssScale(2, 0.005f);
                    pulseScale = MiscHelpers.BetterEssScale(2, 0.25f);
                    break;
            }
            if (NPC.localAI[2] > 1)
            {
                NPC.localAI[2] = 0;
            }
            if (NPC.localAI[0] >= 180 * 2)
            {
                NPC.localAI[1] = 0;
                NPC.localAI[2]++;
                NPC.localAI[0] = 0;
            }
        }
        else
        {
            pulseScale = MiscHelpers.BetterEssScale(4, 0.25f);
            swayScale = MiscHelpers.BetterEssScale(2, 0.0035f);
        }
        NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(player.Center.X * swayScale, player.Center.Y - 400 * pulseScale), 0.1f);


    }
    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 stretch = new(1f, 1f);
        Texture2D tex = TextureAssets.Npc[NPC.type].Value;
        int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
        Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);

        Vector2 center = NPC.Size / 2f;
        Vector2 miragePos = NPC.Center - Main.screenPosition;

        //old treasure bag draw code, augh
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.75f;
        if (AI_State == ActionState.Detaching)
        {
            for (float i = 0f; i < 1f; i += 0.1f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                spriteBatch.Draw(tex, miragePos + new Vector2(0f, 4f).RotatedBy(radians) * time, null, new Color(16, 236, 195, 50) * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
            }

            for (float i = 0f; i < 1f; i += 0.2f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                spriteBatch.Draw(tex, miragePos + new Vector2(0f, 6f).RotatedBy(radians) * time, null, new Color(16, 236, 195, 50) * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
            }
        }
        
        spriteBatch.Draw(tex, miragePos, null, Color.White * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);

        return true;
    }
}
