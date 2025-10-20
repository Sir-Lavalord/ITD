using ITD.Content.Items.Materials;
using ITD.Content.Items.Placeable.Biomes.DeepDesert;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

namespace ITD.Content.NPCs.DeepDesert;

public class EmberlionMiner : ModNPC
{
    public bool CanFly { get; set; }
    public Vector2? ForcedTargetPosition { get; set; }
    public float MoveSpeed { get; set; } = 5f;
    public float Acceleration { get; set; } = 0.1f;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 8;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
    }
    public override void SetDefaults()
    {
        NPC.width = 32;
        NPC.height = 32;
        NPC.damage = 60;
        NPC.defense = 20;
        NPC.lifeMax = 150;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.hide = true;
    }
    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
    }
    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 1f;
        if (NPC.frameCounter > 4f)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y > frameHeight * Main.npcFrameCount[Type] - 1)
            {
                NPC.frame.Y = frameHeight;
            }
        }
    }
    public override bool PreAI()
    {
        if (!NPC.HasValidTarget)
        {
            NPC.TargetClosest(true);
        }
        return true;
    }
    public override void AI()
    {
        bool collision = CheckCollisionForDustSpawns();

        CheckTargetDistance(ref collision);

        Movement(collision);
    }
    private void CheckTargetDistance(ref bool collision)
    {
        // If there is no collision with tiles, we check if the distance between this NPC and its target is too large, so that we can still trigger "collision".
        if (!collision)
        {
            Rectangle hitbox = NPC.Hitbox;

            int maxDistance = 1000;

            bool tooFar = true;

            foreach (var player in Main.ActivePlayers)
            {
                Rectangle areaCheck;

                if (ForcedTargetPosition is Vector2 target)
                    areaCheck = new Rectangle((int)target.X - maxDistance, (int)target.Y - maxDistance, maxDistance * 2, maxDistance * 2);
                else if (!player.dead && !player.ghost)
                    areaCheck = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);
                else
                    continue;  // Not a valid player

                if (hitbox.Intersects(areaCheck))
                {
                    tooFar = false;
                    break;
                }
            }

            if (tooFar)
                collision = true;
        }
    }
    private void Movement(bool collision)
    {
        float targetXPos, targetYPos;

        Player playerTarget = Main.player[NPC.target];

        Vector2 forcedTarget = ForcedTargetPosition ?? playerTarget.Center;
        (targetXPos, targetYPos) = (forcedTarget.X, forcedTarget.Y);
        Vector2 npcCenter = NPC.Center;

        float targetRoundedPosX = (int)(targetXPos / 16f) * 16;
        float targetRoundedPosY = (int)(targetYPos / 16f) * 16;
        npcCenter.X = (int)(npcCenter.X / 16f) * 16;
        npcCenter.Y = (int)(npcCenter.Y / 16f) * 16;
        float dirX = targetRoundedPosX - npcCenter.X;
        float dirY = targetRoundedPosY - npcCenter.Y;

        float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

        // If we do not have any type of collision, we want the NPC to fall down and de-accelerate along the X axis.
        if (!collision && !CanFly)
            Movement_HandleFallingFromNoCollision(dirX, MoveSpeed, Acceleration);
        else
        {
            // Else we want to play some audio (soundDelay) and move towards our target.
            Movement_PlayDigSounds(length);

            Movement_HandleMovement(dirX, dirY, length, MoveSpeed, Acceleration);
        }

        Movement_SetRotation(collision);
    }

    private void Movement_HandleFallingFromNoCollision(float dirX, float speed, float acceleration)
    {
        // Keep searching for a new target
        NPC.TargetClosest(true);

        // Constant gravity of 0.11 pixels/tick
        NPC.velocity.Y += 0.11f;

        // Ensure that the NPC does not fall too quickly
        if (NPC.velocity.Y > speed)
            NPC.velocity.Y = speed;

        // The following behavior mimics vanilla worm movement
        if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4f)
        {
            // Velocity is sufficiently fast, but not too fast
            if (NPC.velocity.X < 0.0f)
                NPC.velocity.X -= acceleration * 1.1f;
            else
                NPC.velocity.X += acceleration * 1.1f;
        }
        else if (NPC.velocity.Y == speed)
        {
            // NPC has reached terminal velocity
            if (NPC.velocity.X < dirX)
                NPC.velocity.X += acceleration;
            else if (NPC.velocity.X > dirX)
                NPC.velocity.X -= acceleration;
        }
        else if (NPC.velocity.Y > 4)
        {
            if (NPC.velocity.X < 0)
                NPC.velocity.X += acceleration * 0.9f;
            else
                NPC.velocity.X -= acceleration * 0.9f;
        }
    }

    private void Movement_PlayDigSounds(float length)
    {
        if (NPC.soundDelay == 0)
        {
            // Play sounds quicker the closer the NPC is to the target location
            float num1 = length / 40f;

            if (num1 < 10)
                num1 = 10f;

            if (num1 > 20)
                num1 = 20f;

            NPC.soundDelay = (int)num1;

            SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
        }
    }

    private void Movement_HandleMovement(float dirX, float dirY, float length, float speed, float acceleration)
    {
        float absDirX = Math.Abs(dirX);
        float absDirY = Math.Abs(dirY);
        float newSpeed = speed / length;
        dirX *= newSpeed;
        dirY *= newSpeed;

        if ((NPC.velocity.X > 0 && dirX > 0) || (NPC.velocity.X < 0 && dirX < 0) || (NPC.velocity.Y > 0 && dirY > 0) || (NPC.velocity.Y < 0 && dirY < 0))
        {
            // The NPC is moving towards the target location
            if (NPC.velocity.X < dirX)
                NPC.velocity.X += acceleration;
            else if (NPC.velocity.X > dirX)
                NPC.velocity.X -= acceleration;

            if (NPC.velocity.Y < dirY)
                NPC.velocity.Y += acceleration;
            else if (NPC.velocity.Y > dirY)
                NPC.velocity.Y -= acceleration;

            // The intended Y-velocity is small AND the NPC is moving to the left and the target is to the right of the NPC or vice versa
            if (Math.Abs(dirY) < speed * 0.2 && ((NPC.velocity.X > 0 && dirX < 0) || (NPC.velocity.X < 0 && dirX > 0)))
            {
                if (NPC.velocity.Y > 0)
                    NPC.velocity.Y += acceleration * 2f;
                else
                    NPC.velocity.Y -= acceleration * 2f;
            }

            // The intended X-velocity is small AND the NPC is moving up/down and the target is below/above the NPC
            if (Math.Abs(dirX) < speed * 0.2 && ((NPC.velocity.Y > 0 && dirY < 0) || (NPC.velocity.Y < 0 && dirY > 0)))
            {
                if (NPC.velocity.X > 0)
                    NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
                else
                    NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
            }
        }
        else if (absDirX > absDirY)
        {
            // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
            if (NPC.velocity.X < dirX)
                NPC.velocity.X += acceleration * 1.1f;
            else if (NPC.velocity.X > dirX)
                NPC.velocity.X -= acceleration * 1.1f;

            if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
            {
                if (NPC.velocity.Y > 0)
                    NPC.velocity.Y += acceleration;
                else
                    NPC.velocity.Y -= acceleration;
            }
        }
        else
        {
            // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
            if (NPC.velocity.Y < dirY)
                NPC.velocity.Y += acceleration * 1.1f;
            else if (NPC.velocity.Y > dirY)
                NPC.velocity.Y -= acceleration * 1.1f;

            if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
            {
                if (NPC.velocity.X > 0)
                    NPC.velocity.X += acceleration;
                else
                    NPC.velocity.X -= acceleration;
            }
        }
    }

    private void Movement_SetRotation(bool collision)
    {
        // Set the correct rotation for this NPC.
        // Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

        // Some netupdate stuff (multiplayer compatibility).
        if (collision)
        {
            if (NPC.localAI[0] != 1)
                NPC.netUpdate = true;

            NPC.localAI[0] = 1f;
        }
        else
        {
            if (NPC.localAI[0] != 0)
                NPC.netUpdate = true;

            NPC.localAI[0] = 0f;
        }

        // Force a netupdate if the NPC's velocity changed sign and it was not "just hit" by a player
        if (((NPC.velocity.X > 0 && NPC.oldVelocity.X < 0) || (NPC.velocity.X < 0 && NPC.oldVelocity.X > 0) || (NPC.velocity.Y > 0 && NPC.oldVelocity.Y < 0) || (NPC.velocity.Y < 0 && NPC.oldVelocity.Y > 0)) && !NPC.justHit)
            NPC.netUpdate = true;
    }
    private bool CheckCollisionForDustSpawns()
    {
        int minTilePosX = (int)(NPC.Left.X / 16) - 1;
        int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
        int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
        int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;

        // Ensure that the tile range is within the world bounds
        if (minTilePosX < 0)
            minTilePosX = 0;
        if (maxTilePosX > Main.maxTilesX)
            maxTilePosX = Main.maxTilesX;
        if (minTilePosY < 0)
            minTilePosY = 0;
        if (maxTilePosY > Main.maxTilesY)
            maxTilePosY = Main.maxTilesY;

        bool collision = false;

        // This is the initial check for collision with tiles.
        for (int i = minTilePosX; i < maxTilePosX; ++i)
        {
            for (int j = minTilePosY; j < maxTilePosY; ++j)
            {
                Tile tile = Main.tile[i, j];

                // If the tile is solid or is considered a platform, then there's valid collision
                if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0) || tile.LiquidAmount > 64)
                {
                    Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);

                    if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y && NPC.Top.Y < tileWorld.Y + 16)
                    {
                        // Collision found
                        collision = true;

                        if (Main.rand.NextBool(100))
                            WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
                    }
                }
            }
        }

        return collision;
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.GetITDPlayer().ZoneDeepDesert)
        {
            return 0.15f;
        }
        return 0f;
    }
    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Pegmatite>(), 1, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberlionSclerite>(), 1, 1, 2));
    }
}
