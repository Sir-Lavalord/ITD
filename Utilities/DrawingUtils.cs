using Terraria.Graphics.Shaders;

namespace ITD.Utilities;
public static class DrawingUtils
{
#pragma warning disable
    public static MiscShaderData TrailShader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);
#pragma warning restore
}
