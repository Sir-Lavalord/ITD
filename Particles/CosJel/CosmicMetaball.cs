using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace ITD.Particles.CosJel
{
    public class CosmicMetaball : Metaball
    {

  
        public override void AI()
        {
        }

        public override void SetStaticDefaults()
        {
            MetaballSystem.Sets.MainColor[Type] = Color.Beige;
            MetaballSystem.Sets.OutlineColor[Type] = Color.BlueViolet;
            MetaballSystem.Sets.OutlineThickness[Type] = 3;
            MetaballSystem.Sets.MiscShader[Type] = GameShaders.Misc["CosmicBall"];
            MetaballSystem.Sets.MiscShader[Type].UseColor(Color.Purple);
            MetaballSystem.Sets.Type[Type] = MetaballSystem.Sets.MetaballType.Both;
            MetaballSystem.Sets.Image1[Type] = TextureAssets.Extra[193];
            MetaballSystem.Sets.Image2[Type] = ModContent.Request<Texture2D>(Mod.Name + "/" + ITD.MiscShadersFolderPath + "CosmicBallOverlay");

        }


    }
}
