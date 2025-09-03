using Clamity.Content.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Clamity.Content.Bosses.Clamitas.Particles
{
    public class ClamitasIntroChromaticBurstParticle : ChromaticBurstParticle
    {
        public int npcIndex;
        public ClamitasIntroChromaticBurstParticle(int npc)
            : base(Vector2.Zero, Vector2.Zero, Color.Red, 20, 10f, 0f)
        {
            this.npcIndex = npc;
        }
        public override void Update()
        {
            base.Update();
            NPC npc = Main.npc[npcIndex];
            Position = npc.Center + Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * npc.width / 2;
        }
    }
}
