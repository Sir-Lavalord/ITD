using ITD.Content.Dusts;
using ITD.Particles;
using ITD.Particles.CosJel;
using ITD.PrimitiveDrawing;
using ITD.Systems;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Shaders;
namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicRay : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
    }


    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.width = Projectile.height = 128;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = 800;
    }

    public override void OnSpawn(IEntitySource source)
    {

        Projectile.rotation = Rotation + MathHelper.PiOver2;
        UpdateLaserCollision();
        CurrentLasterLength = LasersLength;

    }
    public const int MAXGOO = 60;
    public const int MAXLASERLENGTH = 3000;
    public const int SWEEPTIME = 180;


    public List<CosmicGoos> cosmicGoos = [];
    public float CurrentHitboxesAmount = 0;
    private readonly int laserWidth = 200;
    private int NPCOwner
    {
        get { return (int)Projectile.ai[0]; }
        set { Projectile.ai[0] = value; }
    }
    private int CurrentLasterLength
    {
        get { return (int)Projectile.ai[2]; }
        set { Projectile.ai[2] = value; }
    }
    private float Rotation
    {
        get { return Projectile.ai[1]; }
        set { Projectile.ai[1] = value; }
    }
    private float sweepDir => Projectile.localAI[0];
    // 0 = no collision, 1 = tile collision only, 2 = tile and platform collisions.
    private ref float CollisionType => ref Projectile.localAI[1];
    // private int max_timeleft;

    int LasersLength = 0;
    bool spawnAnim = false;
    public override void AI()
    {
        NPC npc = Main.npc[NPCOwner];
        if (npc == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            Projectile.Kill();
            return;
        }
        Player player = Main.player[npc.target];
        player.GetITDPlayer().BetterScreenshake(20, 5, 5, true);
        Projectile.Center = npc.Center + new Vector2(0, -60);
        // change the projetile rotation for adjusting the laser rotation
        if (!spawnAnim)
        {                //from clamtea
            float dustLoopCheck = 24f;
            int dustIncr = 0;
            while (dustIncr < dustLoopCheck)
            {
                Vector2 dustRotate = Vector2.UnitX * 0f;
                dustRotate += -Vector2.UnitY.RotatedBy((double)(dustIncr * (MathHelper.TwoPi / dustLoopCheck)), default) * new Vector2(2f, 8f);
                dustRotate = dustRotate.RotatedBy((double)Projectile.rotation, default);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, 0f, 0f, 0, default, 2f);
                dust.noGravity = true;
                dust.scale = 2f;
                dust.position = Projectile.Center + dustRotate;
                dust.velocity = Projectile.velocity * 0f + dustRotate.SafeNormalize(Vector2.UnitY) * 2f;
                dustIncr++;
            }
            spawnAnim = true;

        }
        Projectile.rotation -= ((float)Math.PI / SWEEPTIME) * sweepDir;
        if (slopTimer++>SWEEPTIME * 2)
        {
            Projectile.Kill();
        }
        if (slopTimer % 15 == 0)
        {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
            {
                PositionInWorld = Projectile.Center,

            }, Projectile.whoAmI);
        }
        Projectile.velocity = Projectile.rotation.ToRotationVector2();
        UpdateLaserCollision();

        //update current laser length slowly, if you dont want that, just uncomment the comment at the end of the AI hook
        if (LasersLength > CurrentLasterLength)
        {
            CurrentLasterLength += 25;
            if (CurrentLasterLength > LasersLength)
                CurrentLasterLength = LasersLength;

        }
        else
        {
            CurrentLasterLength -= 25;
            if (CurrentLasterLength < LasersLength)
                CurrentLasterLength = LasersLength;
        }

        //goo / stream updating
        for (int i = cosmicGoos.Count - 1; i >= 0; i--)
        {
            int timeLeft = cosmicGoos[i].Timeleft - 1;
            if (timeLeft <= 0)
            {
                cosmicGoos.RemoveAt(i);
                continue;
            }
            cosmicGoos[i] = cosmicGoos[i] with { Timeleft = timeLeft };
        }
        if (Projectile.timeLeft % 5 == 0)
            SpawnACosmicGoo();

        //uncomment this for normal laser collision behavouir
        CurrentLasterLength = LasersLength;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile other = Main.projectile[i];
            float f = 0;

            if (other.type == ModContent.ProjectileType<CosmicRayMeteorite>() && other.active && other.timeLeft > 0
                && Collision.CheckAABBvLineCollision(other.TopLeft, other.Size, Projectile.Center, Projectile.Center + Projectile.velocity * CurrentLasterLength, 22, ref f))
            {

                Projectile.scale += 0.2f;
                player.GetModPlayer<ITDPlayer>().BetterScreenshake(4, 5, 2, true);
                other.ai[1] = (360 - slopTimer) /2;
                other.Kill();
                for (int j = 0; j < 20; j++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
                    dust.noGravity = true;
                    dust.velocity = new Vector2(0, -20).RotatedByRandom(MathHelper.ToRadians(10));

                }

            }
        }

    }

    int slopTimer;
    public void SpawnACosmicGoo()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
        }
        CosmicGoos cosmicGoo = new(new Rectangle((int)(Projectile.position.X + Projectile.velocity.X * CurrentLasterLength), (int)(Projectile.position.Y + Projectile.velocity.Y * CurrentLasterLength), Projectile.width, Projectile.height), 120, Main.rand.NextFloat(MathHelper.TwoPi));
        cosmicGoos.Add(cosmicGoo);
        Metaball.NewMetaball<CosmicMetaball>(Projectile.Center + Projectile.velocity * CurrentLasterLength, Main.rand.NextVector2CircularEdge(0.25f, 0.25f), 0.035f, 120);

    }

    public void UpdateLaserCollision()
    {
        Vector2 playerCenter = Main.player[Main.npc[NPCOwner].target].Center;
        if (CollisionType == 0)
        {
            LasersLength = MAXLASERLENGTH;
            return;
        }

        RaycastData data = Helpers.QuickRaycast(Projectile.Center, Projectile.velocity, (point) => { return playerCenter.Y >= point.ToWorldCoordinates().Y + 20 && CollisionType == 2 && !Main.tile[point].IsActuated || Main.tile[point].HasTile && CollisionType == 1 && Main.tile[point].TileType == TileID.Platforms; }, MAXLASERLENGTH);
        LasersLength = (int)data.Length;

    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (CosmicGoos hitbox in cosmicGoos)
        {
            if (hitbox.Hitbox.Intersects(targetHitbox))
                return true;
        }
        float f = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * CurrentLasterLength, 22, ref f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        CosmicLaserImpactVertex.Draw(Projectile.Center - Main.screenPosition + Projectile.velocity * CurrentLasterLength, 0, new Vector2(Projectile.width * 1f, Projectile.height * 0.25f), Projectile.timeLeft / 120f, (float)Projectile.timeLeft / 60);
        CosmicLaserVertex.Draw(Projectile.Center - Main.screenPosition, Projectile.rotation, new Vector2(Projectile.velocity.Length() * CurrentLasterLength, laserWidth));
        //foreach (CosmicGoos goo in cosmicGoos)
        //{
        //    default(CosmicGooVertex).Draw(goo.hitbox.Center.ToVector2() - Main.screenPosition, goo.rotation, new Vector2(Projectile.width, Projectile.height), MathHelper.Min((float)goo.timeleft / 120f,(float)Projectile.timeLeft / max_timeleft), (float)goo.timeleft / 60f);
        //}



        return false;
    }

    public override void OnKill(int timeLeft)
    {
        cosmicGoos.Clear();
    }

}
public readonly record struct CosmicGoos(Rectangle Hitbox, int Timeleft, float Rotation);
public readonly struct CosmicGooVertex
{
    public static void Draw(Vector2 position, float rotation, Vector2 size, float timeleftPercentage, float randomOffset)
    {
        GameShaders.Misc["CosmicGoo"]
            .UseShaderSpecificData(new Vector4(timeleftPercentage, randomOffset, 0, 0))
            .UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion])
            .UseColor(Color.Beige)
            .UseSecondaryColor(new Color(192, 59, 166))
            .Apply();

        SimpleSquare.Draw(position, rotation: rotation, size: size * 0.5f, rotationCenter: position);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }
}
public readonly struct CosmicLaserImpactVertex
{
    public static void Draw(Vector2 position, float rotation, Vector2 size, float timeleftPercentage, float randomOffset)
    {
        MiscShaderData shader = GameShaders.Misc["CosmicLaserImpact"];

        shader.UseShaderSpecificData(new Vector4(timeleftPercentage, randomOffset, 0, 0)) //timeleftPercentage, random texture offset, none, none
              .UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion])
              .UseColor(Color.Beige)
              .UseSecondaryColor(new Color(192, 59, 166))
              .Apply();

        SimpleSquare.Draw(position, rotation: rotation, size: size * 2, rotationCenter: position);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }
}

