using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Furniture.Paintings;
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
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
    public enum PyrogenAttacks : int
    {
        Spawn = 0,
        Firewall,
        FlameBombs,
        FireballSucking,
        InfernoPillars,

        Death = 99
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

        private int attackTimer;

        private int previosAttack;

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

        public static int FireBlastDamage = 23; // 92; Also applies to GFB darts
        public static int FireRainDamage = 23; // 92; Also applies to GFB darts
        public static int FireBombDamage = 28; // 112; Also applies to GFB fireblasts
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
            writer.Write(attackTimer);
            writer.Write(previosAttack);
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
            attackTimer = reader.ReadInt32();
            previosAttack = reader.ReadInt32();
            for (int i = 0; i < 4; i++)
            {
                NPC.Calamity().newAI[i] = reader.ReadSingle();
            }
        }
        public PyrogenAttacks Attack
        {
            get => (PyrogenAttacks)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Attack = PyrogenAttacks.Spawn;
        }
        public const int FirewallTime = 120;
        public const int FirewallCount = 4;

        public const int SuckingFireballCount = 40;
        public const int SuckingFireballVelocity = 10;

        public override void AI()
        {
            #region PreAttackAI
            //attackTimer++;
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
            bool expertOrBR = Main.expertMode || bossRushActive;
            bool refOrBR = CalamityWorld.revenge || bossRushActive;
            bool deathOrBR = CalamityWorld.death || bossRushActive;

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

            bool isEnraged = biomeEnrageTimer <= 0 || bossRushActive;
            float enragePower = deathOrBR ? 0.5f : 0f;
            if (isEnraged)
            {
                NPC.Calamity().CurrentlyEnraged = !bossRushActive;
                enragePower += 2f;
            }

            if (enragePower > 2f)
            {
                enragePower = 2f;
            }

            if (bossRushActive)
            {
                enragePower = 3f;
            }

            float num3 = NPC.life / (float)NPC.lifeMax;
            bool flag4 = num3 < (refOrBR ? 0.85f : 0.8f) || deathOrBR;
            bool flag5 = num3 < (deathOrBR ? 0.8f : refOrBR ? 0.7f : 0.6f);
            bool flag6 = num3 < (deathOrBR ? 0.6f : refOrBR ? 0.55f : 0.4f);
            bool flag7 = num3 < (deathOrBR ? 0.5f : refOrBR ? 0.45f : 0.3f);
            bool flag8 = num3 < (deathOrBR ? 0.35f : 0.25f) && refOrBR;
            bool flag9 = num3 < (deathOrBR ? 0.25f : 0.15f) && refOrBR;
            int smallFireBall = ModContent.ProjectileType<SmallFireball>();
            int bigFireBall = ModContent.ProjectileType<InfernoFireball>();
            int smallFireBallHoming = ModContent.ProjectileType<SmallFireballHoming>();
            /*int type = 235;
            if (!Main.zenithWorld)
            {
                _ = SoundID.Item28;
            }
            else
            {
                _ = SoundID.Item20;
            }*/

            //NPC.HitSound = (Main.zenithWorld ? SoundID.NPCHit41 : HitSound);
            //NPC.DeathSound = (Main.zenithWorld ? SoundID.NPCDeath14 : DeathSound);
            NPC.damage = NPC.defDamage;
            if ((int)NPC.ai[0] + 1 > currentPhase)
            {
                HandlePhaseTransition((int)NPC.ai[0] + 1);
            }

            /*int num8 = (int)NPC.ai[2] - 1;
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
                    NPC.localAI[1] = deathOrBR ? 840f : expertOrBR ? 1220f : 1580f;
                }

                if (NPC.localAI[1] > 0f)
                {
                    NPC.localAI[1] -= 1f;
                }
            }*/

            CalamityWorld.StopRain(); //honestly pyrogen should just stop rain because hes hot

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
            /*bool flag10 = NPC.ai[1] >= num12;
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
                        int projectileDamage = FireBlastDamage;
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
            }*/
            #endregion

            #region Stage Animations

            /*if (NPC.ai[0] >= 1f && globalTimer % Main.rand.Next(60 / (int)NPC.ai[0], 60 / (int)NPC.ai[0] + 4) < 3)
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
            }*/
            #endregion
            //Start of attack AI

            void SetNextAttack()
            {
                NPC.ai[0]++;
                if (NPC.ai[0] > 3)
                    NPC.ai[0] = 1;
                attackTimer = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
            }
            void SetAttackInt(int b)
            {
                NPC.ai[0] = b;
                attackTimer = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
            }
            void SetAttack(PyrogenAttacks a)
            {
                NPC.ai[0] = (int)a;
                attackTimer = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
            }

            attackTimer++;
            switch (Attack)
            {
                case PyrogenAttacks.Spawn:
                    int appear = 6 * 60;
                    int startFight = 12 * 60;
                    NPC.Center = player.Center - new Vector2(0, 200);
                    if (attackTimer % 10 == 0 && attackTimer <= appear)
                    {
                        Color color = Color.Lerp(Color.Red, Color.Yellow, 1f - attackTimer / 60f);
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, color, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 10f, 0f, 20));
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = MathHelper.Lerp(0, 20, (float)attackTimer / appear);
                    }
                    if (attackTimer == appear)
                    {
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Yellow, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 10f, 40));
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Orange, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 15f, 40));
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 20f, 40));

                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = 40;
                    }
                    if (attackTimer >= startFight)
                    {
                        SetNextAttack();
                    }
                    break;
                case PyrogenAttacks.Firewall:
                    if (attackTimer % FirewallTime == 0)
                    {
                        int randRot = Main.rand.Next(4);
                        int empty = Main.rand.Next(-3, 3);
                        for (int i = -40; i < 40; i++)
                        {
                            if (i >= empty - 1 && i <= empty + 1) continue;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(2000, i * 32).RotatedBy(MathHelper.PiOver2 * randRot), new Vector2(-10, 0).RotatedBy(MathHelper.PiOver2 * randRot), smallFireBall, NPC.GetProjectileDamageClamity(smallFireBall), 0, Main.myPlayer);
                        }
                    }
                    if (attackTimer >= FirewallTime * FirewallCount)
                    {
                        SetNextAttack();
                    }
                    break;
                case PyrogenAttacks.FlameBombs:
                    List<int> list = new List<int>() { 0, 1, 2 };
                    list.Shuffle<int>(NPC.whoAmI);

                    if (attackTimer % (20 / enragePower) == 0 && NPC.ai[2] < list.Count - 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 10).RotatedBy(MathHelper.TwoPi / list.Count * list[(int)NPC.ai[2]]), bigFireBall, NPC.GetProjectileDamageClamity(bigFireBall), 0, Main.myPlayer);
                        NPC.ai[2]++;
                    }
                    if (attackTimer >= 59)
                    {
                        SetNextAttack();
                    }

                    break;
                case PyrogenAttacks.FireballSucking:
                    int velocity = SuckingFireballVelocity;
                    if (attackTimer % FirewallTime == 0)
                    {
                        //int randRot = Main.rand.Next(4);
                        int empty = Main.rand.Next(-10, 10);
                        for (int i = 0; i < SuckingFireballCount; i++)
                        {
                            if (i >= empty - 3 && i <= empty + 3) continue;
                            int barrage = Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                NPC.Center + new Vector2(0, 1000).RotatedBy(MathHelper.TwoPi / SuckingFireballCount * i),
                                new Vector2(0, -velocity).RotatedBy(MathHelper.TwoPi / SuckingFireballCount * i),
                                smallFireBall, NPC.GetProjectileDamageClamity(smallFireBall), 0, Main.myPlayer);
                            Main.projectile[barrage].timeLeft = (int)(1000f / velocity) - 2;
                        }
                    }
                    if (attackTimer >= FirewallTime * FirewallCount)
                    {
                        SetNextAttack();
                    }
                    break;
                case PyrogenAttacks.InfernoPillars:

                    break;

                case PyrogenAttacks.Death:

                    break;
            }

            NPC.rotation = NPC.velocity.X * 0.1f;
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
            //Fire
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

            //Boss itself
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (base.NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            if ((Attack == PyrogenAttacks.Spawn && attackTimer > 6 * 60) || Attack != PyrogenAttacks.Spawn)
            {
                base.NPC.DrawBackglow(BackglowColor, 4f, spriteEffects, base.NPC.frame, screenPos);
                Vector2 vector = new Vector2(TextureAssets.Npc[base.NPC.type].Value.Width / 2, TextureAssets.Npc[base.NPC.type].Value.Height / Main.npcFrameCount[base.NPC.type] / 2);
                Vector2 position = base.NPC.Center - screenPos;
                position -= new Vector2(value.Width, value.Height / Main.npcFrameCount[base.NPC.type]) * base.NPC.scale / 2f;
                position += vector * base.NPC.scale + new Vector2(0f, base.NPC.gfxOffY);
                spriteBatch.Draw(value, position, base.NPC.frame, base.NPC.GetAlpha(Color.White), base.NPC.rotation, vector, base.NPC.scale, spriteEffects, 0f);
            }
            return false;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            base.NPC.lifeMax = (int)((float)base.NPC.lifeMax * 0.8f * balance);
            base.NPC.damage = (int)((double)base.NPC.damage * base.NPC.GetExpertDamageMultiplierClamity());
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
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<InfernoFireball>(), NPC.GetProjectileDamageClamity(ModContent.ProjectileType<InfernoFireball>()), 1f, Main.myPlayer);
                        randomAttack = -1;
                    }
                    else if (randomAttack >= 25 && randomAttack < 100)
                    {
                        if (attackTimer % 10 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 4 * i + secondRotation) * 5f, ModContent.ProjectileType<SmallFireball>(), NPC.GetProjectileDamageClamity(ModContent.ProjectileType<SmallFireball>()), 1f, Main.myPlayer);
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
