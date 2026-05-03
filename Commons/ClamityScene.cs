using Clamity.Content.Bosses.Ihor.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Commons
{
    public class ClamityScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            return true;
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            bool flag = NPC.AnyNPCs(ModContent.NPCType<IhorHead>()) && IhorHead.BlizzardEffect;
            player.ManageSpecialBiomeVisuals("Blizzard", flag);
        }
    }
}
