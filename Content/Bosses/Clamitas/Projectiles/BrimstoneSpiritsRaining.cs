using Clamity.Content.Bosses.Clamitas.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas.Projectiles
{
    public class BrimstoneSpiritsRaining : BrimstoneSpirits
    {
        public NPC ClamitasOwner
        {
            get => Main.npc[(int)Projectile.ai[1]];
        }
        public override void AI()
        {
            base.AI();
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!ClamitasOwner.active)
                    Projectile.Kill();

                
                if (ClamitasOwner.ai[1] < ClamitasBoss.SpiritRain_Timing) //Fly Up
                {
                    Projectile.velocity.X *= 0.9f;
                }
                else if (ClamitasOwner.ai[1] == ClamitasBoss.SpiritRain_Timing) //Start Raining
                {
                    Projectile.Center = Main.player[ClamitasOwner.target].Center + new Microsoft.Xna.Framework.Vector2(0, -100) + Main.rand.NextVector2Square(100, 100);
                }
            }
        }

    }
}
