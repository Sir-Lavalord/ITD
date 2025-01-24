using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.PrimitiveDrawing;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;
using Terraria.DataStructures;
using System.Collections.Generic;
namespace ITD.Content.Projectiles.Hostile
{
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




            // edit the MAX_TIMELEFT instead of this
            Projectile.timeLeft = 2;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Rotation + MathHelper.PiOver2;
            Projectile.timeLeft = MAX_TIMELEFT;
            UpdateLaserCollision();
            CurrentLasterLength = LasersLength;


        }
        private const int MAX_TIMELEFT = 800;
        public const int MAX_GOO = 300;
        private const int MAX_LASER_LENGTH = 3000;

        public List<CosmicGoos> cosmicGoos = new List<CosmicGoos>();
        public float CurrentHitboxesAmount = 0;
        private int laserWidth = 200;
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
        private bool LockIn => Projectile.localAI[0] != 0;
        private bool DoTileCollide => Projectile.localAI[1] != 0;


        int LasersLength = 0;

        public override void AI()
        {
            NPC npc = Main.npc[NPCOwner];
            Player player = Main.player[npc.target];

            Projectile.Center = npc.Center - new Vector2(0,15);
            // change the projetile rotation for adjusting the laser rotation
            if (!LockIn)
            {
                Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.AngleTo(player.Center), 0.01f);
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

                // goo/stream updating
/*                cosmicGoos.ForEach(g => g.timeleft--);
                cosmicGoos.RemoveAll(g => g.timeleft <= 0);
                SpawnACosmicGoo();
*/
                //uncomment this for normal laser collision behavouir
                CurrentLasterLength = LasersLength;
            
        }

        public void SpawnACosmicGoo()
        {
            CosmicGoos cosmicGoo = new CosmicGoos(new Rectangle((int)(Projectile.position.X + Projectile.velocity.X * CurrentLasterLength), (int)(Projectile.position.Y + Projectile.velocity.Y * CurrentLasterLength), Projectile.width, Projectile.height), 120, Main.rand.NextFloat(MathHelper.TwoPi));
            cosmicGoos.Add(cosmicGoo);
        }

        public void UpdateLaserCollision() 
        {
            LasersLength = 0;
            for (int i = 0; i < MAX_LASER_LENGTH; i++)
            {

                if (Collision.CanHitWithCheck(Projectile.Center, 15, 15, Projectile.Center + Projectile.velocity * LasersLength, 15, 15, (x, y) => { return Main.tile[x, y].TileType != TileID.Platforms; }))
                    LasersLength = i;
                else
                    break;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            foreach (CosmicGoos hitbox in cosmicGoos)
            {
                if (hitbox.hitbox.Intersects(targetHitbox))
                    return true;
            }


            float f = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * CurrentLasterLength, 22, ref f);
        }
        public override bool PreDraw(ref Color lightColor)
        {

            default(CosmicLaserVertex).Draw(Projectile.Center - Main.screenPosition,Projectile.rotation,new Vector2(Projectile.velocity.Length() * CurrentLasterLength, laserWidth));

            //default(CosmicGooVertex).Draw(Projectile.Center - Main.screenPosition + Projectile.velocity * CurrentLasterLength, MathHelper.ToRadians( Projectile.timeLeft), new Vector2(Projectile.width * 2, Projectile.height * 2), Projectile.timeLeft / 120f, (float)Projectile.timeLeft / 60);

            foreach (CosmicGoos goo in cosmicGoos)
            {
                default(CosmicGooVertex).Draw(goo.hitbox.Center.ToVector2() - Main.screenPosition, goo.rotation, new Vector2(Projectile.width, Projectile.height), MathHelper.Min((float)goo.timeleft / 120f,(float)Projectile.timeLeft / MAX_TIMELEFT), (float)goo.timeleft / 60f);
            }



            return false;
        }

        public override void OnKill(int timeLeft)
        {
            cosmicGoos.Clear();
        }

    }

    public class CosmicGoos
    {

        public CosmicGoos(Rectangle hitbox, int timeleft, float rotation) 
        {
            this.hitbox = hitbox;
            this.timeleft = timeleft;
            this.rotation = rotation;

        }

        public Rectangle hitbox;
        public int timeleft;
        public float rotation;

    }

    public struct CosmicGooVertex
    {

        private static SimpleSquare square = new SimpleSquare();

        public void Draw(Vector2 position, float rotation, Vector2 size, float timeleftPercentage, float randomOffset)
        {
            MiscShaderData shader = GameShaders.Misc["CosmicGoo"];

            shader.UseShaderSpecificData(new Vector4(timeleftPercentage, randomOffset, 0, 0)); //timeleftPercentage, random texture offset, none, none
            shader.UseImage0(TextureAssets.Extra[193]);
            shader.UseColor(Color.Beige);
            shader.UseSecondaryColor(new Color(192, 59, 166));
            shader.Apply();

            square.Draw(position, rotation: rotation, size: size * 2, rotationCenter: position);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }


    }
    public struct CosmicLaserVertex 
    {
    
        private static SimpleSquare square = new SimpleSquare();
        
        public void Draw(Vector2 position, float rotation, Vector2 size)
        {
            MiscShaderData shader = GameShaders.Misc["CosmicLaser"];

            //purple laser 
            shader.UseShaderSpecificData(new Vector4(size.X, 15, 0, 0)); //Laserlength, none, none, none
            shader.UseImage0(TextureAssets.Extra[193]);
            shader.UseColor(new Color(192,59,166));
            shader.Apply();

            square.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1.5f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);

            //beige laser 
            shader.UseShaderSpecificData(new Vector4(size.X, 15, 0, 0)); //Laserlength, none, none, none
            shader.UseImage1(TextureAssets.Extra[193]);
            shader.UseColor(Color.Beige);
            shader.Apply();

            square.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1,1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


    }

}
