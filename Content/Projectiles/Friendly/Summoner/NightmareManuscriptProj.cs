using ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ITD.Content.Items.Weapons.Summoner;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class NightmareManuscriptProj : ModProjectile
    {
        private int syncTimer;
        private Vector2 mousePos;
        public bool bookClosed = false;
        public bool bookFullyOpened = false;
        public Player player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 9;
        }
        public int currentSelect = 0;
        public override void SetDefaults()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.width = 76;
            Projectile.height = 38;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.netImportant = true;
        }

        public int timer;
        public float lerp = 0.12f;
        public override bool? CanDamage()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(mousePos.X);
            writer.Write(mousePos.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Vector2 buffer;
            buffer.X = reader.ReadSingle();
            buffer.Y = reader.ReadSingle();
            if (Projectile.owner != Main.myPlayer)
            {
                mousePos = buffer;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            var waxwell = player.GetModPlayer<WaxwellPlayer>();
            if (player.dead || !player.active)
                Projectile.Kill();
            Vector2 center = player.MountedCenter;
            center.Y -= 20;
            Projectile.Center = center;
            player.heldProj = Projectile.whoAmI;
                Projectile.frameCounter++;
            if (!bookClosed)
            {
                if (player.GetModPlayer<WaxwellPlayer>().codexMode == 0)
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<ManuscriptUIProj>()] <= 0 && bookFullyOpened && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
                    {
                        player.GetModPlayer<WaxwellPlayer>().codexClickCD = 60;
                        if (Main.myPlayer == player.whoAmI)
                        {

                            Projectile ui = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                                                     player.Center,
                                                     Vector2.Zero,
                                                     ModContent.ProjectileType<ManuscriptUIProj>(),
                                                     0,
                                                     0f,
                                                     player.whoAmI);
                            ui.localAI[0] = Projectile.whoAmI;
                            ui.netUpdate = true;
                        }
                    }
                }
                else
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<ManuscriptCursor>()] <= 0 && bookFullyOpened && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
                    {
                        if (Main.myPlayer == player.whoAmI)
                        {
                            player.GetModPlayer<WaxwellPlayer>().codexClickCD = 60;

                            Projectile cursor = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                                                     Main.MouseWorld,
                                                     Vector2.Zero,
                                                     ModContent.ProjectileType<ManuscriptCursor>(),
                                                     0,
                                                     0f,
                                                     player.whoAmI);
                            cursor.localAI[0] = Projectile.whoAmI;
                            cursor.netUpdate = true;
                        }
                    }
                }
                if (!player.GetModPlayer<WaxwellPlayer>().isholdingCodex)
                {
                    bookClosed = true;
                }
                if (Projectile.frame == 5)
                {
                    bookFullyOpened = true;
                }
                else bookFullyOpened = false;
                RegisterRightClick(player);
            }
            else bookFullyOpened = false;

            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                if (!bookClosed && waxwell.isholdingCodex)
                {
                    if (Projectile.frame >= 5)
                    {
                        Projectile.frame = 5;
                    }
                }
                else if (bookClosed || !waxwell.isholdingCodex)
                {
                    if (Projectile.frame > Main.projFrames[Projectile.type] - 1)
                        Projectile.Kill();
                }
                Projectile.frameCounter = 0;
            }

        }
        
        public void RegisterRightClick(Player player)
        {
            if (Main.mouseRight && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
            {
                if (!bookClosed)
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<ManuscriptUIProj>()] > 0)
                    {
                        foreach (Projectile p in Main.ActiveProjectiles)
                        {
                            if (p.type == ModContent.ProjectileType<ManuscriptUIProj>() &&
                                p.owner == player.whoAmI)
                            {
                                p.Kill();
                            }
                        }
                    }
                    bookClosed = true;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int height = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int width = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Width;
            int frame = height * Projectile.frame;
            SpriteEffects flipdirection = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rectangle Origin = new Rectangle(0, frame, width, height);
            Main.spriteBatch.Draw(texture2D, Projectile.Center - Main.screenPosition, Origin, lightColor, Projectile.rotation, new Vector2(width / 2, height / 2), Projectile.scale, flipdirection, 0);
            return false;
        }
    }
}