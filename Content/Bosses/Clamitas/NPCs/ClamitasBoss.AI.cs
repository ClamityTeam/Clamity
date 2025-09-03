using CalamityMod;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.NPCs;
using CalamityMod.Particles;
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
    public partial class ClamitasBoss : ModNPC
    {
        public enum Attacks : int
        {
            PreFight = 0,
            StartingCutscene = 1,

            SidefallTeleports,
            FastTeleports,
            CrossSpirits,
            SpiritWave,

            HahaLimboMonent, //what is this. i forgot
        }

        private int attack = (int)Attacks.PreFight;
        public Attacks CurrentAttack
        {
            get => (Attacks)attack;
            set => attack = (int)value;
        }
        public int AttackTimer
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public bool BattleIsStarted => CurrentAttack != Attacks.PreFight && (CurrentAttack != Attacks.StartingCutscene);
        public Player player => Main.player[NPC.target];
        public override void AI()
        {
            Myself = NPC;
            NPC.TargetClosest();
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            NPC.chaseable = hasBeenHit;

            if (BattleIsStarted && Main.netMode != NetmodeID.Server && !Main.player[NPC.target].dead && Main.player[NPC.target].active)
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
            AttackTimer++;
            switch (CurrentAttack)
            {
                case Attacks.PreFight:
                    AttackIThink_PreFightState();
                    break;
                case Attacks.StartingCutscene:
                    AttackIThink_StartingCutsceneState();
                    break;
                case Attacks.SidefallTeleports:
                    Attack_SidefallTeleports();
                    break;
                case Attacks.CrossSpirits:
                    Attack_CrossSpirits();
                    break;
                case Attacks.SpiritWave:
                    Attack_SpiritWave();
                    break;

            }
        }
        private Attacks SetNextAttack(Attacks nextAttack)
        {
            CurrentAttack = nextAttack;
            AttackTimer = 0;

            return nextAttack;
        }
        private void AttackIThink_PreFightState()
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
                SetNextAttack(Attacks.StartingCutscene);
            }
        }
        private void AttackIThink_StartingCutsceneState()
        {
            hide = true;
            int animTime = 120;

            if (AttackTimer % 10 == 0)
            {
                //ChromaticBurstParticleEasing particle1 = new ChromaticBurstParticleEasing(NPC.Center, Vector2.Zero, Color.Red, (Color.Red * 0.4f) with { A = 255 }, 20, 2f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                ChromaticBurstParticleEasingAlt particle1 = new ChromaticBurstParticleEasingAlt(NPC.Center, Vector2.Zero, Color.Red, 20, 2f, 0.00f, EasingCurves.Quadratic, EasingType.Out);
                //ChromaticBurstParticle particle1 = new ChromaticBurstParticle(NPC.Center, Vector2.Zero, Color.Red, 20, 1.5f, 0.05f);
                GeneralParticleHandler.SpawnParticle(particle1);
            }
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.GetLerpValue(0, 1, AttackTimer / (float)60, true);

            if (AttackTimer > animTime)
            {
                hide = false;
                SetNextAttack(Attacks.CrossSpirits);
            }
        }
        private void Do_UpdateFall(Vector2 teleportPosition, int charkUpTime, bool skipCharge = false, Action<Vector2> onFallEffect = null)
        {
            if (NPC.ai[1] == 0f) //find target
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.TargetClosest(true);
                    NPC.ai[2] = 1f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 1f) //teleport
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
                    /*NPC.position.X = player.Center.X - (float)(NPC.width / 2);
                    NPC.position.Y = player.Center.Y - (float)(NPC.height / 2) + player.gfxOffY - 200f;
                    NPC.position.X = NPC.position.X - 15f;
                    NPC.position.Y = NPC.position.Y - 100f;*/
                    NPC.Center = teleportPosition;
                    NPC.ai[1] = 2f + (skipCharge ? 1 : 0);
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 2f) //charging attack
            {
                if (Main.rand.NextBool())
                {
                    int attackDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch, 0f, 0f, 200, default, 1.5f);
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
                    NPC.ai[1] = 3f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 3f) //Fall itself
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                NPC.velocity.Y += 0.8f;
                attackAnim = true;
                if (NPC.Center.Y > (player.Center.Y - (float)(NPC.height / 2) + player.gfxOffY - 15f))
                {
                    NPC.noTileCollide = false;
                    NPC.ai[1] = 4f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 4f) //landing
            {
                if (NPC.velocity.Y == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                    NPC.noGravity = false;
                    attack = -1;
                    SoundEngine.PlaySound(SlamSound, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        onFallEffect.Invoke(NPC.Center);
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
        private void Attack_SidefallTeleports()
        {
            int attackCount = 4;
            int delay = 60;
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
                SetNextAttack(Attacks.SpiritWave);
            }
        }
        private void Attack_SpiritWave()
        {

        }
    }
}
