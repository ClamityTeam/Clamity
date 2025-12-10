using CalamityMod;
using CalamityMod.Sounds;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.WoB.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.NPCs
{
    //[AutoloadBossHead]
    public class WallOfBronzeTorret : BaseWoBGunAI
    {
        public override void SetDefaults()
        {
            NPC.npcSlots = 5f;
            NPC.damage = 200;
            NPC.width = 172;
            NPC.height = 108;
            NPC.defense = 50;
            NPC.DR_NERD(0.2f);
            NPC.LifeMaxNERB(10000, 20000, 30000);
            NPC.lifeMax += (int)(NPC.lifeMax * CalamityServerConfig.Instance.BossHealthBoost * 0.01f);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.Opacity = 0.0f;
            NPC.knockBackResist = 0.0f;
            NPC.canGhostHeal = false;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.DeathSound = CommonCalamitySounds.ExoDeathSound;
            NPC.HitSound = SoundID.NPCHit4;
            //NPC.netUpdate = true;
            //NPC.netAlways = true;
            NPC.hide = true;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            if (Main.getGoodWorld)
                NPC.scale = 0.75f;
        }
        public override int MaxParticleTimer => 144;
        public override int MaxTimer => 500;
        public static readonly SoundStyle TelSound = new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/AresTeslaArmCharge")
        {
            Volume = 1.1f
        };
        public static readonly SoundStyle TeslaOrbShootSound = new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/TeslaShoot", 2);
        public override void Attack()
        {
            //AIState = 0;
            int num111 = (CalamityWorld.death ? 8 : (CalamityWorld.revenge ? 7 : (Main.expertMode ? 6 : 5)));

            Timer++;
            if (Timer % (30 / (num111 / 3)) == 0)
            {
                SoundEngine.PlaySound(in TeslaOrbShootSound, base.NPC.Center);
                Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                         NPC.Center,
                                         Vector2.UnitX.RotatedBy(NPC.rotation + Main.rand.NextFloat(-0.1f, 0.1f)) * 10,
                                         ModContent.ProjectileType<WallOfBronzeTorretBlast>(),
                                         NPC.GetProjectileDamageClamity(ModContent.ProjectileType<WallOfBronzeTorretBlast>()),
                                         0f,
                                         Main.myPlayer);

                float minValue = 1.8f;
                float maxValue = 2.8f;
                float num3 = 0.35f;
                for (int i = 0; i < 40; i++)
                {
                    Vector2 spinningpoint = new Vector2(Main.rand.NextFloat(minValue, maxValue), 0f).RotatedBy(NPC.rotation);
                    spinningpoint = spinningpoint.RotatedBy(0f - num3);
                    spinningpoint = spinningpoint.RotatedByRandom(2f * num3);
                    int num4 = ((Main.rand.NextBool(2)) ? 206 : DustID.GemAmethyst);
                    float num5 = ((num4 == 206) ? 1.5f : 1f);
                    int num6 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y) + Vector2.UnitX.RotatedBy(NPC.rotation) * 5, NPC.width / 6, NPC.height / 6, num4, spinningpoint.X, spinningpoint.Y, 200, default(Color), 2.5f * num5);
                    Main.dust[num6].position = NPC.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * NPC.width / 2f;
                    Main.dust[num6].noGravity = true;
                    Main.dust[num6].velocity *= 3f;
                    num6 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y) + Vector2.UnitX.RotatedBy(NPC.rotation) * 5, NPC.width / 6, NPC.height / 6, num4, spinningpoint.X, spinningpoint.Y, 100, default(Color), 1.5f * num5);
                    Main.dust[num6].position = NPC.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * NPC.width / 2f;
                    Main.dust[num6].velocity *= 2f;
                    Main.dust[num6].noGravity = true;
                    Main.dust[num6].fadeIn = 1f;
                    Main.dust[num6].color = Color.Cyan * 0.5f;
                }

                for (int j = 0; j < 20; j++)
                {
                    Vector2 spinningpoint2 = new Vector2(Main.rand.NextFloat(minValue, maxValue), 0f).RotatedBy(NPC.rotation);
                    spinningpoint2 = spinningpoint2.RotatedBy(0f - num3);
                    spinningpoint2 = spinningpoint2.RotatedByRandom(2f * num3);
                    int num7 = (Main.rand.NextBool(2) ? 206 : DustID.GemAmethyst);
                    float num8 = ((num7 == 206) ? 1.5f : 1f);
                    int num9 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y) + Vector2.UnitX.RotatedBy(NPC.rotation) * 5, NPC.width / 6, NPC.height / 6, num7, spinningpoint2.X, spinningpoint2.Y, 0, default(Color), 3f * num8);
                    Main.dust[num9].position = NPC.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(NPC.velocity.ToRotation()) * NPC.width / 3f;
                    Main.dust[num9].noGravity = true;
                    Main.dust[num9].velocity *= 0.5f;
                }
            }
            if (Timer >= 30 * 5)
            {
                Timer = 0;
                AIState = 0;
                //NPC.netUpdate = true;
            }
        }
        public override void AIinParticleState()
        {
            SoundEngine.PlaySound(TelSound, NPC.Center);
        }
        public override void ExtraAI()
        {
            NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();
        }
    }
}
