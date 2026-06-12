using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Stupid
{
    public class Stupid : ModNPC
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.width = 164;
            NPC.height = 120;
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 14;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.value = 60f;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Zombie;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            for (int index1 = 0; index1 < 10; ++index1)
            {
                int index2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, Main.rand.Next(139, 143), 0.0f, 0.0f, 0, new Color(), 1f);
                Dust dust = Main.dust[index2];
                dust.velocity.X += (float)Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y += (float)Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= (float)(1.0 + (double)Main.rand.Next(-30, 31) * 0.0099999997764825821);
            }
        }
        public override void PostAI()
        {
            /*NPC.Calamity().newAI[0]++;
            Vector2 num1 = (Main.player[base.NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitX);
            int damage3 = NPC.damage;
            if (NPC.Calamity().newAI[0] % 100 == 0)
            {
                int type3 = ModContent.ProjectileType<BrimstoneHellblast>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, num1 * 8f, type3, damage3, 0f, Main.myPlayer);
            }
            if (NPC.Calamity().newAI[0] % 120 == 0)
            {
                int type3 = ModContent.ProjectileType<HolySpear>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, num1 * 8f, type3, damage3, 0f, Main.myPlayer);
            }
            if (NPC.Calamity().newAI[0] % 270 == 0)
            {
                int type3 = ModContent.ProjectileType<HolyFlare>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX * 8f, type3, damage3, 0f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitX * 8f, type3, damage3, 0f, Main.myPlayer);
            }
            if (NPC.Calamity().newAI[0] % 240 == 0)
            {
                int type3 = ModContent.ProjectileType<UnstableCrimulanGlob>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, num1.RotatedBy(0) * 5f, type3, damage3, 0f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, num1.RotatedBy(MathF.PI * 2 / 3) * 5f, type3, damage3, 0f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, num1.RotatedBy(MathF.PI * 4 / 3) * 5f, type3, damage3, 0f, Main.myPlayer);
            }
            if (NPC.Calamity().newAI[0] % 160 == 0)
            {
                int type3 = ModContent.ProjectileType<IceBomb>();
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), Main.player[base.NPC.target].Center + Vector2.One.RotatedBy(MathF.PI / 4 * i) * 8f, Vector2.Zero, type3, damage3, 0f, Main.myPlayer);
                }
            }*/
        }
    }
}
