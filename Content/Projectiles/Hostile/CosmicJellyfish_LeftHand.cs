using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.DataStructures;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using System.IO;
using Terraria.GameContent.Drawing;
using SteelSeries.GameSense;


namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicJellyfish_RightHand : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/Hostile/CosmicJellyfish_Hand";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
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
        bool bSmackdown;
        bool bStopfalling;
        int iMeteorCount;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (handState == HandState.MeteorStrike)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                if (!bStopfalling)
                {

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile Blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<CosmicLightningBlast>(), (int)(Projectile.damage), 2f, -1, Projectile.owner);
                            Blast.ai[1] = 300f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;
                        }
                        Projectile.position += Projectile.velocity;
                        Projectile.velocity = Vector2.Zero;
                    
                }
                bStopfalling = true;
                Projectile.netUpdate = true;

            }

            return false;

        }
        public override void OnSpawn(IEntitySource source)
        {
            //0 = voidring
            //1 = starfall
            //2 = basic swing
            //3 = wait swing
            //4 = meteor
            //5 = super meteor
            NPC NPC = Main.npc[(int)Projectile.ai[0]];
            if (NPC.active && NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                switch (Projectile.ai[2])
                {
                    case 0:

                            handState = HandState.StarShard;
                        
                        break;
                    case 2:

                            handState = HandState.Charging;

                        break;
                    case 3:

                            handState = HandState.Waiting;

                        break;

                    case 4:

                            handState = HandState.MeteorStrike;
                        break;
                }
            }
            Projectile.scale = 0.1f;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 15;
            height = 15;
            NPC NPC = Main.npc[(int)Projectile.ai[0]];
            if (NPC.active && NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[NPC.target];
                fallThrough = player.Center.Y >= Projectile.Bottom.Y - 10 && !bStopfalling;
            }

            return true;


        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.Center, 8, 8, DustID.PortalBolt, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1);
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
        private enum HandState
        {
            Waiting,
            Charging,
            Slinging,
            DownToSize,
            ForcedKill,
            VoidShard,
            StarShard,
            HandSlam,
            MeteorStrike,
        }
        //USELESS LEST ARTIFICIAL DIFFICULTY
        Vector2 vLockedIn;
        private HandState handState = HandState.Waiting;
        public float rotation = 0f;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        private Vector2 handTarget = Vector2.Zero;
        private Vector2 handStatic = Vector2.Zero;
        private bool targetPicked = false;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(vLockedIn);
            writer.Write(iMeteorCount);
            writer.Write(bSmackdown);
            writer.Write(bStopfalling);
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(Projectile.localAI[2]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bStopfalling = reader.ReadBoolean();
            bSmackdown = reader.ReadBoolean();
            vLockedIn = reader.ReadVector2();
            iMeteorCount = reader.ReadInt32();
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            Projectile.localAI[2] = reader.ReadSingle();
        }
        public override void AI()
        {
            NPC NPC = Main.npc[(int)Projectile.ai[0]];
            if (NPC.active && NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[NPC.target];
                if (Projectile.scale < 1f)
                {
                    Projectile.scale += 0.05f;
                }
                Projectile.timeLeft = 2;
                Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                Vector2 toPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Vector2 chargedPosition = NPC.Center - extendOut * 150 + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                Vector2 normalCenter = NPC.Center - extendOut * 150 + new Vector2(0f, NPC.velocity.Y);
                float targetRotation = NPC.rotation;
                if (handState != HandState.Charging && handState != HandState.Slinging
                    && (!bSmackdown))
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
                else if (handState == HandState.Charging)
                {
                    Projectile.frame = 5;
                }
                else if (handState == HandState.Slinging || (handState == HandState.MeteorStrike && bSmackdown))
                {
                    Projectile.frame = 6;
                }
                switch (handState)
                {
                    case HandState.Waiting:
                        Projectile.netUpdate = true;
                        Projectile.tileCollide = false;
                        bSmackdown = false;
                        bStopfalling = false;
                        iMeteorCount = 0;
                        Projectile.localAI[0] = 0;
                        handSling = 0f;
                        handCharge = 0f;
                        handFollowThrough = 0f;
                        Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);

                            if (NPC.ai[2] == 1)
                            {
                                handState = HandState.Charging;
                            }
                        
                        break;
                        
                    case HandState.Charging:
                        if (handCharge < 1f)
                        {
                            handCharge += 0.04f;
                            targetRotation += handCharge;
                        }
                        else
                        {
                            if (Projectile.localAI[0]++ == 0)
                            {
                                vLockedIn = player.Center;
                            }
                            else if (Projectile.localAI[0] >= 5 && NPC.localAI[2] > 6 || Projectile.localAI[0] >= 0 && NPC.localAI[2] <= 6)
                            {
                                player.GetITDPlayer().Screenshake = 5;

                                Projectile.localAI[0] = 0;
                                handState = HandState.Slinging;
                                Vector2 toTarget = (vLockedIn - Projectile.Center).SafeNormalize(Vector2.Zero);
                                handTarget = vLockedIn + toTarget * 120f;
                                handStatic = Projectile.Center;
                            }
                        }
                        Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                        break;
                    case HandState.Slinging:
                        targetPicked = false;
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
                                if (NPC.life <= NPC.lifeMax/2)// cosjel check but bad
                                {
                                    NPC.ai[1] = 1;
                                    NPC.ai[2] = 0;
                                    Projectile.netUpdate = true;
                                }
                                else
                                    Projectile.Kill();
                                handState = HandState.Waiting;
                            }
                        }
                        Projectile.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                        break;
                    case HandState.DownToSize:
                        handSling = 0f;
                        handCharge = 0f;
                        handFollowThrough = 0f;
                        Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        if (Projectile.localAI[0]++ >= 30)
                        {
                                handState = HandState.Waiting;
                                Projectile.Kill();

                        }
                        break;
                    case HandState.ForcedKill:
                        handState = HandState.Waiting;
                        Projectile.Kill();
                        Projectile.localAI[0] = 0;
                        handSling = 0f;
                        handCharge = 0f;
                        handFollowThrough = 0f;
                        Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        break;
                    case HandState.VoidShard:
                            if (Projectile.localAI[0]++ >= 80)
                            {
                                Projectile.localAI[0] = 0;
                                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                                //P2 stat garbage here
                                int ProjectileAmount = Main.rand.Next(5, 11);
                                float radius = 6.5f;
                                float sector = MathHelper.ToRadians(80f);
                                //Full circle
                                if (Main.expertMode || Main.masterMode)
                                {
                                    ProjectileAmount = Main.rand.Next(16, 20);
                                    sector = (float)(MathHelper.TwoPi);
                                }
                                float sectorOfSector = sector / ProjectileAmount;
                                float towardsAngle = toPlayer.ToRotation();
                                float startAngle = towardsAngle - sectorOfSector * (ProjectileAmount - 1) / 2;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < ProjectileAmount; i++)
                                    {
                                        float angle = startAngle + sectorOfSector * i;
                                        Vector2 ProjectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, ProjectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                                    }
                                }
                            }
                            Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        if (NPC.ai[3] !=2)
                        {
                            Projectile.netUpdate = true;
                            handState = HandState.ForcedKill;
                        }
                        break;
                    case HandState.MeteorStrike:
                        if (NPC.ai[2] == 1)
                        {
                            Vector2 ToPlayerHead = new Vector2(player.Center.X + player.velocity.X * 20f, player.Center.Y - 300) + new Vector2(0f, NPC.velocity.Y);

                            if (Projectile.localAI[1]++ <= 50)
                            {
                                Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                            }
                            else
                            {
                                if (!bStopfalling)
                                {
                                    if (!bSmackdown)
                                    {
                                        if (handCharge < 1f)
                                        {
                                            handCharge += 0.04f;
                                            targetRotation += handCharge;
                                        }
                                        else
                                        {
                                            if (Projectile.localAI[0]++ == 0)
                                            {
                                                vLockedIn = player.Center;
                                            }
                                            else if (Projectile.localAI[0] >= 5 && NPC.localAI[2] > 6 || Projectile.localAI[0] >= 0 && NPC.localAI[2] <= 6)
                                            {
                                                Projectile.netUpdate = true;

                                                bSmackdown = true;
                                                Vector2 toTarget = (vLockedIn - Projectile.Center).SafeNormalize(Vector2.Zero);
                                                handTarget = vLockedIn + toTarget * 120f;
                                                handStatic = Projectile.Center;
                                            }
                                        }
                                        Vector2 chargedToPlayerPosition = ToPlayerHead + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                                        Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                                    }
                                    else
                                    {
                                        Projectile.localAI[2] = 0;
                                        Projectile.localAI[0] = 0;
                                        handSling = 0f;
                                        handCharge = 0f;
                                        handFollowThrough = 0f;
                                        Projectile.tileCollide = true;
                                        Projectile.velocity.Y += 1.5f;
                                    }
                                }
                                else
                                {
                                    if (Projectile.localAI[2]++ >= 1)
                                    {
                                        Projectile.localAI[2] = 0;
                                        player.GetITDPlayer().Screenshake = 10;
                                        if (iMeteorCount++ <= 10)
                                        {
                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                            {
                                                for (int i = 0; i < Main.maxPlayers; i++)
                                                {
                                                    for (int f = 0; f < 2; f++)
                                                    {
                                                        SoundEngine.PlaySound(SoundID.Item88, Main.player[i].Center);
                                                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Main.player[i].Center.X + (Main.player[i].velocity.X * 20f) + (Main.rand.Next(-40, 40)), Main.player[i].Center.Y - 500)
                                                            , new Vector2(0, 6).RotatedByRandom(0.01) * Main.rand.NextFloat(0.85f, 1), Main.rand.Next(ProjectileID.Meteor1, ProjectileID.Meteor3 + 1), Projectile.damage, Projectile.knockBack, Main.player[i].whoAmI);
                                                        Main.projectile[proj].hostile = true;
                                                        Main.projectile[proj].friendly = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Projectile.tileCollide = false;
                                            if (NPC.Center.X < player.Center.X)
                                            {
                                                NPC.ai[1] = 1;
                                                NPC.ai[2] = 0;
                                            }
                                            else
                                            {
                                                NPC.ai[1] = 0;
                                                NPC.ai[2] = 1;
                                            }
                                            Projectile.netUpdate = true;
                                            bSmackdown = false;
                                            bStopfalling = false;
                                            iMeteorCount = 0;
                                            Projectile.localAI[0] = 0;
                                            Projectile.localAI[1] = 0;
                                            Projectile.localAI[2] = 0;

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                        }
                        break;
                }
            
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.3f);
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];

                    if (i != Projectile.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                    {
                        if (Projectile.localAI[0] == 0 && handState == HandState.Slinging && handFollowThrough < 1f &&
                            player.Distance(Projectile.Center) < 60f
                            )
                        {
                            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                new ParticleOrchestraSettings { PositionInWorld = other.Center }, Projectile.owner);
                            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), Projectile.Center);
                            player.GetITDPlayer().Screenshake = 20;
                            if (NPC.life > NPC.lifeMax / 2)
                                handState = HandState.DownToSize;
                            CombatText.NewText(Projectile.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                            Projectile.velocity = -Projectile.velocity * 2;
                            // if the achievements mod is on, unlock the parry achievement
                            ITD.Instance.achievements?.Call("Event", "ParryNPCHand");
                        }
                    }
                }
        }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[Projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[Projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * Projectile.frame, tex.Width, vertSize);
                if (handState == HandState.Slinging || (handState == HandState.MeteorStrike && bSmackdown))
                {
                    for (int k = 0; k < Projectile.oldPos.Length; k++)
                    {
                        Vector2 center = Projectile.Size / 2f;
                        Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                        Main.EntitySpriteDraw(tex, drawPos, frameRect, color, Projectile.oldRot[k], origin, Projectile.scale, SpriteEffects.None, 0f);
                    }
                }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frameRect, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}