using ITD.Utilities;

namespace ITD.PrimitiveDrawing;

public static class SimpleSquare
{
    private static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
    private static GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
    public static void Draw(Vector2 positions, Color colors = default, Vector2 size = default, float rotation = 0, Vector2 rotationCenter = default)
    {
        vertices[0].Position = (positions + new Vector2((float)-size.X * 0.5f, (float)-size.Y * 0.5f)).RotatedBy(rotation, rotationCenter).ToVector3();
        vertices[1].Position = (positions + new Vector2(size.X * 0.5f, (float)-size.Y * 0.5f)).RotatedBy(rotation, rotationCenter).ToVector3();
        vertices[2].Position = (positions + new Vector2((float)-size.X * 0.5f, size.Y * 0.5f)).RotatedBy(rotation, rotationCenter).ToVector3();
        vertices[3].Position = (positions + new Vector2(size.X * 0.5f, size.Y * 0.5f)).RotatedBy(rotation, rotationCenter).ToVector3();

        vertices[0].TextureCoordinate = Vector2.Zero;
        vertices[1].TextureCoordinate = new Vector2(1, 0);
        vertices[2].TextureCoordinate = new Vector2(0, 1);
        vertices[3].TextureCoordinate = Vector2.One;

        vertices[0].Color = colors;
        vertices[1].Color = colors;
        vertices[2].Color = colors;
        vertices[3].Color = colors;

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, (short[])[0, 1, 2, 3, 1, 2], 0, 2);
    }
}
