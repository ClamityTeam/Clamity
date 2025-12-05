using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Furniture.DevPaintings;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.Pyrogen.Drop;
using Clamity.Content.Bosses.Pyrogen.Drop.Weapons;
using Clamity.Content.Bosses.Pyrogen.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using static Clamity.Commons.CalRemixCompatibilitySystem;


namespace Clamity.Content.Bosses.Pyrogen.NPCs
{
    public class PyrogenBossBar : ModBossBar
    {
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC nPC = Main.npc[info.npcIndexToAimAt];
            if (!nPC.active)
            {
                return false;
            }

            life = nPC.life;
            lifeMax = nPC.lifeMax;
            shield = 0f;
            shieldMax = 0f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC nPC2 = Main.npc[i];
                if (nPC2.active && nPC2.type == ModContent.NPCType<PyrogenShield>())
                {
                    shield += nPC2.life;
                    shieldMax += nPC2.lifeMax;
                }
            }

            return true;
        }
    }
    [AutoloadBossHead]
    public class PyrogenBoss : ModNPC
    {
        private static NPC myself;
        public static NPC Myself
        {
            get
            {
                if (myself is not null && !myself.active)
                    return null;

                return myself;
            }
            private set => myself = value;
        }
        private int biomeEnrageTimer = 300;

        private int currentPhase = 1;

        private int teleportLocationX;

        private int globalTimer;

        public FireParticleSet FireDrawer;

        public static readonly SoundStyle ShieldRegenSound = new SoundStyle("CalamityMod/Sounds/Custom/CryogenShieldRegenerate");
        public static Color BackglowColor => new Color(238, 102, 70, 80) * 0.6f;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementNPC", 2, Type);

            var fanny1 = new FannyDialog("Pyrogen", "FannyNuhuh").WithDuration(4f).WithCondition(_ => { return Myself is not null; });

            fanny1.Register();
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.npcSlots = 24f;
            //NPC.GetNPCDamageClamity();
            NPC.width = 86;
            NPC.height = 88;
            NPC.defense = 15;
            NPC.DR_NERD(0.3f);
            NPC.LifeMaxNERB(20000, 26000, 200000);  //Old:
                                                    //HP on normal with all shields = 33 500 (3 500 of shields)
                                                    //Death - 42 000 (7000 of shields)
                                                    //Boss Rush - 350 000 (50000 of shields)
            double num = (double)CalamityServerConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * num);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 40);
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<PyrogenBossBar>();
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.coldDamage = false;
            //NPC.HitSound = HitSound;
            //NPC.DeathSound = DeathSound;
            if (Main.getGoodWorld)
            {
                NPC.scale *= 0.8f;
            }

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = false;

            if (!Main.dedServ)
            {
                Music = Clamity.mod.GetMusicFromMusicMod("Pyrogen") ?? MusicID.Sandstorm;
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[2]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.PyrogenBoss.Bestiary")
            });
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(biomeEnrageTimer);
            writer.Write(teleportLocationX);
            writer.Write(NPC.dontTakeDamage);
            writer.Write(globalTimer);
            for (int i = 0; i < 4; i++)
            {
                writer.Write(NPC.Calamity().newAI[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            biomeEnrageTimer = reader.ReadInt32();
            teleportLocationX = reader.ReadInt32();
            NPC.dontTakeDamage = reader.ReadBoolean();
            globalTimer = reader.ReadInt32();
            for (int i = 0; i < 4; i++)
            {
                NPC.Calamity().newAI[i] = reader.ReadSingle();
            }
        }
        public override void AI()
        {
            #region PreAttackAI
            globalTimer++;
            Myself = NPC;
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();
            Lighting.AddLight((int)((NPC.position.X + NPC.width / 2) / 16f), (int)((NPC.position.Y + NPC.height / 2) / 16f), 0f, 1f, 1f);
            if (FireDrawer != null)
            {
                FireDrawer.Update();
            }

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 3200f)
            {
                NPC.TargetClosest();
            }

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

            if (num2 > 2f)
            {
                num2 = 2f;
            }

            if (bossRushActive)
            {
                num2 = 3f;
            }

            float num3 = NPC.life / (float)NPC.lifeMax;
            bool flag4 = num3 < (flag2 ? 0.85f : 0.8f) || flag3;
            bool flag5 = num3 < (flag3 ? 0.8f : flag2 ? 0.7f : 0.6f);
            bool flag6 = num3 < (flag3 ? 0.6f : flag2 ? 0.55f : 0.4f);
            bool flag7 = num3 < (flag3 ? 0.5f : flag2 ? 0.45f : 0.3f);
            bool flag8 = num3 < (flag3 ? 0.35f : 0.25f) && flag2;
            bool flag9 = num3 < (flag3 ? 0.25f : 0.15f) && flag2;
            int fireBarrage = ModContent.ProjectileType<FireBarrage>();
            int fireblast = ModContent.ProjectileType<Fireblast>();
            int fireBarrageHoming = ModContent.ProjectileType<FireBarrageHoming>();
            int type = 235;

            /*
            if (!Main.zenithWorld)
            {
                _ = SoundID.Item28;
            }
            else
            {
                _ = SoundID.Item20;
            }
            */

            //NPC.HitSound = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
            //NPC.DeathSound = (Main.zenithWorld ? SoundID.NPCDeath14 : DeathSound);
            NPC.damage = NPC.defDamage;
            if ((int)NPC.ai[0] + 1 > currentPhase)
            {
                HandlePhaseTransition((int)NPC.ai[0] + 1);
            }

            if (NPC.ai[2] == 0f && NPC.localAI[1] == 0f && Main.netMode != NetmodeID.MultiplayerClient && (NPC.ai[0] < 3f || bossRushActive || flag3 && NPC.ai[0] > 3f))
            {
                SoundEngine.PlaySound(in ShieldRegenSound, NPC.Center);
                int num7 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<PyrogenShield>(), NPC.whoAmI);
                NPC.ai[2] = num7 + 1;
                NPC.localAI[1] = -1f;
                NPC.netUpdate = true;
                Main.npc[num7].ai[0] = NPC.whoAmI;
                Main.npc[num7].netUpdate = true;
            }

            int num8 = (int)NPC.ai[2] - 1;
            if (num8 != -1 && Main.npc[num8].active && Main.npc[num8].type == ModContent.NPCType<PyrogenShield>())
            {
                NPC.dontTakeDamage = true;
            }
            else
            {
                NPC.dontTakeDamage = false;
                NPC.ai[2] = 0f;
                if (NPC.localAI[1] == -1f)
                {
                    NPC.localAI[1] = flag3 ? 840f : flag ? 1220f : 1580f;
                }

                if (NPC.localAI[1] > 0f)
                {
                    NPC.localAI[1] -= 1f;
                }
            }

            CalamityMod.CalamityMod.StopRain(); //honestly pyrogen should just stop rain because hes hot

            /*if (CalamityConfig.Instance.BossesStopWeather)
            {
                CalamityMod.CalamityMod.StopRain();
            }
            else if (!Main.raining)
            {
                CalamityUtils.StartRain();
            }*/

            if (!player.active || player.dead)
            {
                NPC.TargetClosest(faceTarget: false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead)
                {
                    if (NPC.velocity.Y > 3f)
                    {
                        NPC.velocity.Y = 3f;
                    }

                    NPC.velocity.Y -= 0.1f;
                    if (NPC.velocity.Y < -12f)
                    {
                        NPC.velocity.Y = -12f;
                    }

                    if (NPC.timeLeft > 60)
                    {
                        NPC.timeLeft = 60;
                    }

                    if (NPC.ai[1] != 0f)
                    {
                        NPC.ai[1] = 0f;
                        teleportLocationX = 0;
                        calamityGlobalNPC.newAI[2] = 0f;
                        NPC.netUpdate = true;
                    }

                    return;
                }
            }
            else if (NPC.timeLeft < 1800)
            {
                NPC.timeLeft = 1800;
            }

            /*if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
            {
                int type2 = 156;
                if (!NPC.AnyNPCs(type2))
                {
                    int num9 = 1000;
                    for (int i = 0; i < num9; i++)
                    {
                        int num10 = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                        int j;
                        for (j = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); j < Main.maxTilesY - 10 && !WorldGen.SolidTile(num10, j); j++)
                        {
                        }

                        j--;
                        if (!WorldGen.SolidTile(num10, j))
                        {
                            int num11 = NPC.NewNPC(NPC.GetSource_FromAI(), num10 * 16 + 8, j * 16, type2);
                            if (Main.netMode == 2 && num11 < Main.maxNPCs)
                            {
                                NetMessage.SendData(23, -1, -1, null, num11);
                            }

                            break;
                        }
                    }
                }
            }*/

            float num12 = bossRushActive ? 240f : 360f;
            float num13 = 60f;
            float num14 = NPC.ai[0] != 2f ? CalamityWorld.LegendaryMode && CalamityWorld.revenge ? 90f : 120f : CalamityWorld.LegendaryMode && CalamityWorld.revenge ? 60f : 80f;
            float num15 = 1f / num14;
            float num16 = 15f;
            float num17 = CalamityWorld.LegendaryMode && CalamityWorld.revenge ? 24f : 12f;
            float num18 = CalamityWorld.LegendaryMode && CalamityWorld.revenge ? 42f : 30f;
            if (Main.getGoodWorld)
            {
                num12 *= 0.7f;
                num13 *= 0.8f;
            }

            float num19 = num12 + num14;
            float num20 = num19 + num16;
            bool flag10 = NPC.ai[1] >= num12;
            if (flag && (NPC.ai[0] < 5f || !flag8) && !flag10) //summoning "ice bombs"
            {
                calamityGlobalNPC.newAI[3] += 1f;
                if (calamityGlobalNPC.newAI[3] >= (bossRushActive ? 660f : 900f))
                {
                    calamityGlobalNPC.newAI[3] = 0f;
                    //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                    //SoundEngine.PlaySound(in style, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int num21 = 3;
                        float num22 = MathF.PI * 2f / num21;
                        int projectileDamage = NPC.GetProjectileDamageClamity(fireblast);
                        float num24 = 2f + NPC.ai[0];
                        double num25 = (double)num22 * 0.5;
                        double a = (double)MathHelper.ToRadians(90f) - num25;
                        float num26 = (float)((double)num24 * Math.Sin(num25) / Math.Sin(a));
                        Vector2 spinningpoint = Main.rand.NextBool() ? new Vector2(0f, 0f - num24) : new Vector2(0f - num26, 0f - num24);
                        for (int k = 0; k < num21; k++)
                        {
                            Vector2 vector = spinningpoint.RotatedBy(num22 * k);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector) * 30f, vector, fireblast, projectileDamage, 0f, Main.myPlayer);
                        }
                    }
                }
            }
            #endregion

            #region Stage Animations

            if (NPC.ai[0] >= 1f && globalTimer % Main.rand.Next(60 / (int)NPC.ai[0], 60 / (int)NPC.ai[0] + 4) < 3)
            {
                Vector2 vec1 = new Vector2(NPC.width / 2f * Main.rand.NextFloat(-1, 1), NPC.height / 2f * Main.rand.NextFloat(-1, 1));
                Vector2 vec2 = vec1.RotatedByRandom(MathHelper.PiOver4);
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center + vec1, DustID.Flare, vec2 * i * 0.01f);
                    dust.scale = 1.5f;
                }
            }
            if (NPC.ai[0] >= 3f && globalTimer % Main.rand.Next(50, 60) < 3)
            {
                Vector2 vec11 = new Vector2(NPC.width * Main.rand.NextFloat(0, 1), NPC.height * Main.rand.NextFloat(0, 1));
                int index0 = Projectile.NewProjectile(NPC.GetSource_Death(), NPC.position, Vector2.Zero, ModContent.ProjectileType<PyrogenKillExplosion>(), 0, 0, Main.myPlayer, NPC.whoAmI, vec11.X, vec11.Y);
                //Main.projectile[index0].scale = 1f;
            }
            #endregion
            //Start of attack AI
            #region Phase 1 - curcle of fireballs (new firebomb)
            if (NPC.ai[0] == 0f) //phase 1 - curcle of fireballs
            {
                NPC.rotation = NPC.velocity.X * 0.1f;
                if (!NPC.dontTakeDamage)
                {
                    NPC.localAI[0] += 1f;
                    if (NPC.localAI[0] >= 120f)
                    {
                        NPC.localAI[0] = 0f;
                        NPC.TargetClosest();
                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                        {
                            //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                            //SoundEngine.PlaySound(in style, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int num27 = bossRushActive ? 24 : 16;
                                float num28 = MathF.PI * 2f / num27;
                                int fireBomb = ModContent.ProjectileType<FireBomb>();
                                float num30 = 9f + num2;
                                Vector2 spinningpoint2 = new Vector2(0f, 0f - num30);
                                for (int i = 0; i < 20; i++)
                                {
                                    //0.075f
                                    Vector2 vector2 = (Main.player[NPC.target].Center - NPC.Center) * 0.09f + Main.rand.NextVector2Circular(100, 100);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vector2, fireBomb, NPC.GetProjectileDamageClamity(fireBomb), 0f, Main.myPlayer);
                                }
                                /*for (int l = 0; l < num27; l++)
                                {
                                    Vector2 vector2 = spinningpoint2.RotatedBy(num28 * l);
                                    vector2 += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10f;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector2) * 30f, vector2, num29, projectileDamage2, 0f, Main.myPlayer);
                                }*/
                            }
                        }
                    }
                }

                Vector2 vector3 = new Vector2(NPC.Center.X, NPC.Center.Y);
                float num31 = player.Center.X - vector3.X;
                float num32 = player.Center.Y - vector3.Y;
                float num33 = (float)Math.Sqrt(num31 * num31 + num32 * num32);
                num33 = ((flag2 ? 5f : 4f) + 4f * num2) / num33;
                num31 *= num33;
                num32 *= num33;
                float num34 = 50f;
                if (Main.getGoodWorld)
                {
                    num34 *= 0.5f;
                }

                NPC.velocity.X = (NPC.velocity.X * num34 + num31) / (num34 + 1f);
                NPC.velocity.Y = (NPC.velocity.Y * num34 + num32) / (num34 + 1f);
                if (flag4)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 1f;
                    NPC.localAI[0] = 0f;
                    NPC.netUpdate = true;
                }

                return;
            }
            #endregion
            #region Phase 2 - floating above player and shooting a curcle of fireballs
            if (NPC.ai[0] == 1f) //phase 2 - floating above player and shooting a curcle of fireballs
            {
                if (NPC.ai[1] < num12 / 3 * 2)
                {
                    NPC.ai[1] += 1f;
                    NPC.rotation = NPC.velocity.X * 0.1f;

                    if (!NPC.dontTakeDamage)
                    {
                        NPC.localAI[0] += 1f;
                        if (NPC.localAI[0] >= 120f)
                        {
                            NPC.localAI[0] = 0f;
                            NPC.TargetClosest();
                            if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                            {
                                //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                                //SoundEngine.PlaySound(in style, NPC.Center);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int num35 = bossRushActive ? 18 : 12;
                                    float num36 = MathF.PI * 2f / num35;
                                    float num38 = 9f + num2;
                                    Vector2 spinningpoint3 = new Vector2(0f, 0f - num38);
                                    for (int m = 0; m < num35; m++)
                                    {
                                        Vector2 vector4 = spinningpoint3.RotatedBy(num36 * m);
                                        //vector4 += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 5f;
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector4) * 30f, vector4, fireBarrageHoming, NPC.GetProjectileDamageClamity(fireBarrageHoming), 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                    }

                    float num39 = flag2 ? 3.5f : 4f;
                    float num40 = 0.15f;
                    num39 -= num2 * 0.8f;
                    num40 += 0.07f * num2;
                    if (NPC.position.Y > player.position.Y - 375f)
                    {
                        if (NPC.velocity.Y > 0f)
                        {
                            NPC.velocity.Y *= 0.98f;
                        }

                        NPC.velocity.Y -= num40;
                        if (NPC.velocity.Y > num39)
                        {
                            NPC.velocity.Y = num39;
                        }
                    }
                    else if (NPC.position.Y < player.position.Y - 425f)
                    {
                        if (NPC.velocity.Y < 0f)
                        {
                            NPC.velocity.Y *= 0.98f;
                        }

                        NPC.velocity.Y += num40;
                        if (NPC.velocity.Y < 0f - num39)
                        {
                            NPC.velocity.Y = 0f - num39;
                        }
                    }

                    if (NPC.position.X + NPC.width / 2 > player.position.X + player.width / 2 + 300f)
                    {
                        if (NPC.velocity.X > 0f)
                        {
                            NPC.velocity.X *= 0.98f;
                        }

                        NPC.velocity.X -= num40;
                        if (NPC.velocity.X > num39)
                        {
                            NPC.velocity.X = num39;
                        }
                    }

                    if (NPC.position.X + NPC.width / 2 < player.position.X + player.width / 2 - 300f)
                    {
                        if (NPC.velocity.X < 0f)
                        {
                            NPC.velocity.X *= 0.98f;
                        }

                        NPC.velocity.X += num40;
                        if (NPC.velocity.X < 0f - num39)
                        {
                            NPC.velocity.X = 0f - num39;
                        }
                    }
                }
                else if (NPC.ai[1] < num19) //rotation before dash
                {
                    NPC.ai[1] += 1f;
                    float num41 = 3f;
                    if ((NPC.ai[1] - num12) % (num14 / num41) == 0f && Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                        //SoundEngine.PlaySound(in style, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.dontTakeDamage)
                        {
                            float num43 = 9f + num2;
                            float num44 = num43 - calamityGlobalNPC.newAI[0] * num43 * 0.5f;
                            int num45 = 7;
                            int num46 = 10 - (int)Math.Round(calamityGlobalNPC.newAI[0] * num45);
                            for (int n = 0; n < 2; n++)
                            {
                                float num47 = MathF.PI * 2f / num46;
                                float num48 = num44 - num44 * 0.5f * n;
                                double num49 = (double)num47 * 0.5;
                                double a2 = (double)MathHelper.ToRadians(90f) - num49;
                                float num50 = (float)((double)num48 * Math.Sin(num49) / Math.Sin(a2));
                                Vector2 spinningpoint4 = n == 0 ? new Vector2(0f, 0f - num48) : new Vector2(0f - num50, 0f - num48);
                                for (int num51 = 0; num51 < num46; num51++)
                                {
                                    Vector2 vector5 = spinningpoint4.RotatedBy(num47 * num51);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector5) * 30f, vector5, fireBarrage, NPC.GetProjectileDamageClamity(fireBarrage), 0f, Main.myPlayer, 0f, num44);
                                }
                            }
                        }
                    }

                    calamityGlobalNPC.newAI[0] += num15;
                    NPC.rotation += calamityGlobalNPC.newAI[0];
                    NPC.velocity *= 0.98f;
                }
                else //dash
                {
                    if (NPC.ai[1] == num19)
                    {
                        float num52 = Vector2.Distance(NPC.Center, player.Center) / num13 * 2f;
                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * (num52 + num2 * 2f);
                        if (NPC.velocity.Length() < num17)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= num17;
                        }

                        if (NPC.velocity.Length() > num18)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= num18;
                        }

                        NPC.ai[1] = num19 + num13;
                        calamityGlobalNPC.newAI[0] = 0f;
                    }

                    NPC.ai[1] -= 1f;
                    if (NPC.ai[1] == num19)
                    {
                        NPC.TargetClosest();
                        NPC.ai[1] = 0f;
                        NPC.localAI[0] = 0f;
                        NPC.rotation = NPC.velocity.X * 0.1f;
                    }
                    else if (NPC.ai[1] <= num20)
                    {
                        NPC.velocity *= 0.95f;
                        NPC.rotation = NPC.velocity.X * 0.15f;
                    }
                    else
                    {
                        NPC.rotation += NPC.direction * 0.5f;
                    }
                }

                if (flag5)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.localAI[0] = 0f;
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[2] = 0f;
                    NPC.netUpdate = true;
                }

                return;
            }
            #endregion
            #region Phase 3 - only dashes
            if (NPC.ai[0] == 2f) //phase 3 - only dashes
            {
                if (NPC.ai[1] < num12)
                {
                    NPC.ai[1] += 1f;
                    NPC.rotation = NPC.velocity.X * 0.1f;

                    if (!NPC.dontTakeDamage)
                    {
                        NPC.localAI[0] += 1f;
                        if (NPC.localAI[0] >= 120f)
                        {
                            NPC.localAI[0] = 0f;
                            NPC.TargetClosest();
                            if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                            {
                                //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                                //SoundEngine.PlaySound(in style, NPC.Center);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int num53 = bossRushActive ? 18 : 12;
                                    float num54 = MathF.PI * 2f / num53;
                                    float num56 = 9f + num2;
                                    Vector2 spinningpoint5 = new Vector2(0f, 0f - num56);
                                    for (int num57 = 0; num57 < num53; num57++)
                                    {
                                        Vector2 vector6 = spinningpoint5.RotatedBy(num54 * num57);
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector6) * 30f, vector6, fireBarrage, NPC.GetProjectileDamageClamity(fireBarrage), 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                    }

                    Vector2 vector7 = new Vector2(NPC.Center.X, NPC.Center.Y);
                    float num58 = player.Center.X - vector7.X;
                    float num59 = player.Center.Y - vector7.Y;
                    float num60 = (float)Math.Sqrt(num58 * num58 + num59 * num59);
                    num60 = ((flag2 ? 7f : 6f) + 4f * num2) / num60;
                    num58 *= num60;
                    num59 *= num60;
                    float num61 = 50f;
                    if (Main.getGoodWorld)
                    {
                        num61 *= 0.5f;
                    }

                    NPC.velocity.X = (NPC.velocity.X * num61 + num58) / (num61 + 1f);
                    NPC.velocity.Y = (NPC.velocity.Y * num61 + num59) / (num61 + 1f);
                }
                else if (NPC.ai[1] < num19)
                {
                    NPC.ai[1] += 1f;
                    float num62 = 2f;
                    if ((NPC.ai[1] - num12) % (num14 / num62) == 0f && Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                        //SoundEngine.PlaySound(in style, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float num64 = 9f + num2;
                            float num65 = num64 - calamityGlobalNPC.newAI[0] * num64 * 0.5f;
                            int num66 = calamityGlobalNPC.newAI[1] == 0f ? 8 : 4;
                            int num67 = (int)(num66 * 0.4f);
                            int num68 = num66 - (int)Math.Round(calamityGlobalNPC.newAI[0] * num67);
                            for (int num69 = 0; num69 < 3; num69++)
                            {
                                float num70 = MathF.PI * 2f / num68;
                                float num71 = num65 - num65 * 0.33f * num69;
                                double num72 = (double)num70 * 0.5;
                                double a3 = (double)MathHelper.ToRadians(90f) - num72;
                                float num73 = (float)((double)num71 * Math.Sin(num72) / Math.Sin(a3));
                                Vector2 spinningpoint6 = num69 == 1 ? new Vector2(0f, 0f - num71) : new Vector2(0f - num73, 0f - num71);
                                for (int num74 = 0; num74 < num68; num74++)
                                {
                                    Vector2 vector8 = spinningpoint6.RotatedBy(num70 * num74);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector8) * 30f, vector8, fireBarrage, NPC.GetProjectileDamageClamity(fireBarrage), 0f, Main.myPlayer, 0f, num65);
                                }
                            }
                        }
                    }

                    calamityGlobalNPC.newAI[0] += num15;
                    NPC.rotation += calamityGlobalNPC.newAI[0];
                    NPC.velocity *= 0.98f;
                }
                else
                {
                    if (NPC.ai[1] == num19)
                    {
                        float num75 = Vector2.Distance(NPC.Center, player.Center) / num13 * 2f;
                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * (num75 + num2 * 2f);
                        if (NPC.velocity.Length() < num17)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= num17;
                        }

                        if (NPC.velocity.Length() > num18)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= num18;
                        }

                        NPC.ai[1] = num19 + num13;
                        calamityGlobalNPC.newAI[0] = 0f;
                    }

                    NPC.ai[1] -= 1f;
                    if (NPC.ai[1] == num19)
                    {
                        NPC.TargetClosest();
                        calamityGlobalNPC.newAI[1] += 1f;
                        if (calamityGlobalNPC.newAI[1] > 1f)
                        {
                            NPC.ai[1] = 0f;
                            NPC.localAI[0] = 0f;
                            calamityGlobalNPC.newAI[1] = 0f;
                        }
                        else
                        {
                            NPC.ai[1] = num12;
                        }

                        NPC.rotation = NPC.velocity.X * 0.1f;
                    }
                    else if (NPC.ai[1] <= num20)
                    {
                        NPC.velocity *= 0.95f;
                        NPC.rotation = NPC.velocity.X * 0.15f;
                    }
                    else
                    {
                        NPC.rotation += NPC.direction * 0.5f;
                    }
                }

                if (flag6)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = 0f;
                    NPC.localAI[0] = 0f;
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;
                    NPC.netUpdate = true;
                }

                return;
            }
            #endregion
            #region Phase 4 - slow following and teleport
            if (NPC.ai[0] == 3f) // Phase 4 -
            {
                NPC.rotation = NPC.velocity.X * 0.1f;
                NPC.localAI[0] += 1f;
                if (NPC.localAI[0] >= 90f && NPC.Opacity == 1f)
                {
                    NPC.localAI[0] = 0f;
                    if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                        //SoundEngine.PlaySound(in style, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int num76 = bossRushActive ? 18 : 12;
                            float num77 = MathF.PI * 2f / num76;
                            float num79 = 10f + num2;
                            Vector2 spinningpoint7 = new Vector2(0f, 0f - num79);
                            for (int num80 = 0; num80 < num76; num80++)
                            {
                                Vector2 vector9 = spinningpoint7.RotatedBy(num77 * num80);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector9) * 30f, vector9, fireBarrage, NPC.GetProjectileDamageClamity(fireBarrage), 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                Vector2 vector10 = new Vector2(NPC.Center.X, NPC.Center.Y);
                float num81 = player.Center.X - vector10.X;
                float num82 = player.Center.Y - vector10.Y;
                float num83 = (float)Math.Sqrt(num81 * num81 + num82 * num82);
                num83 = ((flag2 ? 5.5f : 5f) + 3f * num2) / num83;
                num81 *= num83;
                num82 *= num83;
                float num84 = 50f;
                if (Main.getGoodWorld)
                {
                    num84 *= 0.5f;
                }

                NPC.velocity.X = (NPC.velocity.X * num84 + num81) / (num84 + 1f);
                NPC.velocity.Y = (NPC.velocity.Y * num84 + num82) / (num84 + 1f);
                if (NPC.ai[1] == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.localAI[2] += 1f;
                        if (NPC.localAI[2] >= 180f)
                        {
                            NPC.TargetClosest();
                            NPC.localAI[2] = 0f;
                            int num85 = 0;
                            do
                            {
                                num85++;
                                int num86 = (int)player.Center.X / 16;
                                int num87 = (int)player.Center.Y / 16;
                                int minValue = 16;
                                int maxValue = 20;
                                num86 = !Main.rand.NextBool(2) ? num86 - Main.rand.Next(minValue, maxValue) : num86 + Main.rand.Next(minValue, maxValue);
                                num87 = !Main.rand.NextBool(2) ? num87 - Main.rand.Next(minValue, maxValue) : num87 + Main.rand.Next(minValue, maxValue);
                                if (!WorldGen.SolidTile(num86, num87) && Collision.CanHit(new Vector2(num86 * 16, num87 * 16), 1, 1, player.position, player.width, player.height))
                                {
                                    NPC.ai[1] = 1f;
                                    teleportLocationX = num86;
                                    calamityGlobalNPC.newAI[2] = num87;
                                    NPC.netUpdate = true;
                                    break;
                                }
                            }
                            while (num85 <= 100);
                        }
                    }
                }
                else if (NPC.ai[1] == 1f)
                {
                    NPC.damage = 0;
                    Vector2 position = new Vector2(teleportLocationX * 16f - NPC.width / 2, calamityGlobalNPC.newAI[2] * 16f - NPC.height / 2);
                    for (int num88 = 0; num88 < 5; num88++)
                    {
                        int num89 = Dust.NewDust(position, NPC.width, NPC.height, type, 0f, 0f, 100, default, 2f);
                        Main.dust[num89].noGravity = true;
                    }

                    NPC.Opacity -= 0.008f;
                    if (NPC.Opacity <= 0f)
                    {
                        NPC.Opacity = 0f;
                        NPC.position = position;
                        for (int num90 = 0; num90 < 15; num90++)
                        {
                            int num91 = Dust.NewDust(NPC.position, NPC.width, NPC.height, type, 0f, 0f, 100, default, 3f);
                            Main.dust[num91].noGravity = true;
                        }

                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                        {
                            NPC.localAI[0] = 0f;
                            //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                            //SoundEngine.PlaySound(in style, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int projectileDamage8 = NPC.GetProjectileDamageClamity(fireBarrageHoming);
                                float num93 = 9f + num2;
                                for (int num94 = 0; num94 < 3; num94++)
                                {
                                    int num95 = bossRushActive ? 9 : 6;
                                    float num96 = MathF.PI * 2f / num95;
                                    float num97 = num93 - num93 * 0.33f * num94;
                                    float num98 = 0f;
                                    if (num94 > 0)
                                    {
                                        double num99 = (double)num96 * 0.33 * (3 - num94);
                                        double a4 = (double)MathHelper.ToRadians(90f) - num99;
                                        num98 = (float)((double)num97 * Math.Sin(num99) / Math.Sin(a4));
                                    }

                                    Vector2 spinningpoint8 = num94 == 0 ? new Vector2(0f, 0f - num97) : new Vector2(0f - num98, 0f - num97);
                                    for (int num100 = 0; num100 < num95; num100++)
                                    {
                                        Vector2 vector11 = spinningpoint8.RotatedBy(num96 * num100);
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector11) * 30f, vector11, fireBarrageHoming, projectileDamage8, 0f, Main.myPlayer, 0f, num93);
                                    }
                                }
                            }
                        }

                        NPC.ai[1] = 2f;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.ai[1] == 2f)
                {
                    NPC.damage = 0;
                    NPC.Opacity += 0.2f;
                    if (NPC.Opacity >= 1f)
                    {
                        NPC.Opacity = 1f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;
                    }
                }

                if (flag7)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 4f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[0] = 0f;
                    NPC.localAI[2] = 0f;
                    NPC.Opacity = 1f;
                    teleportLocationX = 0;
                    calamityGlobalNPC.newAI[2] = 0f;
                    NPC.netUpdate = true;
                    //int consequent = 100;
                    /*if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
                    {
                        consequent = 20;
                    }

                    if (Main.zenithWorld)
                    {
                        consequent = 1;
                    }

                    if (Main.rand.NextBool(consequent))
                    {
                        string key = "Mods.CalamityMod.Status.Boss.PyrogenBossText";
                        Color value = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, value);
                    }*/
                }

                return;
            }
            #endregion
            #region Phase 5 - slow dashes around player
            if (NPC.ai[0] == 4f) //phase 5
            {
                if (flag8)
                {
                    if (NPC.ai[1] == 60f)
                    {
                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * (18f + num2 * 2f);
                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                        {
                            //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                            //SoundEngine.PlaySound(in style, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float num102 = 1.5f + num2 * 0.5f;
                                int num103 = flag9 ? 3 : 2;
                                for (int num104 = 0; num104 < num103; num104++)
                                {
                                    int num105 = bossRushActive ? 3 : 2;
                                    float num106 = MathF.PI * 2f / num105;
                                    float num107 = num102 - num102 * (flag9 ? 0.25f : 0.5f) * num104;
                                    float num108 = 0f;
                                    float ai = Main.zenithWorld ? 2f : NPC.target;
                                    if (num104 > 0)
                                    {
                                        double num109 = (double)num106 * (flag9 ? 0.25 : 0.5) * (num103 - num104);
                                        double a5 = (double)MathHelper.ToRadians(90f) - num109;
                                        num108 = (float)((double)num107 * Math.Sin(num109) / Math.Sin(a5));
                                    }

                                    Vector2 spinningpoint9 = num104 == 0 ? new Vector2(0f, 0f - num107) : new Vector2(0f - num108, 0f - num107);
                                    for (int num110 = 0; num110 < num105; num110++)
                                    {
                                        Vector2 vector12 = spinningpoint9.RotatedBy(num106 * num110);
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector12) * 30f, vector12, fireBarrage, NPC.GetProjectileDamageClamity(fireBarrage), 0f, Main.myPlayer, ai, 1f);
                                    }
                                }
                            }
                        }
                    }

                    NPC.ai[1] -= 1f;
                    if (NPC.ai[1] <= 0f)
                    {
                        NPC.ai[3] += 1f;
                        NPC.TargetClosest();
                        if (NPC.ai[3] > 2f)
                        {
                            NPC.ai[0] = 5f;
                            NPC.ai[1] = 0f;
                            NPC.ai[3] = 0f;
                            calamityGlobalNPC.newAI[3] = 0f;
                        }
                        else
                        {
                            NPC.ai[1] = 60f;
                        }

                        NPC.rotation = NPC.velocity.X * 0.1f;
                    }
                    else if (NPC.ai[1] <= 15f)
                    {
                        NPC.velocity *= 0.95f;
                        NPC.rotation = NPC.velocity.X * 0.15f;
                    }
                    else
                    {
                        NPC.rotation += NPC.direction * 0.5f;
                    }

                    return;
                }

                float num111 = 18f + num2 * 2f;
                Vector2 vector13 = new Vector2(NPC.Center.X + NPC.direction * 20, NPC.Center.Y + 6f);
                float num112 = player.position.X + player.width * 0.5f - vector13.X;
                float num113 = player.Center.Y - vector13.Y;
                float num114 = (float)Math.Sqrt(num112 * num112 + num113 * num113);
                float num115 = num111 / num114;
                num112 *= num115;
                num113 *= num115;
                calamityGlobalNPC.newAI[2] -= 1f;
                float num116 = 300f;
                float num117 = 30f;
                if (num114 < num116 || calamityGlobalNPC.newAI[2] > 0f)
                {
                    if (num114 < num116)
                    {
                        calamityGlobalNPC.newAI[2] = num117;
                    }

                    if (NPC.velocity.Length() < num111)
                    {
                        NPC.velocity.Normalize();
                        NPC.velocity *= num111;
                    }

                    NPC.rotation += NPC.direction * 0.5f;
                    return;
                }

                float num118 = 30f;
                if (Main.getGoodWorld)
                {
                    num118 *= 0.5f;
                }

                NPC.velocity.X = (NPC.velocity.X * num118 + num112) / (num118 + 1f);
                NPC.velocity.Y = (NPC.velocity.Y * num118 + num113) / (num118 + 1f);
                if (globalTimer % 10 == 0)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity * 0.5f, ModContent.ProjectileType<FireBomb>(), NPC.GetProjectileDamageClamity(ModContent.ProjectileType<FireBomb>()), 0);

                if (num114 < num116 + 200f)
                {
                    NPC.velocity.X = (NPC.velocity.X * 9f + num112) / 10f;
                    NPC.velocity.Y = (NPC.velocity.Y * 9f + num113) / 10f;
                }

                if (num114 < num116 + 100f)
                {
                    NPC.velocity.X = (NPC.velocity.X * 4f + num112) / 5f;
                    NPC.velocity.Y = (NPC.velocity.Y * 4f + num113) / 5f;
                }

                NPC.rotation = NPC.velocity.X * 0.15f;
                return;
            }
            #endregion

            NPC.rotation = NPC.velocity.X * 0.1f;
            calamityGlobalNPC.newAI[3] += 1f;
            if (calamityGlobalNPC.newAI[3] >= (bossRushActive ? 50f : 75f))
            {
                calamityGlobalNPC.newAI[3] = 0f;
                //SoundStyle style = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
                //SoundEngine.PlaySound(in style, NPC.Center);
                int num119 = 2;
                float num120 = MathF.PI * 2f / num119;
                float num122 = 6f;
                double num123 = (double)num120 * 0.5;
                double a6 = (double)MathHelper.ToRadians(90f) - num123;
                float x = (float)((double)num122 * Math.Sin(num123) / Math.Sin(a6));
                Vector2 spinningpoint10 = Main.rand.NextBool() ? new Vector2(0f, 0f - num122) : new Vector2(x, 0f - num122);
                for (int num124 = 0; num124 < num119; num124++)
                {
                    Vector2 vector14 = spinningpoint10.RotatedBy(num120 * num124);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(vector14) * 30f, vector14, fireblast, NPC.GetProjectileDamageClamity(fireblast), 0f, Main.myPlayer);
                }
            }

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= (bossRushActive ? 120f : 180f))
            {
                NPC.TargetClosest();
                NPC.ai[0] = 4f;
                NPC.ai[1] = 60f;
                calamityGlobalNPC.newAI[3] = 0f;
                calamityGlobalNPC.newAI[2] = 0f;
                NPC.netUpdate = true;
            }

            float num125 = flag2 ? 5f : 6f;
            float num126 = 0.2f;
            num125 -= num2;
            num126 += 0.07f * num2;
            if (NPC.position.Y > player.position.Y - 375f)
            {
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y *= 0.98f;
                }

                NPC.velocity.Y -= num126;
                if (NPC.velocity.Y > num125)
                {
                    NPC.velocity.Y = num125;
                }
            }
            else if (NPC.position.Y < player.position.Y - 400f)
            {
                if (NPC.velocity.Y < 0f)
                {
                    NPC.velocity.Y *= 0.98f;
                }

                NPC.velocity.Y += num126;
                if (NPC.velocity.Y < 0f - num125)
                {
                    NPC.velocity.Y = 0f - num125;
                }
            }

            if (NPC.position.X + NPC.width / 2 > player.position.X + player.width / 2 + 350f)
            {
                if (NPC.velocity.X > 0f)
                {
                    NPC.velocity.X *= 0.98f;
                }

                NPC.velocity.X -= num126;
                if (NPC.velocity.X > num125)
                {
                    NPC.velocity.X = num125;
                }
            }

            if (NPC.position.X + NPC.width / 2 < player.position.X + player.width / 2 - 350f)
            {
                if (NPC.velocity.X < 0f)
                {
                    NPC.velocity.X *= 0.98f;
                }

                NPC.velocity.X += num126;
                if (NPC.velocity.X < 0f - num125)
                {
                    NPC.velocity.X = 0f - num125;
                }
            }
        }
        private void HandlePhaseTransition(int newPhase)
        {
            //SoundStyle style = (Main.zenithWorld ? SoundID.NPCDeath14 : TransitionSound);
            //SoundEngine.PlaySound(in style, NPC.Center);
            if (Main.netMode != NetmodeID.Server && !Main.zenithWorld)
            {
                int num = newPhase >= 5 ? 3 : newPhase < 3 ? 1 : 2;
                /*for (int i = 1; i < num; i++)
                {
                    Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("CryoChipGore" + i).Type, NPC.scale);
                }*/
            }

            currentPhase = newPhase;
            switch (currentPhase)
            {
                case 2:
                    NPC.defense = 13;
                    NPC.Calamity().DR = 0.27f;
                    break;
                case 3:
                    NPC.defense = 10;
                    NPC.Calamity().DR = 0.21f;
                    break;
                case 4:
                    NPC.defense = 6;
                    NPC.Calamity().DR = 0.12f;
                    break;
                case 5:
                case 6:
                    NPC.defense = 0;
                    NPC.Calamity().DR = 0f;
                    break;
                case 0:
                case 1:
                    break;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float num = (float)base.NPC.width * 0.6f;
            if (num < 10f)
            {
                num = 10f;
            }

            float num2 = (float)base.NPC.height / 100f;
            if (num2 > 2.75f)
            {
                num2 = 2.75f;
            }

            if (FireDrawer == null)
            {
                FireDrawer = new FireParticleSet(int.MaxValue, 1, Color.Red * 1.25f, Color.Red, num, num2);
            }
            else
            {
                FireDrawer.DrawSet(base.NPC.Bottom - Vector2.UnitY * (12f - base.NPC.gfxOffY));
            }

            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (base.NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            base.NPC.DrawBackglow(BackglowColor, 4f, spriteEffects, base.NPC.frame, screenPos);
            Vector2 vector = new Vector2(TextureAssets.Npc[base.NPC.type].Value.Width / 2, TextureAssets.Npc[base.NPC.type].Value.Height / Main.npcFrameCount[base.NPC.type] / 2);
            Vector2 position = base.NPC.Center - screenPos;
            position -= new Vector2(value.Width, value.Height / Main.npcFrameCount[base.NPC.type]) * base.NPC.scale / 2f;
            position += vector * base.NPC.scale + new Vector2(0f, base.NPC.gfxOffY);
            spriteBatch.Draw(value, position, base.NPC.frame, base.NPC.GetAlpha(Color.White), base.NPC.rotation, vector, base.NPC.scale, spriteEffects, 0f);
            return false;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            base.NPC.lifeMax = (int)((float)base.NPC.lifeMax * 0.8f * balance);
            base.NPC.damage = (int)((double)base.NPC.damage * base.NPC.GetExpertDamageMultiplier());
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int type = 235;
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(base.NPC.position, base.NPC.width, base.NPC.height, type, hit.HitDirection, -1f);
            }

            if (base.NPC.life > 0)
            {
                return;
            }

            for (int j = 0; j < 40; j++)
            {
                int num = Dust.NewDust(new Vector2(base.NPC.position.X, base.NPC.position.Y), base.NPC.width, base.NPC.height, type, 0f, 0f, 100, default(Color), 2f);
                Main.dust[num].velocity *= 3f;
                if (Main.rand.NextBool(2))
                {
                    Main.dust[num].scale = 0.5f;
                    Main.dust[num].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }

            for (int k = 0; k < 70; k++)
            {
                int num2 = Dust.NewDust(new Vector2(base.NPC.position.X, base.NPC.position.Y), base.NPC.width, base.NPC.height, type, 0f, 0f, 100, default(Color), 3f);
                Main.dust[num2].noGravity = true;
                Main.dust[num2].velocity *= 5f;
                num2 = Dust.NewDust(new Vector2(base.NPC.position.X, base.NPC.position.Y), base.NPC.width, base.NPC.height, type, 0f, 0f, 100, default(Color), 2f);
                Main.dust[num2].velocity *= 2f;
            }

            /*if (Main.netMode != 2 && !Main.zenithWorld)
            {
                float num3 = (float)Main.rand.Next(-200, 201) / 100f;
                for (int l = 1; l < 4; l++)
                {
                    Gore.NewGore(base.NPC.GetSource_Death(), base.NPC.position, base.NPC.velocity * num3, base.Mod.Find<ModGore>("CryoDeathGore" + l).Type, base.NPC.scale);
                    Gore.NewGore(base.NPC.GetSource_Death(), base.NPC.position, base.NPC.velocity * num3, base.Mod.Find<ModGore>("CryoChipGore" + l).Type, base.NPC.scale);
                }
            }*/
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = 499;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<PyrogenBag>()));
            LeadingConditionRule mainRule = npcLoot.DefineNormalOnlyDropSet();
            int[] itemIDs = new int[5]
            {
                ModContent.ItemType<SearedShredder>(),
                ModContent.ItemType<Obsidigun>(),
                ModContent.ItemType<TheGenerator>(),
                ModContent.ItemType<HellsBells>(),
                ModContent.ItemType<MoltenPiercer>()
            };
            mainRule.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, itemIDs));
            //mainRule.Add(ModContent.ItemType<GlacialEmbrace>(), 10);
            //mainRule.Add(ItemDropRule.Common(ModContent.ItemType<EssenceOfFlame>(), 1, 8, 10));
            mainRule.Add(DropHelper.PerPlayer(ModContent.ItemType<SoulOfPyrogen>()));
            mainRule.Add(ModContent.ItemType<PyroStone>(), DropHelper.NormalWeaponDropRateFraction);
            mainRule.Add(ModContent.ItemType<HellFlare>(), DropHelper.NormalWeaponDropRateFraction);
            npcLoot.Add(ItemDropRule.Common(ItemID.DungeonDesertKey, 3));

            mainRule.Add(ItemDropRule.Common(ModContent.ItemType<ThankYouPainting>(), 100));

            //Mask
            mainRule.Add(ItemDropRule.Common(ModContent.ItemType<PyrogenMask>(), 7));
            //Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<PyrogenRelic>());
            //Trophy
            npcLoot.Add(ModContent.ItemType<PyrogenTrophy>(), 10);
            //Lore
            npcLoot.AddConditionalPerPlayer(() => !ClamitySystem.downedPyrogen, ModContent.ItemType<LorePyrogen>(), ui: true, DropHelper.FirstKillText);
            //GFB drop
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.Hellstone, 1, 1, 9999, hideLootReport: true);
        }

        public override void OnKill()
        {
            CalamityGlobalNPC.SetNewBossJustDowned(base.NPC);

            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0.2f, 20f, 30));
            int index = Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenKillExplosion>(), 0, 0);
            Main.projectile[index].scale = 1f;
            //DownedBossSystem.downedCryogen = true;
            ClamitySystem.downedPyrogen = true;
            CalamityNetcode.SyncWorld();
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle hitbox = target.Hitbox;
            float num = Vector2.Distance(base.NPC.Center, hitbox.TopLeft());
            float num2 = Vector2.Distance(base.NPC.Center, hitbox.TopRight());
            float num3 = Vector2.Distance(base.NPC.Center, hitbox.BottomLeft());
            float num4 = Vector2.Distance(base.NPC.Center, hitbox.BottomRight());
            float num5 = num;
            if (num2 < num5)
            {
                num5 = num2;
            }

            if (num3 < num5)
            {
                num5 = num3;
            }

            if (num4 < num5)
            {
                num5 = num4;
            }

            return num5 <= 40f * base.NPC.scale;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 240);
            }
        }
    }
    //[AutoloadBossHead]
    public class PyrogenShield : ModNPC
    {
        public static readonly SoundStyle BreakSound = new SoundStyle("CalamityMod/Sounds/NPCKilled/CryogenShieldBreak");
        public static Color BackglowColor => new Color(238, 102, 70, 80) * 0.6f;

        public override void SetStaticDefaults()
        {
            //Cryogen;
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.canGhostHeal = false;
            NPC.noTileCollide = true;
            NPC.coldDamage = true;
            //NPC.GetNPCDamageClamity();
            NPC.width = 216;
            NPC.height = 216;
            NPC.scale *= CalamityWorld.death || BossRushEvent.BossRushActive || Main.getGoodWorld ? 1.2f : 1f;
            NPC.DR_NERD(0.6f);
            NPC.lifeMax = CalamityWorld.death ? 3500 : 5000;
            if (BossRushEvent.BossRushActive)
            {
                NPC.lifeMax = 50000;
            }
            //Old           - 700  - 1400  - 10000
            //New           - 3500 - 5000  - 50000
            //Difference    - 2800 - 3600  - 40000

            double num = (double)CalamityServerConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * num);
            NPC.Opacity = 0f;
            //NPC.HitSound = Cryogen.HitSound;
            NPC.DeathSound = BreakSound;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToWater = true;
        }

        public override void AI()
        {
            //NPC.HitSound = (Main.zenithWorld ? SoundID.NPCHit41 : Cryogen.HitSound);
            //NPC.DeathSound = (Main.zenithWorld ? SoundID.NPCDeath14 : BreakSound);
            NPC.Opacity += 0.012f;
            if (NPC.Opacity > 1f)
            {
                NPC.Opacity = 1f;
            }

            NPC.rotation += 0.01f;
            if (NPC.type == ModContent.NPCType<PyrogenShield>())
            {
                int num = (int)NPC.ai[0];
                if (Main.npc[num].active && Main.npc[num].type == ModContent.NPCType<PyrogenBoss>())
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.position = Main.npc[num].Center;
                    NPC.ai[1] = Main.npc[num].velocity.X;
                    NPC.ai[2] = Main.npc[num].velocity.Y;
                    NPC.ai[3] = Main.npc[num].target;
                    NPC.position.X = NPC.position.X - NPC.width / 2;
                    NPC.position.Y = NPC.position.Y - NPC.height / 2;
                }
                else
                {
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
            }

            ref float attackTimer = ref NPC.Calamity().newAI[0];
            ref float randomAttack = ref NPC.Calamity().newAI[1];
            ref float secondRotation = ref NPC.Calamity().newAI[2];
            attackTimer--;
            secondRotation += 0.025f;

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 8 * i + NPC.rotation) * 90f * NPC.scale, DustID.Flare, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 8 * i), Scale: 3f * NPC.scale);
                dust.noGravity = true;
            }

            if (attackTimer < 90)
                NPC.rotation += 0.05f;
            if (attackTimer < 0)
            {
                if (randomAttack == -2) randomAttack = Main.rand.Next(100);
                else if (randomAttack == -1)
                {
                    attackTimer = 200;
                    randomAttack = -2;
                }
                else if (randomAttack >= 0)
                {
                    if (randomAttack < 25)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<Fireblast>(), NPC.GetProjectileDamageClamity(ModContent.ProjectileType<Fireblast>()), 1f, Main.myPlayer);
                        randomAttack = -1;
                    }
                    else if (randomAttack >= 25 && randomAttack < 100)
                    {
                        if (attackTimer % 10 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 4 * i + secondRotation) * 5f, ModContent.ProjectileType<FireBarrage>(), NPC.GetProjectileDamageClamity(ModContent.ProjectileType<FireBarrage>()), 1f, Main.myPlayer);
                            }
                        }
                        if (attackTimer < -200)
                        {
                            randomAttack = -1;
                        }
                    }
                }
                /*switch (randomAttack)
                {
                    case 0:
                        if (attackTimer % 10 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 4 * i + NPC.rotation), ModContent.ProjectileType<FireBarrage>(), NPC.GetProjectileDamage(ModContent.ProjectileType<FireBarrage>()), 1f, Main.myPlayer);
                            }
                        }
                        if (attackTimer < -60)
                        {
                            randomAttack = -1;
                        }

                        break;
                    case 1:
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<Fireblast>(), NPC.GetProjectileDamage(ModContent.ProjectileType<Fireblast>()), 1f, Main.myPlayer);
                        randomAttack = -1;
                        break;

                    case -1:
                        attackTimer = 150;
                        randomAttack = -2;
                        break;
                    case -2:
                        randomAttack = Main.rand.Next(2);
                        break;
                    default:
                        randomAttack = Main.rand.Next(2);
                        break;
                }*/
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle hitbox = target.Hitbox;
            float num = Vector2.Distance(NPC.Center, hitbox.TopLeft());
            float num2 = Vector2.Distance(NPC.Center, hitbox.TopRight());
            float num3 = Vector2.Distance(NPC.Center, hitbox.BottomLeft());
            float num4 = Vector2.Distance(NPC.Center, hitbox.BottomRight());
            float num5 = num;
            if (num2 < num5)
            {
                num5 = num2;
            }

            if (num3 < num5)
            {
                num5 = num3;
            }

            if (num4 < num5)
            {
                num5 = num4;
            }

            if (num5 <= 100f * NPC.scale)
            {
                return NPC.Opacity == 1f;
            }

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(BuffID.OnFire3, 240);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            NPC.DrawBackglow(BackglowColor, 4f, SpriteEffects.None, NPC.frame, screenPos);
            Vector2 vector = new Vector2(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 position = NPC.Center - screenPos;
            position -= new Vector2(value.Width, value.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
            position += vector * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(value, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, vector, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int type = 235;
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, type, hit.HitDirection, -1f);
            }

            if (NPC.life > 0)
            {
                return;
            }

            for (int j = 0; j < 25; j++)
            {
                int num = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, type, 0f, 0f, 100, default, 2f);
                Main.dust[num].velocity *= 3f;
                if (Main.rand.NextBool(2))
                {
                    Main.dust[num].scale = 0.5f;
                    Main.dust[num].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }

            for (int k = 0; k < 50; k++)
            {
                int num2 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, type, 0f, 0f, 100, default, 3f);
                Main.dust[num2].noGravity = true;
                Main.dust[num2].velocity *= 5f;
                num2 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, type, 0f, 0f, 100, default, 2f);
                Main.dust[num2].velocity *= 2f;
            }

            if (Main.netMode == NetmodeID.Server || Main.zenithWorld)
            {
                return;
            }

            int num3 = 16;
            double num4 = MathF.PI * 2f / num3;
            Vector2 spinningpoint = new Vector2(0f, -1f);
            for (int l = 0; l < num3; l++)
            {
                Vector2 vector = spinningpoint.RotatedBy(num4 * l);
                for (int m = 1; m <= 2; m++)
                {
                    float num5 = Main.rand.Next(-200, 201) / 100f;
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Vector2.Normalize(vector) * 80f, vector * new Vector2(NPC.ai[1], NPC.ai[2]) * num5, Mod.Find<ModGore>("PyrogenShieldGore" + m).Type, NPC.scale);
                }
            }
        }
        /*public override void BossHeadRotation(ref float rotation)
        {
            rotation = NPC.rotation;
        }*/
    }
}
