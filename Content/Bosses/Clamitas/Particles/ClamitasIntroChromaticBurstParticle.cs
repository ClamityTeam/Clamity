using Clamity.Content.Particles;
using Microsoft.Xna.Framework;
using Terraria;

namespace Clamity.Content.Bosses.Clamitas.Particles
{
    public class ClamitasIntroChromaticBurstParticle : ChromaticBurstParticle
    {
        public int npcIndex;
        public ClamitasIntroChromaticBurstParticle(int npc, Vector2 pos)
            : base(pos, Vector2.Zero, Color.Red, 20, 10f, 0f)
        {
            this.npcIndex = npc;
        }
        public override void Update()
        {
            base.Update();
            NPC npc = Main.npc[npcIndex];
            Opacity = 1;
            //Position = npc.Center + Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * npc.width / 2;
            //Main.NewText(Position);
            //Main.NewText(npc.Center);
        }
    }
}
