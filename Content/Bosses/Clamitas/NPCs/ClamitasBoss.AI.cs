using CalamityMod;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.Clamitas.Particles;
using Clamity.Content.Bosses.Clamitas.Projectiles;
using Clamity.Content.Particles;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas.NPCs
{
    public enum ClamitasAttacks : int
    {
        StartingCutscene = 0,

        SpiritRain,
        SidefallTeleports,
        Brimfall,
        CrossSpirits,
        SpiritWave,
        RingOfSpirits,

        DespirationPhase,
    }
    public partial class ClamitasBoss : ModNPC
    {
        private int attack = 1;
        public ClamitasAttacks CurrentAttack
        {
            get => (ClamitasAttacks)attack;
            set => attack = (int)value;
        }
        public int AttackTimer
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        //public bool BattleIsStarted => CurrentAttack != ClamitasAttacks.PreFight && (CurrentAttack != ClamitasAttacks.StartingCutscene);
        public Player player => Main.player[NPC.target];
        public static int SpiritRain_Timing => 300;
        public override void AI()
        {
            Myself = NPC;
            //NPC.TargetClosest();

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 3200f)
            {
                NPC.TargetClosest();
            }

            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            Player player = Main.player[NPC.target];
            bool bossRushActive = BossRushEvent.BossRushActive;
            bool flag = Main.expertMode || bossRushActive;
            bool flag2 = CalamityWorld.revenge || bossRushActive;
            bool flag3 = CalamityWorld.death || bossRushActive;
            if (!player.ZoneDesert && !bossRushActive)
            {
                if (biomeEnrageTimer > 0)
                {
                    biomeEnrageTimer--;
                }
            }
            else
            {
                biomeEnrageTimer = 300;
            }

            bool num = biomeEnrageTimer <= 0 || bossRushActive;
            float num2 = flag3 ? 0.5f : 0f;
            if (num)
            {
                NPC.Calamity().CurrentlyEnraged = !bossRushActive;
                num2 += 2f;
            }

            //Starts Main AI


            if (NPC.life < 2)
            {
                //TODO - Later Despriration Phase
                NPC.active = false;
                NPC.netUpdate = true;
                NPC.NPCLoot();
            }

            if (!Main.dedServ)
            {
                if (!Main.player[NPC.target].dead && Main.player[NPC.target].active)
                {
                    player.AddBuff(ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.Clamity>(), 2);
                    player.AddBuff(ModContent.BuffType<BossEffects>(), 2);
                }
            }
            /*if (Main.player[NPC.target].dead && !Main.player[NPC.target].active)
            {
                NPC.active = false;
            }*/

            if (!hide)
            {
                Lighting.AddLight(NPC.Center, 0.75f, 0, 0);
            }

            //Main.NewText($"{attack} - {AttackTimer}");

            //Handle Music


                
            ClamityGlobalNPC.Clamitas = NPC.whoAmI;
            AttackTimer++;
            switch (CurrentAttack)
            {
                case ClamitasAttacks.StartingCutscene:
                    AI_StartingCutsceneState();
                    break;

                case ClamitasAttacks.SpiritRain:
                    AI_SpiritRain();
                    break;
                case ClamitasAttacks.SidefallTeleports:
                    AI_SidefallTeleports();
                    break;
                case ClamitasAttacks.Brimfall:
                    AI_Brimfall();
                    break;
                case ClamitasAttacks.CrossSpirits:
                    AI_CrossSpirits();
                    break;
                case ClamitasAttacks.SpiritWave:
                    AI_SpiritWave();
                    break;
                default:
                    attack = 0;
                    break;

            }
            //Main.NewText(Main.GlobalTimeWrappedHourly);
            
        }
        private ClamitasAttacks SetNextAttack(ClamitasAttacks nextAttack)
        {
            CurrentAttack = nextAttack;
            AttackTimer = 0;
            NPC.ai[1] = 0;
            NPC.ai[2] = 0;
            NPC.Calamity().newAI[0] = 0;
            NPC.Calamity().newAI[1] = 0;
            NPC.Calamity().newAI[2] = 0;
            NPC.Calamity().newAI[3] = 0;
            NPC.netUpdate = true;
            return nextAttack;
        }
        /*private void AttackIThink_PreFight()
        {
            //Music = -1;

            if (NPC.justHit)
            {
                ++hitAmount;
                hasBeenHit = true;
            }

            //NPC.life = NPC.lifeMax;

            if (hitAmount >= 10)
            {
                SetNextAttack(ClamitasAttacks.StartingCutscene);
            }
        }*/
        private void AI_StartingCutsceneState()
        {
            hide = true;
            int animTime = 600;

            if (AttackTimer == 1)
            {                
                //NPC.chaseable = true;
                NPC.damage = 0;
                NPC.Center = player.Center;
            }

            NPC.ai[1]++;
            if (NPC.ai[1] > Utils.Remap(AttackTimer, 0, animTime / 2, 60, 10))
            {
                //ClamitasIntroChromaticBurstParticle particle1 = new ClamitasIntroChromaticBurstParticle(NPC.whoAmI, NPC.Center);

                ChromaticBurstParticleEasing particle1 = new ChromaticBurstParticleEasing(NPC.Center, Vector2.Zero, Color.Red, (Color.Red * 0.4f) with { A = 255 }, 20, 20f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                //ChromaticBurstParticleEasingAlt particle1 = new ChromaticBurstParticleEasingAlt(NPC.Center, Vector2.Zero, Color.Red, 20, 2f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                //ChromaticBurstParticle particle1 = new ChromaticBurstParticle(NPC.Center, Vector2.Zero, Color.Red, 20, 1.5f, 0.05f);
                GeneralParticleHandler.SpawnParticle(particle1);
                NPC.ai[1] = 0;
            }
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.GetLerpValue(0, 1, AttackTimer / (float)60, true);

            if (AttackTimer > animTime)
            {
                hide = false;
                SetNextAttack(ClamitasAttacks.StartingCutscene);
                //SetNextAttack(ClamitasAttacks.CrossSpirits);
            }
        }
        private void UpdateFall(float xOffsetTeleport, ref float aiSubstate, ref float fallCounter, bool isFisrtFall = false, Action<Vector2> onFallEffect = null)
        {
            //ref float slamCount = ref NPC.Infernum().ExtraAI[4];
            NPC.velocity.X = 0f;
            if (AttackTimer == 1f)
            {
                aiSubstate = 1f;
                NPC.netUpdate = true;
            }

            if (aiSubstate == 1f) //teleport
            {
                NPC.alpha += 20;

                NPC.velocity.Y = 0f;
                NPC.noGravity = true;
                NPC.noTileCollide = true;

                if (NPC.alpha >= 255)
                {
                    NPC.alpha = 255;
                    NPC.position.X = player.position.X - 60f + xOffsetTeleport;
                    NPC.position.Y = player.position.Y - 400f;

                    aiSubstate = 2f;
                    NPC.netUpdate = true;
                }
            }
            else if (aiSubstate == 2f) //alpha charging
            {
                if (isFisrtFall)
                    NPC.alpha -= 6;
                else
                    NPC.alpha -= 16;

                if (NPC.alpha <= 0)
                {
                    NPC.alpha = 0;
                    aiSubstate = 3f;
                    NPC.netUpdate = true;
                }
            }
            else if (aiSubstate == 3f) //fall proccess and end
            {
                if (NPC.Bottom.Y > player.Top.Y || NPC.noTileCollide == false)
                {
                    NPC.noTileCollide = false;

                    if (NPC.velocity.Y == 0f)
                    {
                        SoundEngine.PlaySound(GiantClam.SlamSound, NPC.Bottom);

                        AttackTimer = 0;
                        aiSubstate = 1f;
                        fallCounter++;
                        onFallEffect.Invoke(NPC.Center);

                        NPC.netUpdate = true;
                    }
                    else
                        NPC.velocity.Y += 1f;
                }
                else
                    NPC.velocity.Y += 1f;
            }
        }
        private void AI_SpiritRain()
        {
            int type = ModContent.ProjectileType<BrimstoneSpiritsRaining>();
            if (AttackTimer < SpiritRain_Timing)
            {
                if (AttackTimer % 2 == 0)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(-10, 10), 10), type, NPC.GetProjectileDamageClamity(type), 0, Main.myPlayer);
            }
            else if (AttackTimer == SpiritRain_Timing)
            {

            }

            if (AttackTimer == SpiritRain_Timing + 600)
            {
                SetNextAttack(ClamitasAttacks.CrossSpirits);
            }
        }
        private void AI_SidefallTeleports()
        {
            int maxFallCount = 4;
            //int delay = 60;
            Action<Vector2> createRing = (Vector2 center) =>
            {
                int ringCount = 10;
                for (int i = 0; i < ringCount; i++)
                {
                    int type = ModContent.ProjectileType<AfterRoarAcceratingBrimstoneBarrage>();
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / i * ringCount), type, NPC.GetProjectileDamageClamity(type), 0);
                    AttackTimer = 1;
                }
            };

            if (NPC.ai[2] < maxFallCount)
            {
                UpdateFall(Main.rand.Next(500, 600), ref NPC.ai[1], ref NPC.ai[2], onFallEffect: createRing);
            }
            else
            {
                NPC.Calamity().newAI[0] = 1;
                //roar to speed up a rings of projectiles
            }

            if (AttackTimer == 180)
            {
                SetNextAttack(ClamitasAttacks.CrossSpirits);
            }
        }
        private void AI_CrossSpirits()
        {
            int delay = 100;
            if (AttackTimer % delay == 0)
            {
                float num = AttackTimer % (delay * 2) == 0 ? MathHelper.PiOver4 : 0;
                Vector2 center = player.Center;
                for (int i = 0; i < 4; i++)
                {
                    float rot = MathHelper.PiOver2 * i + num;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), center + Vector2.UnitX.RotatedBy(rot) * 2000, -Vector2.UnitX.RotatedBy(rot) * 60, ModContent.ProjectileType<BrimstoneSpiritsSpawner>(), 1, 1, NPC.target, 100, 5);
                }
                SoundEngine.PlaySound(SoundID.NPCHit36, player.Center);
            }

            if (AttackTimer > delay * 4 + delay / 2)
            {
                SetNextAttack(ClamitasAttacks.SpiritWave);
            }

        }
        private void AI_SpiritWave()
        {

        }
        private void AI_Brimfall()
        {

        }        
    }
}
