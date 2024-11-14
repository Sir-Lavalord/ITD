/*
using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ITD.Utilities;
using Terraria.DataStructures;
using ITD.Content.NPCs.Bosses;
using ITD.Particles.CosJel;
using ITD.Particles;
using System.Linq;
using Terraria.GameContent.Drawing;
using System.IO;


namespace ITD.Content.Projectiles.Hostile
{
    public enum CosJelHandState
    {
        Waiting,
        MeteorStrike,
        Charging,
        Slinging,
        DownToSize,
        TemperTantrum
    }
    public class CosmicJellyfish_Hand : ModProjectile
    {
        public enum AIState
        {
            Normal,
            Smackdown,
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // The recording mode
            Main.projFrames[Projectile.type] = 7;//I'm very mature
        }
        public override void SetDefaults()
        {
            Projectile.width = 64; Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        public bool isLeftHand;
        public bool bSmackdown;
        public int iMeteorCount;
        public int iDisFromBoss = 160;
        public int Timer;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        public bool bStopfalling;

        private Vector2 handTarget = Vector2.Zero;
        public AIState State { get { return (AIState)Projectile.ai[0]; } set { Projectile.ai[0] = (float)value; } }
        public CosJelHandState HandState { get { return (CosJelHandState)Projectile.ai[2]; } set { Projectile.ai[2] = (float)value; } }

        public override void SendExtraAI(BinaryWriter writer)
        {
            
            writer.Write(bStopfalling);
            writer.Write(isLeftHand);
            writer.Write(bSmackdown);
            writer.Write(iMeteorCount);
            writer.Write(Timer);
            writer.Write(handCharge);
            writer.Write(handSling);
            writer.Write(handFollowThrough);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bStopfalling = reader.ReadBoolean();
            isLeftHand = reader.ReadBoolean();
            bSmackdown = reader.ReadBoolean();

            iMeteorCount = reader.ReadInt32();
            Timer = reader.ReadInt32();

            handCharge = reader.ReadSingle();
            handSling = reader.ReadSingle();
            handFollowThrough = reader.ReadSingle();
        }
        //AI
        //0.0 is normal
        //0.1 is smackdown
        //1.1 is call to stop spawn explosion
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 15;
            height = 15;
            NPC NPC = Main.npc[(int)Projectile.ai[1]];
            if (NPC.active && NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[NPC.target];
                fallThrough = player.Center.Y >= Projectile.Bottom.Y - 10 && !bStopfalling;
            }

            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (HandState == CosJelHandState.MeteorStrike)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

                if (!bStopfalling)
                {
                    if (expertMode || masterMode)
                    {

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile Blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                            ModContent.ProjectileType<CosmicLightningBlast>(), (int)(Projectile.damage), 2f, -1, Projectile.owner);
                            Blast.ai[1] = 100f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;

                        }
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerTorch, new Vector2(Main.rand.NextFloat() * 6f, -8f + 8f * Main.rand.NextFloat()), 0, default(Color), 1.5f).noGravity = true;
                    }
                    Projectile.position += Projectile.velocity;
                    Projectile.velocity = Vector2.Zero;
                    bStopfalling = true;

                }
                Projectile.netUpdate = true;
            }
            return false;

        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0.1f;
        }
        public override void OnKill(int timeLeft)
        {

            NPC NPC = Main.npc[(int)Projectile.ai[1]];
            if (NPC.active && NPC.ModNPC is CosmicJellyfish cosJel)
            {
                if (isLeftHand)
                {
                    cosJel.hand2 = -1;
                    NetSync();
                }
                else
                {
                                        NetSync();

                    cosJel.hand = -1;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                ITDParticle spaceMist = ParticleSystem.NewParticle<SpaceMist>(Projectile.Center, (-Projectile.velocity).RotatedByRandom(3f), 5f);
                spaceMist.tag = Projectile;

            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
        public override void AI()
        {
            NPC NPC = Main.npc[(int)Projectile.ai[1]];
            if (NPC.active && NPC.ModNPC is CosmicJellyfish cosJel)
            {
                // get the other hand
                // this shouldn't cause many problems
                CosmicJellyfish_Hand otherHand = isLeftHand ? cosJel.RightHand : cosJel.LeftHand;
                Player player = Main.player[NPC.target];
                if (Projectile.scale < 1f)
                {
                    Projectile.scale += 0.05f;
                }
                Projectile.timeLeft = 2;
                int sideMult = isLeftHand ? -1 : 1;
                Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                Vector2 toPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Vector2 chargedPosition = NPC.Center - extendOut * (iDisFromBoss * sideMult) + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                Vector2 normalCenter = NPC.Center - extendOut * (iDisFromBoss * sideMult) + new Vector2(0f, NPC.velocity.Y);
                float targetRotation = NPC.rotation;
                // Animation
                if (HandState != CosJelHandState.Charging && HandState != CosJelHandState.Slinging && !bSmackdown)
                {
                    if (Projectile.frameCounter++ >= 6)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame >= 4)
                        {

                            Projectile.frame = 0;
                        }
                    }
                }
                else if (HandState == CosJelHandState.Charging)
                {
                    Projectile.frame = 5;
                }
                else if (HandState == CosJelHandState.Slinging || (HandState == CosJelHandState.MeteorStrike && bSmackdown))
                {
                    Projectile.frame = 6;
                }
                if (Projectile.ai[1] == 1)
                {
                    iDisFromBoss = 200;
                }
                else
                {
                    iDisFromBoss = 160;
                }
                //
                switch (HandState)
                {
                    case CosJelHandState.Waiting:
                        iMeteorCount = 0;
                        bSmackdown = false;
                        bStopfalling = false;
                        Timer = 0;
                        handSling = 0f;
                        handCharge = 0f;
                        handFollowThrough = 0f;
                        Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        NetSync();
                        break;
                    case CosJelHandState.Charging:
                        if (handCharge < 1f)
                        {
                            handCharge += 0.04f;
                            targetRotation += handCharge;
                        }
                        else
                        {
                            if (Timer++ == 0)
                            {
                                cosJel.vLockedIn = player.Center;
                            }
                            else if (Timer >= cosJel.iDelayTime && cosJel.bSecondStage || Timer >= 0 && !cosJel.bSecondStage)
                            {
                                Timer = 0;
                                if (Projectile.ai[0] != 1)
                                {
                                    NetSync();
                                    HandState = CosJelHandState.Slinging;
                                }
                                else
                                {
                                    Timer = 0;
                                    handSling = 0f;
                                    handCharge = 0f;
                                    handFollowThrough = 0f;
                                    Projectile.velocity.Y += 1.5f;
                                    bSmackdown = true;
                                    Projectile.tileCollide = true;
                                    HandState = CosJelHandState.MeteorStrike;
                                    NetSync();

                                }

                                Vector2 toTarget = (cosJel.vLockedIn - Projectile.Center).SafeNormalize(Vector2.Zero);
                                handTarget = cosJel.vLockedIn + toTarget * 120f;
                            }


                        }
                        Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                        break;
                    case CosJelHandState.Slinging:
                        handCharge = 0f;
                        if (handSling < 1f)
                        {
                            handSling += 0.03f;
                            targetRotation -= handSling;
                        }
                        else
                        {
                            if (handFollowThrough < 1f)
                            {
                                handFollowThrough += 0.1f;
                            }
                            else
                            {
                                if (cosJel.bSecondStage)
                                {
                                    if (cosJel.attackCount++ >= 6)
                                        Projectile.Kill();

                                    NetSync();
                                    if (otherHand != null && otherHand.HandState == CosJelHandState.Waiting)
                                        otherHand.HandState = CosJelHandState.Charging;
                                }
                                else
                                {
                                    NetSync();
                                    Projectile.Kill();
                                }
                                HandState = CosJelHandState.Waiting;
                            }
                        }
                        Projectile.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                        break;
                    case CosJelHandState.DownToSize:
                        handSling = 0f;
                        handCharge = 0f;
                        handFollowThrough = 0f;
                        Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        if (Timer++ >= 30)
                        {
                            if (cosJel.bSecondStage)
                            {
                                NPC.localAI[2]++;
                                NetSync();
                                if (!isLeftHand)
                                {
                                    if (otherHand != null && otherHand.HandState == CosJelHandState.Waiting)
                                        otherHand.HandState = CosJelHandState.Charging;
                                }
                                else
                                {
                                    if (otherHand != null)
                                        otherHand.HandState = CosJelHandState.Charging;
                                }
                                HandState = CosJelHandState.Waiting;
                            }
                            else
                            {
                                NetSync();
                                HandState = CosJelHandState.Waiting;
                                Projectile.Kill();
                            }
                        }
                        break;
                    case CosJelHandState.MeteorStrike:

                        if (bStopfalling)
                        {
                            Projectile.Center += Main.rand.NextVector2Circular(1, 1);
                            player.GetITDPlayer().BetterScreenshake(10, 5, 10, true);
                            if (iMeteorCount <= 12)
                            {
                                if (Timer++ >= 2)
                                {

                                    Timer = 0;
                                    iMeteorCount++;
                                    NetSync();
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        for (int f = 0; f < (isLeftHand ? 2 : 1); f++)
                                        {
                                            SoundEngine.PlaySound(SoundID.Item88, player.Center);
                                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(cosJel.vLockedIn.X + (Main.rand.Next(-40, 40)), player.Center.Y - 500)
                                                , new Vector2(0, 6).RotatedByRandom(0.01) * Main.rand.NextFloat(isLeftHand ? 0.65f : 0.75f, 1.1f), Main.rand.Next(424, 427), Projectile.damage, Projectile.knockBack, player.whoAmI);
                                            Main.projectile[proj].hostile = true;
                                            Main.projectile[proj].friendly = false;
                                            Main.projectile[proj].scale = Main.rand.NextFloat(2, 3f);

                                        }
                                    }
                                }
                            }
                            else
                            {
                                NetSync();
                                bStopfalling = false;
                                bSmackdown = false;
                                iMeteorCount = 0;
                                Timer = 0;
                                HandState = CosJelHandState.Waiting;
                            }
                        }

                        else
                        {

                            cosJel.vLockedIn.X = player.Center.X + (player.velocity.X * 40f);
                            Timer = 0;
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            Projectile.velocity.Y += 1.25f;
                            iMeteorCount = 0;
                            bSmackdown = true;
                            NetSync();


                        }


                        break;
                }
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];

                    if (i != Projectile.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                    {
                        if (Timer == 0 && HandState == CosJelHandState.Slinging && handFollowThrough < 1f &&
                            player.Distance(Projectile.Center) < 60f
                            )
                        {
                            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                new ParticleOrchestraSettings { PositionInWorld = other.Center }, Projectile.owner);
                            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), Projectile.Center);
                            player.GetITDPlayer().BetterScreenshake(20, 5, 5, true);
                            HandState = CosJelHandState.DownToSize;
                            NetSync();

                            if (NPC.life > NPC.lifeMax / 10)
                            {
                                NPC.life -= NPC.lifeMax / 10;
                            }
                            CombatText.NewText(Projectile.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                            Projectile.velocity = -Projectile.velocity * 2;
                            // if the achievements mod is on, unlock the parry achievement
                            ITD.Instance.achievements?.Call("Event", "ParryCosJelHand");
                        }
                    }
                }
            }
            if (Main.rand.NextBool(3) && HandState != CosJelHandState.Slinging && HandState != CosJelHandState.Charging && !bSmackdown)
            {
                Vector2 velo = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                Vector2 veloDelta = (Projectile.position - Projectile.oldPosition); // i can't use projectile.velocity here because we're manually changing the position for most of its existence
                Vector2 sideOffset = new Vector2(30f * (bSmackdown ? -1f : 1f), 0f) * (isLeftHand ? -1f : 1f); //hacky asf
                ITDParticle spaceMist = ParticleSystem.NewParticle<SpaceMist>(Projectile.Center + new Vector2(0f, Projectile.height / 2) + sideOffset, ((velo * 3f) + veloDelta).RotatedByRandom(0.6f), 0f);
                spaceMist.tag = Projectile;
            }
        }
        private void NetSync()//I weep
        {
            NPC NPC = Main.npc[(int)Projectile.ai[1]];
            NPC.netUpdate = true;
            Projectile.netUpdate = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, isLeftHand ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            if (HandState == CosJelHandState.Slinging || bSmackdown || HandState == CosJelHandState.Charging)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 center = Projectile.Size * 0.5f;
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + center;
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Vector2 origin = new(outline.Width * 0.5f, (outline.Height / Main.projFrames[Type]) * 0.5f);
                    sb.Draw(outline, drawPos, frame, color, Projectile.oldRot[k], origin, Projectile.scale, isLeftHand ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            DrawAtProj(outline);
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                if (mist is SpaceMist sMist)
                {
                    sMist.DrawOutline(sb);
                }
            }
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                mist.DrawCommon(sb, mist.Texture, mist.CanvasOffset);
            }
            DrawAtProj(texture);
            return false;
        }
    }
}
*/