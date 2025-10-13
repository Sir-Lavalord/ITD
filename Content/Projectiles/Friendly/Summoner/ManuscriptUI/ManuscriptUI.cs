using ITD.Content.Items.Weapons.Summoner;
using ITD.Utilities;
using System.IO;

namespace ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI;

public class ManuscriptUIProj : ModProjectile
{
    public int FadeoutTime = -1;
    public Vector2 PlayerOffset = Vector2.Zero;
    public const int FadeoutTimeMax = 40;
    public static readonly Vector2 LeftBracketOffset = new(-8f, -6f);
    public static readonly Vector2 RightBracketOffset = new(8f, -6f);
    public static readonly Vector2 TopBracketOffset = new(2f, 27f);
    public ManuscriptUIProj CodexUmbra => Main.projectile[(int)Projectile.localAI[0]].ModProjectile<ManuscriptUIProj>();

    public Player player => Main.player[Projectile.owner];

    public override void SetDefaults()
    {
        Projectile.width = 62;
        Projectile.height = 58;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 36000;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(FadeoutTime);
        writer.Write(Projectile.localAI[0]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        FadeoutTime = reader.ReadInt32();
        Projectile.localAI[0] = reader.ReadSingle();
    }
    public override void AI()
    {
        // Death fade-out effect
        if (FadeoutTime > 0)
        {
            Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, 1f - FadeoutTime / (float)FadeoutTimeMax);
            FadeoutTime--;
        }
        else if (FadeoutTime == 0)
        {
            Projectile.Kill();
        }

        if (!player.GetModPlayer<WaxwellPlayer>().isholdingCodex ||
            !Main.projectile[(int)Projectile.localAI[0]].active)
        {
            Projectile.Kill();
            return;
        }

        Projectile.localAI[1]++;
        if (Projectile.localAI[1] < 40f)
        {
            Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.localAI[1] / 40f);
        }
        if (Main.mouseLeft && Main.mouseRight)
        {

        }
        if (Main.myPlayer == Projectile.owner)
        {
            if (PlayerOffset == Vector2.Zero)
            {
                PlayerOffset = Main.player[Projectile.owner].Center - Projectile.Center;
            }
            Projectile.Center = Main.player[Projectile.owner].Center - PlayerOffset;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.myPlayer == Projectile.owner)
        {

            DrawIcons(Main.spriteBatch);
        }
        return false;
    }
    public void DrawIcons(SpriteBatch spriteBatch)
    {
        DrawSneakIcon(spriteBatch);
        DrawLumberIcon(spriteBatch);
        DrawDuelistIcon(spriteBatch);
        DrawMinerIcon(spriteBatch);
    }
    public static Rectangle MouseRectangle => new((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, 2, 2);
    public void DrawMinerIcon(SpriteBatch spriteBatch)
    {
        Texture2D MinerIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptMiner").Value;
        Texture2D HoveredIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptMinerHover").Value;
        Texture2D textureToDraw = MinerIcon;
        Vector2 iconOffset = new(100f, -100f);
        Vector2 drawPosition = Projectile.Center + iconOffset;
        Rectangle iconFrame = Utils.CenteredRectangle(drawPosition, textureToDraw.Size());
        if (MouseRectangle.Intersects(iconFrame))
        {
            textureToDraw = HoveredIcon;
            Main.blockMouse = true;
            if (Main.mouseLeft && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f && (player.maxMinions - player.slotsMinions) >= 2)
            {
                Projectile.Kill();

                player.GetModPlayer<WaxwellPlayer>().codexMode = 1;

            }
        }

        Main.EntitySpriteDraw(textureToDraw,
                         drawPosition - Main.screenPosition,
                         null,
                         Color.White * Projectile.Opacity,
                         0f,
                         textureToDraw.Size() * 0.5f,
                         Projectile.scale,
                         SpriteEffects.None,
                         0);
    }
    public void DrawDuelistIcon(SpriteBatch spriteBatch)
    {
        Texture2D DuelistIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptDuelist").Value;
        Texture2D HoveredIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptMinerHover").Value;
        Texture2D textureToDraw = DuelistIcon;
        Vector2 iconOffset = new(-100, -100f);
        Vector2 drawPosition = Projectile.Center + iconOffset;
        Rectangle iconFrame = Utils.CenteredRectangle(drawPosition, textureToDraw.Size());
        if (MouseRectangle.Intersects(iconFrame))
        {
            textureToDraw = HoveredIcon;
            Main.blockMouse = true;
            if (Main.mouseLeft && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f && (player.maxMinions - player.slotsMinions) >= 1)
            {
                Projectile.Kill();

                player.GetModPlayer<WaxwellPlayer>().codexMode = 2;

            }
        }

        // And finally draw
        Main.EntitySpriteDraw(textureToDraw,
                         drawPosition - Main.screenPosition,
                         null,
                         Color.White * Projectile.Opacity,
                         0f,
                         textureToDraw.Size() * 0.5f,
                         Projectile.scale,
                         SpriteEffects.None,
                         0);
    }
    public void DrawLumberIcon(SpriteBatch spriteBatch)
    {
        Texture2D LumberIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptLumber").Value;
        Texture2D HoveredIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptMinerHover").Value;
        Texture2D textureToDraw = LumberIcon;
        Vector2 iconOffset = new(-100f, 100f);
        Vector2 drawPosition = Projectile.Center + iconOffset;
        Rectangle iconFrame = Utils.CenteredRectangle(drawPosition, textureToDraw.Size());
        if (MouseRectangle.Intersects(iconFrame))
        {
            textureToDraw = HoveredIcon;
            Main.blockMouse = true;
            if (Main.mouseLeft && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f && (player.maxMinions - player.slotsMinions) >= 2)
            {
                Projectile.Kill();

                player.GetModPlayer<WaxwellPlayer>().codexMode = 3;

            }
        }

        Main.EntitySpriteDraw(textureToDraw,
                         drawPosition - Main.screenPosition,
                         null,
                         Color.White * Projectile.Opacity,
                         0f,
                         textureToDraw.Size() * 0.5f,
                         Projectile.scale,
                         SpriteEffects.None,
                         0);
    }
    public void DrawSneakIcon(SpriteBatch spriteBatch)
    {
        Texture2D SneakIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptSneak").Value;
        Texture2D HoveredIcon = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/ManuscriptUI/ManuscriptMinerHover").Value;
        Texture2D textureToDraw = SneakIcon;
        Vector2 iconOffset = new(100f, 100f);
        Vector2 drawPosition = Projectile.Center + iconOffset;
        Rectangle iconFrame = Utils.CenteredRectangle(drawPosition, textureToDraw.Size());
        if (MouseRectangle.Intersects(iconFrame))
        {
            textureToDraw = HoveredIcon;
            Main.blockMouse = true;
            if (Main.mouseLeft && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
            {
                Projectile.Kill();

                player.GetModPlayer<WaxwellPlayer>().codexMode = 4;

            }
        }

        Main.EntitySpriteDraw(textureToDraw,
                         drawPosition - Main.screenPosition,
                         null,
                         Color.White * Projectile.Opacity,
                         0f,
                         textureToDraw.Size() * 0.5f,
                         Projectile.scale,
                         SpriteEffects.None,
                         0);
    }
}
