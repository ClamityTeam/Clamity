using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.Particles;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.Ihor.Projectiles;
using Clamity.Content.Particles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.NPCs
{
    public partial class IhorHead : ModNPC
    {
        public enum Attacks : int
        {
            //23 seconds. Probably a lot of time to spend on intro
            Summon = 0,

            //Phase 1
            Flamethrower,
            SnowAbsorbtionStar,
            IcePathDash,
            ShrinkingRing,
            Whiplash,
            WhipThrowRocks,
            IcePillars,

            //Phase 2
            PhaseTransition,

            IceMaze,
        }
        public Player player => Main.player[NPC.target];
        public ref float Attack => ref NPC.ai[1];
        public Attacks CurrentAttack => (Attacks)((int)Attack);
        public ref float AttackTimer => ref NPC.ai[2];
        //public ref float PreviousAttack => ref NPC.ai[3];
        public static bool BlizzardEffect;
        public Attacks? DebugDefaultAttack = Attacks.Flamethrower;
        public override void AI()
        {
            #region Pre-Attack
            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool masterMode = Main.masterMode || bossRush;
            bool revenge = CalamityWorld.revenge || bossRush;
            bool death = CalamityWorld.death || bossRush;

            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            // Enrage
            if (!player.ZoneSnow && !bossRush)
            {
                if (biomeEnrageTimer > 0)
                    biomeEnrageTimer--;
            }
            else
                biomeEnrageTimer = CalamityGlobalNPC.biomeEnrageTimerMax;

            bool biomeEnraged = biomeEnrageTimer <= 0 || bossRush;

            float enrageScale = bossRush ? 1f : 0f;
            if (biomeEnraged)
            {
                NPC.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 2f;
            }
            #endregion

            //Main AI           

            AttackTimer++;
            switch (CurrentAttack)
            {
                case Attacks.Summon:
                    Do_Summon();
                    break;
                case Attacks.Flamethrower:
                    Do_Flamethrower();
                    break;
                case Attacks.IcePathDash:

                    break;
                case Attacks.Whiplash:

                    break;
                case Attacks.WhipThrowRocks:

                    break;
                case Attacks.IcePillars:

                    break;
                case Attacks.PhaseTransition:

                    break;
                case Attacks.IceMaze:

                    break;
            }

            #region Summon body
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!tailSpawned && NPC.ai[0] == 0f)
                {
                    int previous = NPC.whoAmI;
                    int minLength = death ? 24 : revenge ? 21 : expertMode ? 18 : 15;
                    //minLength *= 2;
                    //if (Main.zenithWorld) minLength *= 2; //funny too long worm on GFB


                    for (int i = 0; i < minLength + 1; i++)
                    {
                        int next, ihorType;
                        if (i > (int)(minLength * 0.75f))
                            ihorType = ModContent.NPCType<IhorBodySmall>();
                        else
                            ihorType = ModContent.NPCType<IhorBody>();
                        next = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ihorType, NPC.whoAmI);

                        Main.npc[next].ai[2] = NPC.whoAmI;
                        Main.npc[next].realLife = NPC.whoAmI;
                        Main.npc[next].ai[1] = previous;
                        Main.npc[next].ai[3] = i;
                        Main.npc[previous].ai[0] = next;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, next, 0f, 0f, 0f, 0);
                        previous = next;
                    }
                }
                tailSpawned = true;
            }
            #endregion
        }
        private void SetRandomAttack(int phase = 0)
        {
            if (DebugDefaultAttack is not null)
            {
                SetAttack(DebugDefaultAttack.Value);
                return;
            }

            List<int> list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, };
            //if (phase == 1) list = new List<int>() {  };
            list.Remove((int)Attack);

            Attack = Main.rand.Next(list);
            AttackTimer = 0;
            NPC.Calamity().newAI[0] = 0;
            NPC.Calamity().newAI[1] = 0;
            NPC.Calamity().newAI[2] = 0;
            NPC.Calamity().newAI[3] = 0;
        }
        private void SetAttack(Attacks attack)
        {
            //PreviousAttack = Attack;
            Attack = (int)attack;
            AttackTimer = 0;
            NPC.Calamity().newAI[0] = 0;
            NPC.Calamity().newAI[1] = 0;
            NPC.Calamity().newAI[2] = 0;
            NPC.Calamity().newAI[3] = 0;
        }
        private void Roar()
        {
            ChromaticBurstParticle p = new(NPC.Center, Vector2.Zero, Color.LightBlue, 16, 0, 16f);
            GeneralParticleHandler.SpawnParticle(p);
            SoundEngine.PlaySound(Mauler.RoarSound with { Pitch = 0.2f }, NPC.Center);
        }
        private void Move(Vector2 target = default, float ratio = 0.015f)
        {
            if (target == default)
                target = player.Center;

            NPC.velocity = (target - NPC.Center) * ratio;
            //float inertia = 30f;
            //NPC.velocity = (NPC.velocity * (inertia - 1f) + (player.Center - NPC.Center)) / inertia;

            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;

        }
        private void MoveConst(Vector2 target = default, float ratio = 0.1f)
        {
            if (target == default)
                target = player.Center;

            Vector2 diff = target - NPC.Center;
            Vector2 velocity = diff.SafeNormalize(Vector2.Zero) * ratio;
            if (velocity.Length() > diff.Length()) NPC.velocity = diff;
            else NPC.velocity = velocity;
            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        }
        private void Do_Summon()
        {
            BlizzardEffect = false;
            if (DebugDefaultAttack is not null)
            {
                SetRandomAttack();
                return;
            }

            NPC.chaseable = false;
            if (AttackTimer == 20 * 60)
            {
                NPC.Opacity = 1;
                NPC.velocity = Vector2.UnitY * 10;
                NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                NPC.Center = player.Center + Vector2.UnitY * 1000;
                NPC.dontTakeDamage = false;
            }
            else if (AttackTimer < 20 * 60)
            {
                NPC.Opacity = 0;
                NPC.Center = player.Center + Vector2.UnitY * 1000;
                NPC.dontTakeDamage = true;
            }
            else if (AttackTimer > 22 * 60)
            {
                Move(default, 0.05f);
            }
            if (AttackTimer > 23 * 60)
            {
                NPC.Opacity = 1;
                NPC.chaseable = true;
                SetAttack(Attacks.SnowAbsorbtionStar);
                //SetRandomAttack();
            }
        }
        //yes do it
        private void Do_()
        {

        }
        #region Phase 1
        private void Do_Flamethrower()
        {
            int Flytime = 100;
            int AttackCounts = 4;
            int TimeToAttack = 40;
            int FlameDelay = 5;

            if (AttackTimer == 1)
            {
                Roar();
            }

            int a = (int)AttackTimer % (Flytime);
            Vector2 t = player.MountedCenter + player.velocity - Vector2.UnitY * 300;
            int setDamage = NPC.defDamage;

            if (AttackTimer < 60)
            {
                NPC.velocity = (t - NPC.Center) * (AttackTimer / 60f);
                NPC.rotation = (player.MountedCenter - NPC.Center).ToRotation() - MathHelper.PiOver2;
                NPC.damage = 0;
            }
            else if (a > Flytime - TimeToAttack) //fire attack
            {
                if (NPC.damage == 0) NPC.damage = setDamage;
                //Move(t, 0.1f);
                //NPC.rotation = (player.MountedCenter - NPC.Center).ToRotation() - MathHelper.PiOver2;
                if (++NPC.Calamity().newAI[0] > FlameDelay)
                {
                    int proj = ModContent.ProjectileType<IhorFire>();
                    int dmg = NPC.GetProjectileDamageClamity(proj);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 4, proj, dmg, 0, Main.myPlayer);
                    NPC.Calamity().newAI[0] = 0;
                }
            }
            else //fly
            {
                if (NPC.damage == 0) NPC.damage = setDamage;
                Move(t, 0.075f);
                //NPC.velocity = Vector2.Zero;
                NPC.rotation = (player.MountedCenter - NPC.Center).ToRotation() - MathHelper.PiOver2;
                if (a == Flytime - TimeToAttack) NPC.velocity = Vector2.Zero;
            }


            if (AttackTimer >= Flytime * AttackCounts)
            {
                //SetRandomAttack();
            }
        }
        public const int Pre_IcePathDashTime = 120;
        public const int Icicle_IcePathDashTime = 200;
        public const int All_IcePathDashTime = 600;
        private void Do_IcePathDash()
        {
            if (AttackTimer == 1)
            {
                for (int i = -3; i < 3; i++)
                {
                    int type = ModContent.ProjectileType<IhorSnowflakeCreatingIcicle>();
                    int projectileDamage = NPC.GetProjectileDamageClamity(type);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, projectileDamage, 0, Main.myPlayer, NPC.whoAmI, i);
                }
            }
            else if (AttackTimer < Pre_IcePathDashTime)
            {
                Move(default, 0.005f);
            }
            else if (AttackTimer == Pre_IcePathDashTime)
            {
                NPC.velocity = player.Center - NPC.Center;
            }
            else if (AttackTimer > All_IcePathDashTime - Icicle_IcePathDashTime)
            {
                if (AttackTimer % 20 == 0)
                {
                    int type = ModContent.ProjectileType<IhorIcicleHomingIThink>();
                    int projectileDamage = NPC.GetProjectileDamageClamity(type);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, projectileDamage, 0, Main.myPlayer, NPC.whoAmI);
                }
            }



            if (AttackTimer == All_IcePathDashTime)
            {

            }
        }
        #endregion

        #region Phase 2
        private void Do_PhaseTransition()
        {
            //I will later something cook
        }
        #endregion














        public override void OnKill()
        {
            BlizzardEffect = false;
        }









        /*private void Do_MagicBurst()
        {
            Move();

            int particleDelay = 20;
            if (AttackTimer < particleDelay * 3 + 1)
            {
                if (AttackTimer % particleDelay == 0)
                {
                    //Roar();
                    IhorChargeChromaticBurstParticle p = new(NPC.whoAmI);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }
            else if (AttackTimer == particleDelay * 3 + 2)
            {
                int type = ModContent.ProjectileType<IhorSpiralIcicles>();
                int projectileDamage = NPC.GetProjectileDamageClamity(type);
                for (int i = 0; i < 40; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(10f, 15f), 0).RotatedBy(NPC.rotation + MathHelper.PiOver2 + Main.rand.NextFloat(-0.4f, 0.4f)), type, projectileDamage, 1f, Main.myPlayer, Main.rand.NextFloat(1.4f, 1.5f) * (Main.rand.NextBool() ? -1 : 1));
                }
            }
            if (AttackTimer > 400 + particleDelay * 3)
            {
                //SetAttack(IhorAttacks.MagicBurst);
                SetRandomAttack();
            }

        }
        
        private void Do_SnowAbsorbtionStar()
        {
            if (AttackTimer == 30)
            {
                //Roar();
                int type = ModContent.ProjectileType<SnowAbsorbtionStar>();
                int projectileDamage = NPC.GetProjectileDamageClamity(type);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, projectileDamage, 0, Main.myPlayer, NPC.whoAmI);
            }
            Move(0.005f);
        }
        
        

        private void Do_Whiplash()
        {
            if (AttackTimer < 30)
            {
                Move(0.005f);
            }
            else if (NPC.ai[2] == 0)
            {
                Move(NPC.Center + Vector2.UnitY * 100);
                if (NPC.Center.Y > player.Center.Y) {
                    NPC.ai[2] = 1;
                }

            }
        }




        //Prob Scrapped
        private void Do_HomingSnowballs()
        {
            Move();

            int particleDelay = 20;
            if (AttackTimer < particleDelay * 3 + 1)
            {
                if (AttackTimer % particleDelay == 0)
                {
                    //Roar();
                    IhorChargeChromaticBurstParticle p = new(NPC.whoAmI);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }
            else if (AttackTimer == particleDelay * 3 + 2)
            {
                int type = ModContent.ProjectileType<HomingSnowball>();
                int projectileDamage5 = NPC.GetProjectileDamageClamity(type);
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(NPC.rotation - MathHelper.PiOver2 / 4 * i) * 25 * i, type, projectileDamage5, 1f, Main.myPlayer);
                }
            }
            if (AttackTimer > 400 + particleDelay * 3)
            {
                SetRandomAttack();
                //SetAttack(IhorAttacks.SnowFlake);
            }
        }
        //TODO - need improve
        private void Do_SnowFlake()
        {
            Move();

            if (AttackTimer == 1)
            {
                Roar();
            }

            int count = 4 + (CalamityWorld.death || BossRushEvent.BossRushActive ? 2 : (CalamityWorld.revenge ? 1 : 0));
            int delay = (int)(30 * 4f / count);
            if (AttackTimer % delay == 0 && AttackTimer < delay * count + 1)
            {
                int type = ModContent.ProjectileType<StaticGiantSnowFlake>();
                int projectileDamage5 = NPC.GetProjectileDamageClamity(type);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, projectileDamage5, 1f, Main.myPlayer, Main.rand.NextFloat(0.5f, 1f) * (Main.rand.NextBool() ? -1 : 1), NPC.target, MathHelper.TwoPi / count * NPC.Calamity().newAI[0] - MathHelper.PiOver2);
                NPC.Calamity().newAI[0]++;
            }
            if (AttackTimer > 300 + delay * count + 180)
            {
                SetRandomAttack();
                //SetAttack(IhorAttacks.HomingSnowballs);
            }

        }*/
    }
}