public readonly struct CosmicLaserVertex
{
    public static void Draw(Vector2 position, float rotation, Vector2 size)
    {
        MiscShaderData shader = GameShaders.Misc["CosmicLaser"];
        //beige laser 
        shader
            .UseColor(new Color(248, 160, 121, 50))
            .Apply();

        SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1.25f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);
        //purple laser 
        shader
            .UseShaderSpecificData(new Vector4(size.X, 0.5f, 0, 0)) //Laserlength, Flow speed, none, none
            .UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion])
            .UseColor(new Color(255, 255, 255, 50))
            .Apply();

        SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);

        //beige laser 
        shader
            .UseColor(new Color(15, 13, 59, 50))
            .Apply();

        SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 0.75f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }
}

public readonly struct CosmicLaserMiniVertex
{
    public static void Draw(Vector2 position, float rotation, Vector2 size)
    {
        MiscShaderData shader = GameShaders.Misc["CosmicLaser"];

        //purple laser 
        shader.UseShaderSpecificData(new Vector4(size.X, 0.5f, 0, 0)) //Laserlength, Flow speed, none, none
              .UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion])
              .UseColor(new Color(192, 59, 166))
              .Apply();

        SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);

        //beige laser 
        //shader.UseShaderSpecificData(new Vector4((size.X), 0.5f, 0, 0)); //Laserlength, Flow speed, none, none
        //shader.UseImage1(TextureAssets.Extra[193]);
        //shader.UseColor(Color.Beige);
        //shader.Apply();

        //square.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }


}
