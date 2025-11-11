using ITD.Content.Biomes;
using Terraria.Localization;

namespace ITD.Content.NPCs.BlueshroomGroves;

public class CicadianHive : ModNPC
{
    public static LocalizedText BestiaryEntry { get; private set; }
    public int frameGroup = 1;
    public int attackTimer = 0;
    private enum ActionState
    {
        Spawning,
        Hovering,
        Escaping,
    }
    private ActionState AI_State;
    private int timeBetweenFrames;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 7;
        BestiaryEntry = Language.GetOrRegister(Mod.GetLocalizationKey($"NPCs.{nameof(CicadianHive)}.Bestiary"));
    }
    public override void SetDefaults()
    {
        AI_State = ActionState.Hovering;
        timeBetweenFrames = 4;
        NPC.width = 66;
        NPC.height = 82;
        NPC.damage = 30;
        NPC.defense = 10;
        NPC.lifeMax = 200;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.noGravity = true;
        NPC.aiStyle = -1;
        SpawnModBiomes = [ModContent.GetInstance<BlueshroomGrovesBiome>().Type];
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 1f;
        int maxFrame;
        if (frameGroup == 2)
        {
            maxFrame = 14;
        }
        else
        {
            maxFrame = 7;
        }

        if (NPC.frameCounter > timeBetweenFrames)
        {
            NPC.frameCounter = 0;

            if (frameGroup == 2)
            {
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * maxFrame)
                {
                    NPC.frame.Y = frameHeight * (maxFrame - 7);
                }
            }
            else
            {
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * maxFrame)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (AI_State == ActionState.Hovering)
        {
            AI_State = ActionState.Escaping;
        }
    }
    public override void AI()
    {
        attackTimer++;
        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
        {
            NPC.TargetClosest();
        }
        Player player = Main.player[NPC.target];

        int tileRange = 16;
        int hoverDistance = 16;

        float maxRotation = MathHelper.Pi / 6;
        float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
        float rotation = rotationFactor * maxRotation;
        NPC.rotation = rotation;

        if (attackTimer > 360)
        {
            if (Vector2.Distance(NPC.Center, player.Center) > tileRange * 16)
            {
                frameGroup = 2;
                AI_State = ActionState.Spawning;
            }
            attackTimer = 0;
        }

        switch (AI_State)
        {
            case ActionState.Spawning:
                timeBetweenFrames = 4;
                SpawnEnemies();
                frameGroup = 1;
                AI_State = ActionState.Escaping;
                break;
            case ActionState.Hovering:
                timeBetweenFrames = 4;
                RaycastData ray = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, maxDistTiles: 32);
                Vector2 hoverPoint = ray.End - new Vector2(0f, hoverDistance * 16);
                Vector2 toHoverPoint = (hoverPoint - NPC.Center) / 32f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHoverPoint, 0.05f);
                if (Vector2.Distance(NPC.Center, player.Center) < tileRange * 16)
                {
                    AI_State = ActionState.Escaping;
                }
                break;
            case ActionState.Escaping:
                timeBetweenFrames = 2;
                float speed = 800f;
                Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
                Vector2 escapeVector = -toPlayer / (Vector2.Distance(NPC.Center, player.Center) * 1f);
                escapeVector.Y /= 4f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, escapeVector, 0.1f);
                if (Vector2.Distance(NPC.Center, player.Center) > tileRange * 16)
                {
                    AI_State = ActionState.Hovering;
                }
                break;
        }
    }

    private void SpawnEnemies()
    {
        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)(NPC.Center.Y + 30f), ModContent.NPCType<ShroomishSlime>());
        NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X - 20f), (int)(NPC.Center.Y - 30f), ModContent.NPCType<ShroomishSlime>());
        NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X - 20f), (int)(NPC.Center.Y - 30f), ModContent.NPCType<ShroomishSlime>());
    }
}
