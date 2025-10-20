using ITD.Utilities;
using System;

namespace ITD.Systems.DataStructures;

public struct SpriteBatchData
{
    // [static fields]

    private static readonly Func<SpriteBatch, SpriteSortMode> sortModeFieldAccessor;
    private static readonly Func<SpriteBatch, BlendState> blendStateFieldAccessor;
    private static readonly Func<SpriteBatch, SamplerState> samplerStateFieldAccessor;
    private static readonly Func<SpriteBatch, DepthStencilState> depthStencilStateFieldAccessor;
    private static readonly Func<SpriteBatch, RasterizerState> rasterizerStateFieldAccessor;
    private static readonly Func<SpriteBatch, Effect> effectFieldAccessor;
    private static readonly Func<SpriteBatch, Matrix> matrixFieldAccessor;

    // [fields]

    public SpriteSortMode SortMode;
    public BlendState BlendState;
    public SamplerState SamplerState;
    public DepthStencilState DepthStencilState;
    public RasterizerState RasterizerState;
    public Effect Effect;
    public Matrix Matrix;

    // [constructors]

    static SpriteBatchData()
    {
        sortModeFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, SpriteSortMode>("sortMode");
        blendStateFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, BlendState>("blendState");
        samplerStateFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, SamplerState>("samplerState");
        depthStencilStateFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, DepthStencilState>("depthStencilState");
        rasterizerStateFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, RasterizerState>("rasterizerState");
        effectFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, Effect>("customEffect");
        matrixFieldAccessor = TypeUtils.GetFieldAccessor<SpriteBatch, Matrix>("transformMatrix");
    }

    public SpriteBatchData(SpriteBatch spriteBatch)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);

        SortMode = sortModeFieldAccessor(spriteBatch);
        BlendState = blendStateFieldAccessor(spriteBatch);
        SamplerState = samplerStateFieldAccessor(spriteBatch);
        DepthStencilState = depthStencilStateFieldAccessor(spriteBatch);
        RasterizerState = rasterizerStateFieldAccessor(spriteBatch);
        Effect = effectFieldAccessor(spriteBatch);
        Matrix = matrixFieldAccessor(spriteBatch);
    }
}