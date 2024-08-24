using ITD.Content.Biomes;
using ITD.Content.Projectiles.Friendly.Snaptraps;
using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.NPCs.BlueshroomGroves
{
    public class Cicadian : ModNPC
    {
        public static LocalizedText BestiaryEntry { get; private set; }
        private static float xSpeed = 2.2f;
        private enum ActionState
        {
            Background,
            Transition,
            Chasing,
            Charging,
            Dashing,
        }
        private ActionState AI_State;
        private float transitionProgress;
        private Vector2 anchorPoint;
        private int boulderCooldown = 0;

        public bool chopped = false;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
            Main.npcFrameCount[Type] = 7;
            BestiaryEntry = Language.GetOrRegister(Mod.GetLocalizationKey($"NPCs.{nameof(Cicadian)}.Bestiary"));
        }
        public override void SetDefaults()
        {
            AI_State = ActionState.Background;
            transitionProgress = 0f;
            NPC.width = 164;
            NPC.height = 64;
            NPC.damage = 90;
            NPC.defense = 85;
            NPC.lifeMax = 12000;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.aiStyle = -1;
            NPC.noTileCollide = true;
            SpawnModBiomes = [ModContent.GetInstance<BlueshroomGrovesBiome>().Type];
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
                new FlavorTextBestiaryInfoElement(BestiaryEntry.Value)
            ]);
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            if (AI_State == ActionState.Background)
            {
                boundingBox = Rectangle.Empty;
            }
            else
            {
                boundingBox = NPC.getRect();
            }
        }
        public override void AI()
        {
            if (!Main.npc[(int)NPC.ai[0]].active && !chopped)
            {
                chopped = true;
                NPC.defense -= 25;
                if (AI_State == ActionState.Background)
                    AI_State = ActionState.Transition;
            }
            switch (AI_State)
            {
                case ActionState.Background:
                    NPC.hide = true;
                    NPC.position = anchorPoint;
                    NPC.velocity = Vector2.Zero;
                    break;
                case ActionState.Transition:
                    transitionProgress += 0.02f;
                    SpawnDiggingDust();
                    NPC.velocity = new Vector2(0f, -1f);
                    if (transitionProgress >= 1f)
                        AI_State = ActionState.Chasing;
                    break;
                case ActionState.Chasing:
                    NPC.noTileCollide = false;
                    NPC.hide = false;
                    DoChase();
                    break;
                default:
                    break;
            }
        }
        private void LaunchIcyBoulder(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center + player.velocity;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            int toPlayerDirection = Math.Sign(toPlayerNormalized.X);
            float gravity = IcyBoulder.IcyBoulderGravity;
            float distance = toPlayer.Length();
            float speed = 14f;

            float verticalDistance = Math.Abs(toPlayer.Y);

            float angle = (float)Math.Atan((Math.Pow(speed, 2) + Math.Sqrt(Math.Pow(speed, 4) - gravity * (gravity * Math.Pow(distance, 2) + 2 * verticalDistance * Math.Pow(speed, 2)))) / (gravity * distance));

            float velocityX = speed * (float)Math.Cos(angle) * toPlayerDirection;
            float velocityY = speed * (float)Math.Sin(angle);
            Vector2 velocity = new Vector2(velocityX, -velocityY);
            if (velocity.HasNaNs())
                velocity = (toPlayerNormalized * speed) + new Vector2(0f, -5f);

            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<IcyBoulder>(), 30, 0.2f);
        }
        private void DoChase()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            int playerDirectionX = NPC.Center.X < player.Center.X ? 1 : -1;
            NPC.direction = playerDirectionX;
            NPC.velocity.X = playerDirectionX * xSpeed;
            CheckForSideTiles();
            boulderCooldown--;
            if (boulderCooldown <= 0 && toPlayer.Length() > 400f && Math.Abs(toPlayer.Y) > 80f)
            {
                LaunchIcyBoulder(player);
                boulderCooldown = 120;
            }
        }
        private void CheckForSideTiles()
        {
            Tile right = Framing.GetTileSafely(new Vector2(NPC.position.X+NPC.width+0.1f, NPC.position.Y+NPC.height-0.1f));
            Tile rightUp = Framing.GetTileSafely(new Vector2(NPC.position.X + NPC.width + 0.1f, NPC.position.Y + NPC.height - 16.1f));
            Tile left = Framing.GetTileSafely(new Vector2(NPC.position.X - 0.1f, NPC.position.Y + NPC.height - 0.1f));
            Tile leftUp = Framing.GetTileSafely(new Vector2(NPC.position.X - 0.1f, NPC.position.Y + NPC.height - 16.1f));
            static bool CanCollide(Tile tile)
            {
                return tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated;
            }
            bool rightIsClimbableSlope = right.Slope == SlopeType.SlopeDownRight;
            bool leftIsClimbableSlope = left.Slope == SlopeType.SlopeDownLeft;
            if (CanCollide(right) && !CanCollide(rightUp) && NPC.velocity.X > 0f && !rightIsClimbableSlope)
            {
                float tp = 16f;
                if (right.IsHalfBlock)
                {
                    tp = 8f;
                }
                NPC.position.Y -= tp;
            }
            if (CanCollide(left) && !CanCollide(leftUp) && NPC.velocity.X < 0f && !leftIsClimbableSlope)
            {
                float tp = 16f;
                if (left.IsHalfBlock)
                {
                    tp = 8f;
                }
                NPC.position.Y -= tp;
            }
        }
        private void SpawnDiggingDust()
        {
            int amount = 6;
            for (int i = 0; i < amount; i++)
            {
                if (Main.rand.NextBool(12))
                {
                    Vector2 spawnPosition = Helpers.QuickRaycast(NPC.position, Vector2.UnitY, false, 8f) + new Vector2(NPC.width / amount * i, 0f);
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), spawnPosition, Vector2.Zero, Main.rand.Next(61, 64));
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            anchorPoint = NPC.position + new Vector2(0f, NPC.height / 2f);
            //Main.NewText(NPC.whoAmI);
            //tree = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<CicadianTree>(), NPC.whoAmI - NPC.whoAmI == 0 ? 0 : 1, ai0: 0f, ai1: NPC.Center.X, ai2: NPC.Center.Y);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (AI_State == ActionState.Background)
                AI_State = ActionState.Transition;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (AI_State == ActionState.Chasing)
                return true;
            return false;
        }
        public override void DrawBehind(int index)
        {
            if (transitionProgress < 1f)
            {
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            }
            else
            {
                NPC.hide = false;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (AI_State == ActionState.Chasing || AI_State == ActionState.Transition)
            {
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > 5f)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 2))
                    {
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            if (AI_State == ActionState.Background)
            {
                NPC.frame.Y = frameHeight * (Main.npcFrameCount[Type] - 1);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AI_State == ActionState.Dashing)
            {
                Texture2D texture = TextureAssets.Npc[Type].Value;
                Vector2 drawOrigin = texture.Size() / 2f;
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);
                    Color color = drawColor * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(texture, drawPos, null, color, 0f, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                }
            }
            return true;
        }
        public override Color? GetAlpha(Color drawColor)
        {
            //float clamped = Helpers.Remap(transitionProgress, 0f, 1f, 0.5f, 1f);
            //return drawColor.MultiplyRGB(new Color(clamped, clamped, clamped, 1f));
            return drawColor;
        }
    }
}
