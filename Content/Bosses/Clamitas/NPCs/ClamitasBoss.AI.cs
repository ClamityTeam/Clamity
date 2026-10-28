using CalamityMod;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Particles;
using Clamity.Commons;
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
        PreFight = 0,
        StartingCutscene = 1,

        SidefallTeleports,
        CrossSpirits,
        SpiritWave,

        HahaLimboMonent, //what is this. i forgot
    }
    public partial class ClamitasBoss : ModNPC
    {
        private int attack = (int)ClamitasAttacks.PreFight;
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
        public byte clamStatus = 0;
        public override void AI()
        {
            Myself = NPC;
            NPC.TargetClosest();
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            NPC.chaseable = hasBeenHit;

            if (Main.netMode != NetmodeID.Server && !Main.player[NPC.target].dead && Main.player[NPC.target].active)
            {
                player.AddBuff(ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.Clamity>(), 2);
                player.AddBuff(ModContent.BuffType<BossEffects>(), 2);
            }
            if (Main.player[NPC.target].dead && !Main.player[NPC.target].active)
            {
                NPC.active = false;
            }

            if (!hide)
            {
                Lighting.AddLight(NPC.Center, 0.75f, 0, 0);
            }

            if (!statChange)
            {
                NPC.defense = 35;
                NPC.damage = 200;

                statChange = true;
            }

            //Main.NewText($"{attack} - {AttackTimer}");

            if (clamStatus == 0)
                GiantClamAI();
            else
            {

                AttackTimer++;
                switch (CurrentAttack)
                {
                    case ClamitasAttacks.PreFight:
                        AttackIThink_PreFight();
                        break;
                    case ClamitasAttacks.StartingCutscene:
                        AttackIThink_StartingCutsceneState();
                        break;
                    case ClamitasAttacks.SidefallTeleports:
                        Attack_SidefallTeleports();
                        break;
                    case ClamitasAttacks.CrossSpirits:
                        Attack_CrossSpirits();
                        break;
                    case ClamitasAttacks.SpiritWave:
                        Attack_SpiritWave();
                        break;

                }
            }
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
        private void AttackIThink_PreFight()
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
        }
        private void AttackIThink_StartingCutsceneState()
        {
            hide = true;
            int animTime = 120;

            if (AttackTimer % 10 == 0)
            {
                ChromaticBurstParticleEasing particle1 = new ChromaticBurstParticleEasing(NPC.Center, Vector2.Zero, Color.Red, (Color.Red * 0.4f) with { A = 255 }, 20, 2f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                //ChromaticBurstParticleEasingAlt particle1 = new ChromaticBurstParticleEasingAlt(NPC.Center, Vector2.Zero, Color.Red, 20, 2f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                //ChromaticBurstParticle particle1 = new ChromaticBurstParticle(NPC.Center, Vector2.Zero, Color.Red, 20, 1.5f, 0.05f);
                GeneralParticleHandler.SpawnParticle(particle1);
            }
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.GetLerpValue(0, 1, AttackTimer / (float)60, true);

            if (AttackTimer > animTime)
            {
                hide = false;
                SetNextAttack(ClamitasAttacks.CrossSpirits);
            }
        }
        private void Do_UpdateFall(float xOffsetTeleport, ref float aiSubstate, ref float fallCounter, bool isFisrtFall = false, Action<Vector2> onFallEffect = null)
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
        private void Attack_SidefallTeleports()
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
                Do_UpdateFall(Main.rand.Next(500, 600), ref NPC.ai[1], ref NPC.ai[2], onFallEffect: createRing);
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
        private void Attack_CrossSpirits()
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
        private void Attack_SpiritWave()
        {

        }
        private void GiantClamAI()
        {
            if (NPC.ai[0] < 240f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.defense = Main.hardMode ? 35 : 10;

                NPC.ai[0] += 1f;
                hide = false;
            }
            else
            {
                if (attack == -1)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    attack = Main.rand.Next(2);
                    if (attack == 0)
                        attack = Main.rand.Next(2); //rarer chance of doing the hiding clam
                }
                else if (attack == 0)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    hide = true;
                    NPC.defense = 9999;
                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] >= 90f)
                    {
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        hide = false;
                        attack = -1;
                        NPC.defense = Main.hardMode ? 35 : 10;
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X + 5), (int)NPC.Center.Y, ModContent.NPCType<Clam>(), 0, 0f, 0f, 0f, 0f, 255);
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Clam>(), 0, 0f, 0f, 0f, 0f, 255);
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X - 5), (int)NPC.Center.Y, ModContent.NPCType<Clam>(), 0, 0f, 0f, 0f, 0f, 255);
                    }
                }
                else if (attack == 1)
                {
                    if (NPC.ai[2] == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.TargetClosest(true);
                            NPC.ai[2] = 1f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[2] == 1f)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        NPC.chaseable = false;
                        NPC.dontTakeDamage = true;
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;

                        NPC.alpha += Main.hardMode ? 8 : 5;
                        if (NPC.alpha >= 255)
                        {
                            NPC.alpha = 255;
                            NPC.position.X = player.Center.X - (float)(NPC.width / 2);
                            NPC.position.Y = player.Center.Y - (float)(NPC.height / 2) + player.gfxOffY - 200f;
                            NPC.position.X = NPC.position.X - 15f;
                            NPC.position.Y = NPC.position.Y - 100f;
                            NPC.ai[2] = 2f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[2] == 2f)
                    {
                        if (Main.rand.NextBool())
                        {
                            int attackDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 200, default, 1.5f);
                            Main.dust[attackDust].noGravity = true;
                            Main.dust[attackDust].velocity *= 0.75f;
                            Main.dust[attackDust].fadeIn = 1.3f;
                            Vector2 vector = new Vector2((float)Main.rand.Next(-200, 201), (float)Main.rand.Next(-200, 201));
                            vector.Normalize();
                            vector *= (float)Main.rand.Next(100, 200) * 0.04f;
                            Main.dust[attackDust].velocity = vector;
                            vector.Normalize();
                            vector *= 34f;
                            Main.dust[attackDust].position = NPC.Center - vector;
                        }

                        NPC.alpha -= Main.hardMode ? 7 : 4;
                        if (NPC.alpha <= 0)
                        {
                            // Set damage
                            NPC.damage = NPC.defDamage;

                            NPC.chaseable = true;
                            NPC.dontTakeDamage = false;
                            NPC.alpha = 0;
                            NPC.ai[2] = 3f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[2] == 3f)
                    {
                        // Set damage
                        NPC.damage = NPC.defDamage;

                        NPC.velocity.Y += 0.8f;
                        attackAnim = true;
                        if (NPC.Center.Y > (player.Center.Y - (float)(NPC.height / 2) + player.gfxOffY - 15f))
                        {
                            NPC.noTileCollide = false;
                            NPC.ai[2] = 4f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[2] == 4f)
                    {
                        if (NPC.velocity.Y == 0f)
                        {
                            // Avoid cheap bullshit
                            NPC.damage = 0;

                            NPC.ai[2] = 0f;
                            NPC.ai[0] = 0f;
                            NPC.netUpdate = true;
                            NPC.noGravity = false;
                            attack = -1;
                            SoundEngine.PlaySound(SlamSound, NPC.Center);
                            if (!Main.dedServ)
                            {
                                for (int stompDustArea = (int)NPC.position.X - 30; stompDustArea < (int)NPC.position.X + NPC.width + 60; stompDustArea += 30)
                                {
                                    for (int stompDustAmount = 0; stompDustAmount < 5; stompDustAmount++)
                                    {
                                        int stompDust = Dust.NewDust(new Vector2(NPC.position.X - 30f, NPC.position.Y + (float)NPC.height), NPC.width + 30, 4, DustID.Water, 0f, 0f, 100, default, 1.5f);
                                        Main.dust[stompDust].velocity *= 0.2f;
                                    }
                                    int stompGore = Gore.NewGore(NPC.GetSource_FromAI(), new Vector2((float)(stompDustArea - 30), NPC.position.Y + (float)NPC.height - 12f), default, Main.rand.Next(61, 64), 1f);
                                    Main.gore[stompGore].velocity *= 0.4f;
                                }
                            }
                        }

                        NPC.velocity.Y += 0.8f;
                    }
                }
            }
        }
    }
}
