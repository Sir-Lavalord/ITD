using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;

namespace ITD.Particles.MetaBalls
{
    public class DebugMetaBall : Metaball
    {

        public override void SetDefaults()
        {
        }


        public override void Draw(SpriteBatch spriteBatch)
        {

            //spriteBatch.Draw(TextureAssets.Extra[91].Value, Position - Main.screenPosition, Color.White);

        }

        public override void SetStaticDefaults()
        {
            MetaballSystem.Sets.Color[Type] = Color.Cyan;
            MetaballSystem.Sets.OutlineColor[Type] = Color.Red;
            MetaballSystem.Sets.OutlineThickness[Type] = 10;


        }

    }
}
