using ITD.Content.Buffs.Debuffs;
using System;

namespace ITD.Content.NPCs.Bosses;

public class GhastlyTombstone : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
        NPCID.Sets.BossBestiaryPriority.Add(Type);
        NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
        {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
    }
    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 34;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.hide = true;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = 1000;
    }

    public override void AI()
    {
        if (NPC.ai[0] < 0 || !Main.npc[(int)NPC.ai[0]].active)
        {
            NPC.life = 0;
            NPC.checkDead();
        }

        if (NPC.localAI[1] == 0)
        {
            NPC.localAI[1] = 1;
            for (int l = 0; l < 10; l++)
            {
                int spawnDust = Dust.NewDust(NPC.Center, 16, 16, DustID.DungeonSpirit, 0, 0, 0, default, 2f);
                Main.dust[spawnDust].noGravity = true;
                Main.dust[spawnDust].velocity *= 2f;
            }
        }

        NPC.localAI[0]++;
        NPC.position += new Vector2(0, (float)Math.Sin(NPC.localAI[0] * 0.2f) * 0.2f);
    }

    public static int[] TheList =
    [
        NPCID.BlueArmoredBones,
        NPCID.BlueArmoredBonesMace,
        NPCID.BlueArmoredBonesNoPants,
        NPCID.BlueArmoredBonesSword,

        NPCID.RustyArmoredBonesAxe,
        NPCID.RustyArmoredBonesFlail,
        NPCID.RustyArmoredBonesSword,
        NPCID.RustyArmoredBonesSwordNoArmor,

        NPCID.HellArmoredBones,
        NPCID.HellArmoredBonesMace,
        NPCID.HellArmoredBonesSpikeShield,
        NPCID.HellArmoredBonesSword,

        NPCID.Necromancer,
        NPCID.RaggedCaster,
        NPCID.DiabolistRed,

        NPCID.GiantCursedSkull,
        NPCID.DungeonSpirit,
        NPCID.DungeonSpirit,
    ];
    public override bool CheckDead()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("TombstoneGore0").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("TombstoneGore1").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("TombstoneGore2").Type);
        }
        if (NPC.ai[0] >= 0)
        {
            NPC npc = Main.npc[(int)NPC.ai[0]];
            if (npc.active)
            {
                for (int l = 0; l < 20; l++)
                {
                    int spawnDust = Dust.NewDust(npc.Center, 0, 0, DustID.DungeonSpirit, 0, 0, 0, default, 2f);
                    Main.dust[spawnDust].noGravity = true;
                    Main.dust[spawnDust].velocity *= 3f;
                }
                npc.StrikeNPC(new NPC.HitInfo
                {
                    Damage = 1000,
                    Knockback = 0f,
                    HitDirection = 0,
                    Crit = true
                });
            }
        }
        if (NPC.ai[0] == -1 || (Main.masterMode && Main.npc[(int)NPC.ai[0]].active))
        {
            for (int l = 0; l < 10; l++)
            {
                int spawnDust = Dust.NewDust(NPC.Center, 16, 16, DustID.DungeonSpirit, 0, 0, 0, default, 2f);
                Main.dust[spawnDust].noGravity = true;
                Main.dust[spawnDust].velocity *= 2f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC npc = Main.npc[NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, TheList[Main.rand.Next(18)])];
                if (NPC.ai[0] >= 0) //hell is war
                {
                    npc.AddBuff<SoulRotBuff>(3600, false);
                    npc.life = (int)(npc.lifeMax * 0.66f);
                }
            }
        }
        return true;
    }

    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 position = NPC.Center - Main.screenPosition;

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle sourceRectangle = texture.Frame(1, 1);
        Vector2 origin = sourceRectangle.Size() / 2f;

        Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
        Rectangle glowRectangle = glowTexture.Frame(1, 1);
        Vector2 glowOrigin = glowRectangle.Size() / 2f;

        Main.EntitySpriteDraw(glowTexture, position, glowRectangle, Color.White, 0, glowOrigin, NPC.scale, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.Lerp(Color.White, drawColor, 0.5f), 0, origin, NPC.scale, SpriteEffects.None, 0f);
        return false;
    }
}