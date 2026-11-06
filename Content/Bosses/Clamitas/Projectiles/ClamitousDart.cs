using CalamityMod.Projectiles.Boss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clamity.Content.Bosses.Clamitas.Projectiles
{
    public class ClamitousDart : CalamitousDart
    {
        public override void AI()
        {
            base.AI();
            switch ((int)Projectile.ai[0]) 
            { 
                case 1:
                    Projectile.velocity.Y += 0.1f;
                    break;

                default:
                    break;
            }
        }
    }
}
