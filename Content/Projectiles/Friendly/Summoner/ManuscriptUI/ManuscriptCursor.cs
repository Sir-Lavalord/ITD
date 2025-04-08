using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Content.Buffs.MinionBuffs;

namespace ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI
{
    public class ManuscriptCursor : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16 * 4;
            Projectile.height = 16 * 4;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 999999;
            Projectile.extraUpdates = 20;
        }
        public Vector2 PlayerOffset = Vector2.Zero;

        public static Rectangle MouseRectangle => new((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, 2, 2);
        bool isOverlapping;
        public Color col;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.position = Main.MouseWorld - new Vector2(Projectile.width / 2, Projectile.height / 2);

            }
            RegisterLeftClick(player);
            Point mouseTiles = player.GetITDPlayer().MousePosition.ToTileCoordinates();
            if (TileHelpers.SolidTile(mouseTiles))
            {
                isOverlapping = true;
            }
            else
            {
                isOverlapping = false;
            }
            if (isOverlapping)
                col = Color.Red;
            else
                col = Color.White;


        }
        int type;
        int damage = 0;
        public void RegisterLeftClick(Player player)
        {
            if (Main.mouseLeft && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f && !isOverlapping && player.statMana >= 50)
            {
                switch (player.GetModPlayer<WaxwellPlayer>().codexMode)
                {
                    case 1:
                        player.CheckMana(50, true);
                        player.AddBuff(ModContent.BuffType<ManuscriptMinerBuff>(), 10);
                        type = ModContent.ProjectileType<ManuscriptMinerProj>();
                        break;
                    case 2:
                        player.CheckMana(50, true);
                        player.AddBuff(ModContent.BuffType<ManuscriptDuelistBuff>(), 10);
                        type = ModContent.ProjectileType<ManuscriptDuelistProj>();
                        damage = 34;
                        break;
                    case 3:
                        player.CheckMana(50, true);
                        player.AddBuff(ModContent.BuffType<ManuscriptLumberBuff>(), 10);
                        type = ModContent.ProjectileType<ManuscriptLumberProj>();
                        break;
                    case 4:
                        player.CheckMana(50, true);
                        type = ModContent.ProjectileType<ManuscriptSneakProj>();
                        break;
                }

                Projectile sneak = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                         Main.MouseWorld,
                         Vector2.Zero,
                         type,
                         damage,
                         0f,
                         player.whoAmI);
                Projectile.Kill();
                player.GetModPlayer<WaxwellPlayer>().codexMode = 0;
            }
        }
        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.GetModPlayer<WaxwellPlayer>().codexMode = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptCursor").Value;

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(col), Projectile.rotation, texture.Size() / 2, 1f, SpriteEffects.None, 0f);
            }

            return false;  
        }
    }
}