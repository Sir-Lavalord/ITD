using System;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace ITD.Content.NPCs;

public abstract class ITDNPC : ModNPC
{
    /// <summary>
    /// Override to do some custom drawing for this boss in Boss Checklist!
    /// </summary>
    public virtual Action<SpriteBatch, Rectangle, Color> DrawBossChecklistPortrait => null;
    /// <summary>
    /// Override and provide a <see cref="LocalizedText"/> that tells the player how to summon this boss. This can, indeed, have logic and be dynamic.
    /// </summary>
    public virtual LocalizedText HowToSummon => null;
    /// <summary>
    /// Override to define this boss's collectibles for BossChecklist! If you call the base impl of <see cref="ModNPC.ModifyNPCLoot(NPCLoot)"/>, these will additionally be automatically registered for the loot.
    /// </summary>
    public virtual IItemDropRule[] CollectibleRules => null;
    /// <summary>
    /// The placement of this boss in Boss Checklist. Leave as less than 0f and this NPC will not be registered. You NEED to also override <see cref="DownedMe"/> if you override this.
    /// </summary>
    public virtual float BossWeight => -1f;
    /// <summary>
    /// The (hypothetical) localization key for this ITDNPC's Bestiary entry.
    /// This will NOT be null if this NPC is <see cref="HiddenFromBestiary"/>.
    /// </summary>
    public string BestiaryEntryKey => this.GetLocalizationKey("Bestiary");
    public LocalizedText BestiaryEntry { get; private set; }
    public Rectangle HitboxTiles { get { return new Rectangle((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16), NPC.width / 16, NPC.height / 16); } }
    public Rectangle BigHitboxTiles => MiscHelpers.TileRectangle(NPC);
    public bool InvalidTarget { get { return NPC.target < 0 || NPC.target == 255 || !Main.player[NPC.target].Exists(); } }
    public SpriteEffects CommonSpriteDirection { get { return NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; } }
    public bool HiddenFromBestiary = false;
    public bool DontAutoRegisterBestiaryEntry = false;
    public sealed override void SetStaticDefaults()
    {
        SetStaticDefaultsSafe();
        if (HiddenFromBestiary)
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            return;
        }
        if (DontAutoRegisterBestiaryEntry)
            return;
        BestiaryEntry = Language.GetOrRegister(BestiaryEntryKey);
    }
    public virtual void SetStaticDefaultsSafe()
    {

    }
    public sealed override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        if (HiddenFromBestiary)
            return;

        if (!DontAutoRegisterBestiaryEntry)
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement(BestiaryEntryKey));

        SetBestiarySafe(database, bestiaryEntry);
    }
    public virtual void SetBestiarySafe(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {

    }
    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        var rules = CollectibleRules;
        if (CollectibleRules is null)
            return;
        foreach (IItemDropRule rule in rules)
        {
            npcLoot.Add(rule);
        }
    }
    /// <summary>
    /// Allows you to do stuff when this NPC is right clicked.
    /// </summary>
    /// <param name="player"></param>
    public virtual void OnRightClick(Player player)
    {

    }
    /// <summary>
    /// Use this for loops. Big flexibility
    /// </summary>
    /// <param name="frameHeight"></param>
    /// <param name="minFrame"></param>
    /// <param name="maxFrame">If left null, this will default to <see cref="Main.npcFrameCount"/> indexed with type, minus one</param>
    /// <param name="maxCounter"></param>
    /// <param name="counterIncrement"></param>
    public void CommonFrameLoop(int frameHeight, int minFrame = 0, int? maxFrame = null, float maxCounter = 5f, float counterIncrement = 1f)
    {
        // get the real max frame value
        int realMaxFrame = maxFrame ?? Main.npcFrameCount[Type] - 1;
        // clamp to min
        if (NPC.frame.Y < minFrame * frameHeight)
            NPC.frame.Y = minFrame * frameHeight;
        // increase counter by the increment
        NPC.frameCounter += counterIncrement;
        // change frames
        if (NPC.frameCounter > maxCounter)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y > frameHeight * realMaxFrame)
            {
                NPC.frame.Y = frameHeight * minFrame;
            }
        }
    }
    public void StepUp()
    {
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
    }
}
