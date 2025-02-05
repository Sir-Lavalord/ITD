﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;

namespace ITD.Particles.MetaBalls
{
    public class DebugMetaBall : Metaball
    {

        public override void AI()
        {
            Size = MathHelper.Lerp(0, Size, (float)TimeLeft / (float)MaxTimeLeft);
        }

        public override void SetStaticDefaults()
        {
            MetaballSystem.Sets.MainColor[Type] = Color.Beige;
            MetaballSystem.Sets.OutlineColor[Type] = Color.BlueViolet;
            MetaballSystem.Sets.OutlineThickness[Type] = 3;
            MetaballSystem.Sets.MiscShader[Type] = GameShaders.Misc["CosmicBall"];
            MetaballSystem.Sets.Type[Type] = MetaballSystem.Sets.MetaballType.Both;
            MetaballSystem.Sets.Image1[Type] = TextureAssets.Extra[193];
            MetaballSystem.Sets.Image2[Type] = ModContent.Request<Texture2D>(Mod.Name + "/" + ITD.MiscShadersFolderPath + "CosmicBallOverlay");

        }

    }
}
