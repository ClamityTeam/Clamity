using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using Clamity.Content.Bosses.Clamitas.NPCs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas
{
    public class ClamitasMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int NPCType => ModContent.NPCType<ClamitasBoss>();
        public override int? MusicModMusic => Clamity.mod.GetMusicFromMusicMod("Clamitas");
        public override int VanillaMusic => MusicID.Boss3;
        public override int OtherworldMusic => MusicID.OtherworldlyBoss2;

        public override bool AdditionalCheck() => ClamityGlobalNPC.Clamitas != -1;
    }
}
