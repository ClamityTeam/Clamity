using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorSnowflakeCreatingIcicle : BaseIhorIcicle
    {
        public const int MaxTimeleft = 200;
        public const int ChargingTime = 20;
        public NPC Ihor => Main.npc[(int)Projectile.ai[0]];
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = MaxTimeleft;
        }
        public int icicleTimer = 0;
        public override void BaseAI()
        {
            if (Projectile.timeLeft > MaxTimeleft - ChargingTime)
            {
                float roation = Ihor.rotation;
                Projectile.Center = Ihor.Center + Vector2.Lerp(Vector2.Zero, new Vector2(250 * Math.Abs(Projectile.ai[1]), 100 * Projectile.ai[1]), roation);
            }
            else
            {
                if (++icicleTimer > 10 + Main.rand.Next(3)) 
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextVector2Circular(1, 1), ModContent.ProjectileType<IhorSnowFlake>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    icicleTimer = 0;
                }
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(icicleTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            icicleTimer = reader.ReadInt32();
        }
    }
}
