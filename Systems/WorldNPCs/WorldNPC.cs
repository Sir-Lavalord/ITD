using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITD.Content.NPCs;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Systems.WorldNPCs
{
    public abstract class WorldNPC : ITDNPC
    {
        public override void OnRightClick(Player player)
        {
            player.GetITDPlayer().TalkWorldNPC = NPC.whoAmI;
        }
        public virtual Asset<Texture2D> DialogueBoxStyle => ModContent.Request<Texture2D>("ITD/Systems/WorldNPCs/DefaultDialogueBoxStyle");
    }
}
