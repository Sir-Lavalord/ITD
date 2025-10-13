using ITD.Content.Buffs.PetBuffs;
using ITD.Physics;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Pets;

public class PetPigPet : ModProjectile
{
    private readonly Asset<Texture2D> pigSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/PetPigPig");
    private readonly Asset<Texture2D> ropeSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/PetPigRope");
    int chestDetectTimer;
    private const int chestDetectCooldown = 360;
    private Chest chosenChest = null;
    private const int detectRadius = 30;
    readonly float lastDir;
    VerletChain pigChain;
    bool goToChosenChest = false;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 1;
        Main.projPet[Type] = true;
        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
            .WithOffset(-10, -20f)
            .WithSpriteDirection(-1)
            .WithCode(DelegateMethods.CharacterPreview.Float);
    }
    public override void SetDefaults()
    {
        Projectile.height = 24;
        Projectile.width = 24;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
    }
    public override void OnSpawn(IEntitySource source)
    {
        // create the verletChain
        Vector2 chainStart = Projectile.Center + Projectile.velocity + Vector2.UnitY * Projectile.height / 2;
        pigChain = PhysicsMethods.CreateVerletChain(8, 6, chainStart, chainStart + Vector2.One);
    }
    public override void OnKill(int timeLeft)
    {
        // destroy the verletChain at the same time as the projectile
        pigChain?.Kill();
    }
    public override void AI()
    {
        // get player. if player is alive and has the pet buff, keep the projectile alive by setting timeLeft to 2
        Player player = Main.player[Projectile.owner];
        if (!player.dead && player.HasBuff(ModContent.BuffType<PetPigBuff>()))
        {
            Projectile.timeLeft = 2;
        }
        // this timer will always go up
        chestDetectTimer++;
        //Main.NewText(chestDetectTimer);
        // the nearest chest will always be detected, but the pet will only go to it after the following if statement
        chosenChest = DetectChest(Projectile.Center.ToTileCoordinates(), detectRadius);
        // if the timer is more than the cooldown, that means it's time to go to the chest
        if (chestDetectTimer >= chestDetectCooldown)
        {
            if (chosenChest != null)
            {
                goToChosenChest = true;
                chestDetectTimer = -(60 * 60); // this makes the timer go down sixty seconds
            }
            else //if it can't find a chest, just reset the timer
            {
                goToChosenChest = false;
                chestDetectTimer = 0;
            }
        }
        if (chosenChest == null)
        {
            goToChosenChest = false;
        }
        else
        {
            for (int chestIndex = 0; chestIndex < Chest.maxItems; chestIndex++)
            {
                if (chosenChest.item[chestIndex] == null)
                {
                    chosenChest = null;
                    goToChosenChest = false;
                }
            }
        }
        DoFloating(player, goToChosenChest);
        // this line of code makes it so the rope is always at the bottom of the balloon properly
        pigChain?.UpdateStart(Projectile.Center + Projectile.velocity + Vector2.UnitY * (Projectile.height / 2));
    }
    private static Chest DetectChest(Point tile, int radius)
    {

        Rectangle rect = new(tile.X - radius, tile.Y - radius, radius + radius, radius + radius);
        static bool inCircle(int i, int j, Rectangle rect) // math things
        {
            Point rC = rect.Center;
            int xElem = i - rC.X;
            int yElem = j - rC.Y;
            int r = rect.Width / 2;
            return xElem * xElem + yElem * yElem <= r * r;
        }
        for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
        {
            Chest chest = Main.chest[chestIndex];
            if (chest == null)
            {
                continue;
            }
            if (inCircle(chest.x, chest.y, rect))
            {
                return chest;
            }
        }
        return null;
    }
    private void DoFloating(Player player, bool goToChest) // taken from snowpoff pet
    {
        Vector2 target = goToChest ? new Point(chosenChest.x, chosenChest.y).ToWorldCoordinates() : player.Center;
        Vector2 targetPoint = target + new Vector2(lastDir * 128f, -64f);
        Vector2 toPlayer = targetPoint - Projectile.Center;
        Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
        float speed = toPlayer.Length();
        //Projectile.direction = Projectile.spriteDirection = Math.Sign(lastDir);
        Projectile.velocity = toPlayerNormalized * (speed / 16f);
    }
    public override bool PreDraw(ref Color lightColor) // draw the verlet chain in predraw so it doesn't draw over the balloon
    {
        pigChain?.Draw(Main.spriteBatch, Main.screenPosition, ropeSprite.Value, Color.White, true, null, null, pigSprite.Value);
        return true;
    }
}
