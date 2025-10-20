using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace ITD.Content.World;

public class WorldGenSystem : ModSystem
{
    public static LocalizedText BluesoilPassMessage { get; private set; }
    public static LocalizedText DeepDesertPassMessage { get; private set; }
    public static LocalizedText WorldNPCsPassMessage { get; private set; }

    public static List<Point> testPoints = [];
    internal static List<ITDGenpass> genpassesTemp = []; // is populated automagically in ITDGenpass.Load()
    public static ITDGenpass[] Genpasses { get; private set; }
    public override void Load()
    {

    }
    public override void SetStaticDefaults()
    {
        // flatten
        Genpasses = [.. genpassesTemp];

        // dispose
        genpassesTemp.Clear();
        genpassesTemp = null;

        for (int i = 0; i < Genpasses.Length; i++)
        {
            ITDGenpass pass = Genpasses[i];
            pass.PassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{pass.GetType().Name}Message")); // pass.Name isn't used because it has spaces
        }
    }
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        for (int i = 0; i < Genpasses.Length; i++)
        {
            ITDGenpass pass = Genpasses[i];
            GenpassOrder orderer = pass.Order;

            SmartGenpass genpass = new(pass);

            int indexOffset = 0;
            switch (orderer.Type)
            {
                case GenpassOrderType.Before:
                    int taskIndex = tasks.FindIndex(g => g.Name == orderer.Find);
                    tasks.Insert(taskIndex + indexOffset, genpass);
                    break;
                case GenpassOrderType.After:
                    indexOffset = 1;
                    goto case GenpassOrderType.Before;
                case GenpassOrderType.BeforeEverything:
                    tasks.Insert(0, genpass);
                    break;
                case GenpassOrderType.AfterEverything:
                    tasks.Add(genpass);
                    break;
            }
        }
    }
    public static bool JustPressed(Keys key)
    {
        return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
    }

    public override void PostUpdateWorld()
    {
        if (Main.keyState.IsKeyDown(Keys.D1))
        {
            TestConstantMethod();
        }
        if (JustPressed(Keys.D1))
        {
            TestMethod();
        }
        if (JustPressed(Keys.D2))
        {
            TestMethod2();
        }
    }
    private static void TestConstantMethod()
    {
        /*
        Player p = Main.LocalPlayer;
        RaycastData d = Helpers.QuickRaycast(p.Center, p.DirectionTo(Main.MouseWorld), visualize: true);
        Dust.NewDustPerfect(d.End, DustID.Torch, Vector2.Zero);
        */
    }
    private static void TestMethod()
    {
        /*
        int allContent = Mod.GetContent().Count();
        int items = Mod.GetContent<ModItem>().Count();
        int NPCs = Mod.GetContent<ModNPC>().Count();
        int projectiles = Mod.GetContent<ModProjectile>().Count();
        int buffs = Mod.GetContent<ModBuff>().Count();
        int prefixes = Mod.GetContent<ModPrefix>().Count();
        int rarities = Mod.GetContent<ModRarity>().Count();
        int dusts = Mod.GetContent<ModDust>().Count();
        int gores = Mod.GetContent<ModGore>().Count();
        int particles = Mod.GetContent<ParticleEmitter>().Count();
        int tiles = Mod.GetContent<ModTile>().Count();
        int walls = Mod.GetContent<ModWall>().Count();
        int genpasses = Mod.GetContent<ITDGenpass>().Count();
        int players = Mod.GetContent<ModPlayer>().Count();
        int tileEntities = Mod.GetContent<ModTileEntity>().Count();
        int globalProjs = Mod.GetContent<GlobalProjectile>().Count();
        int globalNPCs = Mod.GetContent<GlobalNPC>().Count();
        int globalItems = Mod.GetContent<GlobalItem>().Count();
        int globalTiles = Mod.GetContent<GlobalTile>().Count();
        int globalWalls = Mod.GetContent<GlobalWall>().Count();
        int biomesSceneEffects = Mod.GetContent<ModSceneEffect>().Count();
        int surfaceBackgrounds = Mod.GetContent<ModSurfaceBackgroundStyle>().Count();
        int undergroundBackgrounds = Mod.GetContent<ModBackgroundStyle>().Count() - surfaceBackgrounds;
        int menus = Mod.GetContent<ModMenu>().Count();
        Console.WriteLine
        ($"""
        Everything: {allContent}
        Items: {items}
        NPCs: {NPCs}
        Projectiles: {projectiles}
        Buffs: {buffs}
        Prefixes: {prefixes}
        Rarities: {rarities}
        Tiles: {tiles}
        Walls: {walls}
        Tile Entities: {tileEntities}
        Dusts: {dusts}
        Gores: {gores}
        Particles: {particles}
        Scenes and Biomes: {biomesSceneEffects}
        Surface Backgrounds: {surfaceBackgrounds}
        Cave Backgrounds: {undergroundBackgrounds}
        Generation Passes: {genpasses}
        Player Data Modules: {players}
        Projectile Data Modules: {globalProjs}
        NPC Data Modules: {globalNPCs}
        Item Data Modules: {globalItems}
        Tile Data Modules: {globalTiles}
        Wall Data Modules: {globalWalls}
        Main Menu Styles: {menus}
        """);
        */
        // test UI particle (change particle.canvas in particle type)
        //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<TestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
        //test world particle (change particle.canvas in particle type)
        //ITDParticle newParticle = ParticleSystem.NewEmitter<TestParticle>(Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f, 0f);
        // Point pos = Main.MouseWorld.ToTileCoordinates();

        //ParticleSystem.NewSingleParticle<LyteflyParticle>(Main.MouseWorld, Vector2.Zero, lifetime: 60);
        //ParticleSystem.NewSingleParticle<PyroclasticParticle>(Main.MouseWorld, Vector2.Zero, lifetime: 120, canvas: ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        /*
        testPoints.Add(pos);
        if (testPoints.Count == 1)
            EventsSystem.BeginEvent<LavaRainEvent>();
        else
        {
            EventsSystem.StopEvent<LavaRainEvent>();
            testPoints.Clear();
        }
        */
        /*
        if (TileHelpers.TileLiquid(pos, LiquidID.Lava))
        {
            LiquidPoolData p = MiscHelpers.ComputeLiquidPool(pos, LiquidID.Lava, true);
            Projectile.NewProjectile(Main.npc[0].GetSource_FromThis(), p.CenterAverage, -Vector2.UnitY * 4f, ModContent.ProjectileType<PyroclasticFireball>(), 30, 0f);
        }
        */
        /*
        Tile t = Framing.GetTileSafely(pos);
        t.TileType = (ushort)ModContent.TileType<ReinforcedPegmatiteBricks>();
        t.HasTile = true;
        */
        /*
        testPoints.Add(pos);
        if (testPoints.Count == 1)
            UILoader.GetUIState<WorldNPCDialogue>().Open("Mudkarp.FirstTimeSpeaking");
        else
        {
            UILoader.GetUIState<WorldNPCDialogue>().Close();
            testPoints.Clear();
        }
        */

        /*
        testPoints.Add(pos);
        if (testPoints.Count == 2)
        {
            Point p1 = testPoints[0];
            Point p2 = testPoints[1];
            ITDShapes.Ellipse ellipse = new(p1.X, p1.Y, Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
            ellipse.LoopThroughPoints(p =>
            {
                Dust d = Dust.NewDustDirect(p.ToWorldCoordinates(), 1, 1, DustID.Torch);
                d.noGravity = true;
            });
            Point rand = ellipse.RandomPoint(WorldGen.genRand);
            Dust.DrawDebugBox(new Rectangle(rand.X * 16, rand.Y * 16, 16, 16));
            testPoints.Clear();
        }
        */
        /*
        testPoints.Add(pos);
        if (testPoints.Count == 2)
        {
            Point p1 = testPoints[0];
            Point p2 = testPoints[1];
            Rectangle rect = MiscHelpers.ContainsRectangles(new Rectangle(p1.X, p1.Y, 1, 1), new Rectangle(p2.X, p2.Y, 1, 1));
            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    Framing.GetTileSafely(i, j).WallType = (ushort)ModContent.WallType<ReinforcedPegmatiteBrickWallUnsafe>();
                    WorldGen.SquareWallFrame(i, j);
                }
            }
            testPoints.Clear();
        }
        */
        /*
        Tile t = Framing.GetTileSafely(pos);
        t.HasTile = true;
        t.TileType = (ushort)ModContent.TileType<ReinforcedPegmatiteBricks>();
        WorldGen.TileFrame(pos.X, pos.Y);
        */
        //WorldGenHelpers.Procedural.DigQuadTunnel(pos, pos + new Point(100, 50), 5, 9, 3);
        /*
        Point dirSize = new(
                WorldGen.genRand.Next(35, 50) * (1),
                -1
                );
        Rectangle rect = WorldGenHelpers.Procedural.DigQuadTunnel(pos, pos + dirSize, 5, 8, 2);
        Rectangle newRect = new(rect.X * 16, rect.Y * 16, rect.Width * 16, rect.Height * 16);
        Dust.DrawDebugBox(newRect);
        WorldGenHelpers.QuickDebugRectangle(rect);
        */
        /*
        testPoints.Add(pos);
        if (testPoints.Count == 2)
        {
            Point origin = testPoints[0];
            Vector2 sizeDir = (testPoints[1] - testPoints[0]).ToVector2();
            //WorldGenHelpers.Procedural.DigDirectionQuad(origin, sizeDir, 6, 6, 4, true);
            WorldGenHelpers.Procedural.DigQuadTunnel(origin, sizeDir, 6, 3, 3);
            testPoints.Clear();
        }
        */
        /*
        testPoints.Add(pos);
        if (testPoints.Count == 2)
        {
            ITDShapes.Parabola par = new(testPoints[0].X, testPoints[0].Y, testPoints[1].X, testPoints[1].Y, 50);
            
            //par.LoopThroughPoints(p =>
            //{
            //    WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
            //});
            
            double tightness = 0.9d;
            ITDShapes.Banana ban = new(par, testPoints[1].Y, tightness);
            ban.LoopThroughPoints(p =>
            {
                WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
            });
            testPoints.Clear();
        }
        */
        /* test triangle creation
        testPoints.Add(pos);
        if (testPoints.Count == 3)
        {
            ITDShapes.Triangle tri = new(testPoints[0], testPoints[1], testPoints[2]);
            tri.LoopThroughPoints(p =>
            {
                WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
            });
            testPoints.Clear();
        }
        */
        //Helpers.GrowLongMoss(pos.X, pos.Y, ModContent.TileType<LongBlackMold>(), ModContent.TileType<BlackMold>());
        //WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<BlackMold>());
    }
    private static void TestMethod2()
    {
        //EventsSystem.StopEvent<TestChaoticEvent>();
        // test UI particle (change particle.canvas in particle type)
        //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
        // test world particle (change particle.canvas in particle type)
        //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
    }
}
