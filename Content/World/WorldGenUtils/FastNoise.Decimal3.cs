//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using DECIMAL = System.Single;

namespace ITD.Content.World.WorldGenUtils;

public partial class FastNoise
{
    private readonly struct Decimal3(DECIMAL x, DECIMAL y, DECIMAL z)
    {
        public readonly DECIMAL x = x, y = y, z = z;
    }
}
