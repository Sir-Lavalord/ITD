/*using System;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using System.IO;

namespace ITD.Shaders
{
    public class ShaderDebugSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                PrintAllShaders();
            }
        }

        private void PrintAllShaders()
        {
            string path = Path.Combine("", "shader_list.txt");
            using (StreamWriter writer = File.CreateText(path))
            {
                writer.WriteLine("=== shit code ===");
                foreach (var pair in GameShaders.Misc)
                {
                    writer.WriteLine(pair.Key);
                }
            }
        }
    }
}*/

//i'll spare you the trouble
/*=== Shaders ===
ForceField
WaterProcessor
WaterDistortionObject
WaterDebugDraw
HallowBoss
MaskedFade
QueenSlime
StardewValleyFade
RainbowTownSlime
MagicMissile
FlameLash
RainbowRod
FinalFractal
EmpressBlade
LightDisc
//Modded
Telegraph
CosmicLaserImpact
CosmicLaser
CosmicGoo
CosmicBall
Blackhole
//Fargo crap
FargowiltasSouls:KingSlime
FargowiltasSouls:QueenSlime
GaiaShader
WCWingShader
LCWingShader*/
