using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using ITD.Content.Items.Other;
using Terraria.DataStructures;
using Terraria.Localization;

namespace ITD.Content.NPCs.Friendly

{
    public class StrawmanDummy : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            Main.npcFrameCount[NPC.type] = 7;

            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the bestiary
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }
        public override void SetDefaults()
        {
            NPC.width = 84;
            NPC.height = 78;
            NPC.aiStyle = -1;
            NPC.noTileCollide = false;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 421131325; //dummy
            NPC.noGravity = false;
            NPC.boss = true;
            NPC.HitSound = SoundID.NPCHit15;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.gfxOffY = -2;

        }
        public override void ModifyTypeName(ref string typeName)
        {
            string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(StrawmanItem)}.DummyType.{NPC.ai[0]}")).Value;
            typeName = Text + " " + typeName;
        }
        bool die;
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 4)
                NPC.life = 20000;

        }
        public override void AI()
        {
            Player player = Main.LocalPlayer;

            switch (NPC.ai[0])
            {
                case 0:
                    break;
                case 1:
                    NPC.defense = 50;
                    break;
                case 2:
                    NPC.knockBackResist = 0.4f;
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X *= 0.9f;
                    break;
                case 3:
                    NPC.damage = 100;
                    break;
                case 4:
                    NPC.lifeMax = 20000;
                    break;
                case 5:
                    if (NPC.ai[1]++ >= 200)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
                            NPC.ai[1] = 0;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(),
                                new Vector2(NPC.Center.X + 10 * NPC.direction, NPC.Center.Y), 
                                new Vector2(2 * NPC.spriteDirection,0), ProjectileID.BulletSnowman, 100, 0,
                                Main.myPlayer, NPC.whoAmI);
                        }
                    }
                        if (player.position.X <= NPC.position.X)
                            NPC.spriteDirection = -1;
                        else
                            NPC.spriteDirection = 1;


                    break;
                case 6:
                    NPC.boss = false;
                    break;
            }
            if (player.controlUseTile && (player.HeldItem.type == ModContent.ItemType<StrawmanItem>()) && player.noThrow == 0)
            {
                die = true;
            }

            if (die)
            {
                NPC.life -= int.MaxValue;
                SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);

                for (int i = 0; i < 1; i++)
                {
                    int goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                }
            }
        }
        public override void UpdateLifeRegen(ref int damage)
        {
            if (NPC.ai[0] != 4)
            {
                NPC.lifeRegen += 50000;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            if (NPC.ai[0] == 4)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.Blood, 0, 0f, 80, default, Main.rand.NextFloat(0.9f, 1.5f));
                    dust.velocity *= 1f;
                }
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    int goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                    goreIndex = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + (float)(NPC.width / 2) - 24f, NPC.position.Y + (float)(NPC.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                }

            }
        }
        int npcframe = 0;
        public override void FindFrame(int frameHeight)
        {
            npcframe = (int)NPC.ai[0];
            NPC.frame.Y = npcframe * frameHeight;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return true;
        }
    }
}