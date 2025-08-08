using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.Particles;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.Ihor.Particles;
using Clamity.Content.Bosses.Ihor.Projectiles;
using Clamity.Content.Particles;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.NPCs
{
    /*public enum IhorMagicAttacks : int
    {
        Summon = 0,
        MagicBurst,
        HomingStowballs,

        StormPillars,
    }
    public enum IhorMeleeAttacks : int
    {
        Summon = 0,
        LinearDash,
        HomingDash,
        DoGLikeDash,
    }*/
    public enum IhorAttacks : int
    {
        Summon = 0, //23 seconds. Probably a lot of time to spend on intro
        MagicBurst,
        Flamethrower,
        HomingSnowballs,
        SnowFlake,
        IceRockRain,

        StormPillars,
    }
    public class IhorHead : ModNPC
    {
        private int biomeEnrageTimer = CalamityGlobalNPC.biomeEnrageTimerMax;
        private bool tailSpawned = false;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);

        }
        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.GetNPCDamageClamity();
            NPC.defense = 4;
            NPC.npcSlots = 12f;
            NPC.width = 114;
            NPC.height = 114;

            NPC.LifeMaxNERB(95000, 114400, 1650000);

            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            //NPC.alpha = 255;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.netAlways = true;

            if (!Main.dedServ)
            {
                Music = Clamity.mod.GetMusicFromMusicMod("Ihor") ?? MusicID.Boss2;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(biomeEnrageTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            biomeEnrageTimer = reader.ReadInt32();
        }
        public override void OnSpawn(IEntitySource source)
        {
            Attack = (int)IhorAttacks.MagicBurst;
            //PreviousAttack = (int)IhorAttacks.MagicBurst;
            AttackTimer = 0;
        }
        public Player player => Main.player[NPC.target];
        public ref float Attack => ref NPC.ai[1];
        public IhorAttacks AttackEnum => (IhorAttacks)((int)Attack);
        public ref float AttackTimer => ref NPC.ai[2];
        //public ref float PreviousAttack => ref NPC.ai[3];
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
            switch ((IhorAttacks)((int)Attack))
            {
                case IhorAttacks.Summon:
                    Do_Summon();
                    break;
                case IhorAttacks.MagicBurst:
                    Do_MagicBurst();
                    break;
                case IhorAttacks.Flamethrower:
                    Do_Flamethrower();
                    break;
                case IhorAttacks.HomingSnowballs:
                    Do_HomingSnowballs();
                    break;
                case IhorAttacks.SnowFlake:
                    Do_SnowFlake();
                    break;
                case IhorAttacks.IceRockRain:

                    break;

                    /*case IhorAttacks.StormPillars:

                        break;*/
            }

            #region Summon body
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!tailSpawned && NPC.ai[0] == 0f)
                {
                    int previous = NPC.whoAmI;
                    int minLength = death ? 24 : revenge ? 21 : expertMode ? 18 : 15;


                    for (int i = 0; i < minLength + 1; i++)
                    {
                        int lol, ihorType;
                        if (i > (int)(minLength * 0.75f))
                            ihorType = ModContent.NPCType<IhorBodySmall>();
                        else
                            ihorType = ModContent.NPCType<IhorBody>();
                        lol = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ihorType, NPC.whoAmI);

                        Main.npc[lol].ai[2] = NPC.whoAmI;
                        Main.npc[lol].realLife = NPC.whoAmI;
                        Main.npc[lol].ai[1] = previous;
                        Main.npc[previous].ai[0] = lol;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, lol, 0f, 0f, 0f, 0);
                        previous = lol;
                    }
                }
                tailSpawned = true;
            }
            #endregion
        }
        private void SetRandomAttack()
        {
            SetAttack(IhorAttacks.Flamethrower);
            return;

            List<int> list = new List<int>() { 1, 2, 3, 4 };
            list.Remove((int)Attack);
            //PreviousAttack = Attack;

            /*string test = Attack.ToString();
            foreach (var i in list)
                test += " " + i.ToString();
            Main.NewText(test);*/

            Attack = Main.rand.Next(list);
            //Main.NewText(Attack);
            AttackTimer = 0;
            NPC.Calamity().newAI[0] = 0;
        }
        private void SetAttack(IhorAttacks attack)
        {
            //PreviousAttack = Attack;
            Attack = (int)attack;
            AttackTimer = 0;
            NPC.Calamity().newAI[0] = 0;
        }
        private void Roar()
        {
            ChromaticBurstParticle p = new(NPC.Center, Vector2.Zero, Color.LightBlue, 16, 0, 16f);
            GeneralParticleHandler.SpawnParticle(p);
            SoundEngine.PlaySound(Mauler.RoarSound with { Pitch = 0.2f }, NPC.Center);
        }
        private void Move(float percent = 0.015f)
        {
            NPC.velocity = (player.Center - NPC.Center) * percent;
            //float inertia = 30f;
            //NPC.velocity = (NPC.velocity * (inertia - 1f) + (player.Center - NPC.Center)) / inertia;

            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        }
        private void MoveConst(float percent = 0.1f)
        {
            NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * percent;
            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        }
        private void Do_Summon()
        {
            //SetRandomAttack();
            SetAttack(IhorAttacks.HomingSnowballs);
            return;

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
                Move(0.05f);
            }
            if (AttackTimer > 23 * 60)
            {
                NPC.Opacity = 1;
                SetAttack(IhorAttacks.SnowFlake);
                //SetRandomAttack();
            }
        }
        private void Do_MagicBurst()
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
                int type = ModContent.ProjectileType<IhorIcicles>();
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
        private const int LineTime = 200;
        private const int PreDashTime = 30;
        private const int DashTime = 100;
        private void Do_Flamethrower()
        {
            if (AttackTimer == 1)
            {
                Roar();
            }

            int a = (int)AttackTimer % (LineTime + PreDashTime + DashTime);



            //Old AI
            /*if (AttackTimer == 1)
            {
                Roar();
            }

            if (NPC.velocity.Length() < 10f)
            {
                NPC.velocity *= 1.01f;
            }
            NPC.velocity = NPC.velocity.RotateTowards(NPC.AngleTo(player.Center), 0.03f);
            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;

            if (AttackTimer % 10 == 0 && AttackTimer > 90)
            {
                int type = ModContent.ProjectileType<IhorFire>();
                int projectileDamage = NPC.GetProjectileDamageClamity(type);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 3).RotatedBy(NPC.rotation + Main.rand.NextFloat(0.1f)), type, projectileDamage, 1f, Main.myPlayer);


                type = ModContent.ProjectileType<IhorIcicles>();
                projectileDamage = NPC.GetProjectileDamageClamity(type);
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(10f, 15f), 0).RotatedBy(NPC.rotation + MathHelper.PiOver2 + Main.rand.NextFloat(-0.4f, 0.4f)), type, projectileDamage, 1f, Main.myPlayer, Main.rand.NextFloat(1.4f, 1.5f) * (Main.rand.NextBool() ? -1 : 1));
                }
            }

            if (AttackTimer > 400)
            {
                //SetAttack(IhorAttacks.MagicBurst);
                SetRandomAttack();
            }*/
        }
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
                int type = ModContent.ProjectileType<SnowFlake>();
                int projectileDamage5 = NPC.GetProjectileDamageClamity(type);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, projectileDamage5, 1f, Main.myPlayer, Main.rand.NextFloat(0.5f, 1f) * (Main.rand.NextBool() ? -1 : 1), NPC.target, MathHelper.TwoPi / count * NPC.Calamity().newAI[0] - MathHelper.PiOver2);
                NPC.Calamity().newAI[0]++;
            }
            if (AttackTimer > 300 + delay * count + 180)
            {
                SetRandomAttack();
                //SetAttack(IhorAttacks.HomingSnowballs);
            }

        }
        private void Do_IceRockRain()
        {

        }
        /*
        private void Do_()
        {

        }
        */
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.IhorHead.Bestiary")
            });
        }
        public override bool CheckActive() => false;
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AttackEnum is IhorAttacks.Flamethrower)
            {
                //Texture2D line = ModContent.Request<Texture2D>("CalamityMod/ExtraTexture/LaserWallTelegraphBeam").Value;

                SpriteEffects effects = SpriteEffects.None;
                if (NPC.spriteDirection == 1)
                {
                    effects = SpriteEffects.FlipHorizontally;
                }

                Texture2D value6 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLine").Value;
                float path = (AttackTimer % (LineTime + DashTime)) > LineTime ? 0 : AttackTimer / LineTime;
                Color color3 = Color.Lerp(Color.Blue, Color.White, path);
                spriteBatch.Draw(value6,
                                 NPC.Center /*- base.NPC.rotation.ToRotationVector2() * base.NPC.spriteDirection * 104f*/ - screenPos,
                                 null,
                                 color3,
                                 base.NPC.rotation + MathF.PI / 2f,
                                 new Vector2((float)value6.Width / 2f, value6.Height),
                                 new Vector2(1f * path, 4200f),
                                 effects,
                                 0f);
            }

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
