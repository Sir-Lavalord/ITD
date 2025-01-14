﻿using ITD.Systems.WorldNPCs;
using ITD.Utilities.Placeholders;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
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
        public override IEnumerable<SoundStyle> GetSpeechSounds()
        {
            yield return new SoundStyle(WorldNPCAssetsPath + "SpeechSounds/Mudkarp/bloop", new ReadOnlySpan<int>([0, 1, 2, 3, 4]));
        }
        public override int DialogueMusic => ITD.Instance.GetMusic("Mudkarp") ?? MusicID.SlimeRain;
    }
}
