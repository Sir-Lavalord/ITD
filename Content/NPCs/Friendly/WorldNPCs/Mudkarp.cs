using ITD.Systems.WorldNPCs;
using ITD.Utilities.Placeholders;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Friendly.WorldNPCs
{
    public class Mudkarp : WorldNPC
    {
        public override string Texture => Placeholder.PHGeneric;
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 32;
        }
        public override SpeakerHeadDrawingData DrawingData => new(ModContent.Request<Texture2D>("ITD/Systems/WorldNPCs/Assets/SpeakerHeads/Mudkarp"), 1);
    }
}
