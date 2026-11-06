using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Enemy;
using Terraria;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Projectiles
{
    public class BrimstonePearlBurst : PearlBurst, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }
    }
}
