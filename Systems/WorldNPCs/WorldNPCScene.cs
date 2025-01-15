using ITD.Content.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Systems.WorldNPCs
{
    public class WorldNPCScene : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                string dialogue = UILoader.GetUIState<WorldNPCDialogue>().dialogueBox.godSpeaker;
                if (string.IsNullOrEmpty(dialogue))
                    return MusicID.MenuMusic;
                return (Mod.Find<ModNPC>(dialogue) as WorldNPC).DialogueMusic;
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        public override bool IsSceneEffectActive(Player player)
        {
            return UILoader.GetUIState<WorldNPCDialogue>().isOpen;
        }
    }
}
