using ITD.Content.NPCs;
using ITD.Utilities.Placeholders;
using System.Collections.Generic;
using Terraria.Audio;

namespace ITD.Systems.WorldNPCs;

public struct SpeakerHeadDrawingData(Asset<Texture2D> texture, byte frameCount)
{
    public Asset<Texture2D> Texture = texture;
    public byte FrameCount = frameCount;
}
public abstract class WorldNPC : ITDNPC
{
    public const string WorldNPCAssetsPath = "ITD/Systems/WorldNPCs/Assets/";
    public override void OnRightClick(Player player)
    {
        player.ITD().TalkWorldNPC = NPC.whoAmI;
    }
    public virtual Asset<Texture2D> DialogueBoxStyle => ModContent.Request<Texture2D>(WorldNPCAssetsPath + "BoxStyles/DefaultBoxStyle");
    public virtual SpeakerHeadDrawingData DrawingData => new(ModContent.Request<Texture2D>(Placeholder.PHGeneric), 1);
    /// <summary>
    /// <para>Do NOT return a lot of different sounds here, it'd be terrible for performance.</para>
    /// <para>You should use one of the constructor overloads for SoundStyle that allows multiple variants instead.</para>
    /// <para>Variants for these overloads are detected automatically using the provided sound path and suffixing a number.</para>
    /// Example: ("SoundPath_", 3) would look for sounds named "SoundPath_0", "SoundPath_1", "SoundPath_2", and "SoundPath_3". 
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<SoundStyle> GetSpeechSounds()
    {
        yield return SoundID.MenuTick;
    }
    public virtual int DialogueMusic => MusicID.MenuMusic;
}
