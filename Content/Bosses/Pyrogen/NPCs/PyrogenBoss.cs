using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.UI;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using CalamityMod.UI.DialogueDisplay;
using CalamityMod.World;
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
using System.Linq;
using CalamityMod.DataStructures;


namespace Clamity.Content.Bosses.Pyrogen.NPCs
{
    public class PyrogenBossBar : ModBossBar
    {
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active)
            {
                return false;
            }

            life = npc.life;
            lifeMax = npc.lifeMax;
            shield = 0f;
            shieldMax = 0f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc2 = Main.npc[i];
                if (npc2.active && (npc2.type == ModContent.NPCType<PyrogenShield>() || npc2.type == ModContent.NPCType<PyrogenMinion>()))
                {
                    shield += npc2.life;
                    shieldMax += npc2.lifeMax;
                }
            }

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            return npc.ai[0] > 0;
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

        private readonly Mod Calamity = ModLoader.GetMod("CalamityMod");

        private int currentPhase = 1;
        private int teleportLocationX = 0;

        public bool spawnedChains = false;

        public static Color BackglowColor => new Color(238, 102, 70, 80) * 0.6f;

        //public override string Texture => "CalamityMod/NPCs/Cryogen/Cryogen_Phase1";

        public static readonly SoundStyle HitSound = SoundID.NPCHit41;
        public static readonly SoundStyle TransitionSound = new("CalamityMod/Sounds/NPCHit/CryogenPhaseTransitionCrack");
        public static readonly SoundStyle ShieldRegenSound = new("CalamityMod/Sounds/Custom/CryogenShieldRegenerate");
        public static readonly SoundStyle DeathSound = SoundID.NPCDeath14;

        public FireParticleSet FireDrawer = null;

        public static List<List<VerletSimulatedSegment>> Chains
        {
            get;
            internal set;
        }

        public float shieldDrawTimer;
        public float shieldDrawCounter;

        public int ArenaBox;

        public const int ArenaRadius = 1000;

        private enum Attacks
        {
            Idle,
            InfernoStorm,
            ShardSweep,
            FireballStorm
        };
        private List<Attacks> AttackChoicesChainedShield =
        [
            Attacks.InfernoStorm,
            Attacks.FireballStorm
        ];
        private List<Attacks> AttackChoicesChainedMinions =
        [
            Attacks.ShardSweep,
            Attacks.FireballStorm
        ];

        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            if (ModLoader.TryGetMod("Redemption", out var redemption))
                redemption.Call("addElementNPC", 2, Type);

            var fanny = new FannyDialog("Pyrogen", "FannyNuhuh").WithDuration(4f).WithCondition(_ => { return Myself is not null; });
            fanny.Register();
        }

        public static int FireBlastDamage = 23;
        public static int FireRainDamage = 23;
        public static int FireBombDamage = 28;

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 69; // 138
            NPC.npcSlots = 24f;
            NPC.width = 86;
            NPC.height = 88;
            NPC.defense = 15;
            NPC.DR_NERD(0.3f);
            NPC.LifeMaxNERB(14062, 27000, 168750);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 6);
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<PyrogenBossBar>();
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = HitSound;
            NPC.DeathSound = DeathSound;

            if (Main.getGoodWorld)
                NPC.scale *= 0.8f;

            if (Main.zenithWorld)
            {
                NPC.Calamity().VulnerableToHeat = true;
                NPC.Calamity().VulnerableToCold = false;
                NPC.Calamity().VulnerableToSickness = false;
            }
            else
            {
                NPC.Calamity().VulnerableToHeat = false;
                NPC.Calamity().VulnerableToCold = true;
                NPC.Calamity().VulnerableToWater = true;
            }

            if (!Main.dedServ)
            {
                Music = Clamity.mod.GetMusicFromMusicMod("Pyrogen") ?? MusicID.OldOnesArmy;
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
            writer.Write(teleportLocationX);
            writer.Write(NPC.dontTakeDamage);

            writer.Write7BitEncodedInt((int)NPC.localAI[0]);
            writer.Write7BitEncodedInt((int)NPC.localAI[1]);
            writer.Write7BitEncodedInt((int)NPC.localAI[2]);
            writer.Write7BitEncodedInt((int)NPC.localAI[3]);

            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            teleportLocationX = reader.ReadInt32();
            NPC.dontTakeDamage = reader.ReadBoolean();

            NPC.localAI[0] = reader.Read7BitEncodedInt();
            NPC.localAI[1] = reader.Read7BitEncodedInt();
            NPC.localAI[2] = reader.Read7BitEncodedInt();
            NPC.localAI[3] = reader.Read7BitEncodedInt();

            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.localAI[3] = 60;
        }

        public override void AI()
        {
            #region Synced Local Variables
            ref float attack = ref NPC.ai[0];
            ref float timer = ref NPC.ai[1];
            ref float data = ref NPC.ai[2];
            ref float data2 = ref NPC.ai[3];

            ref float attackChoice = ref NPC.localAI[0];
            ref float attackTimer = ref NPC.localAI[1];
            ref float data3 = ref NPC.localAI[2];
            ref float spawnTimer = ref NPC.localAI[3];
            #endregion

            NPC.TargetClosest();
            Player target = Main.player[NPC.target];

            if ((!target.active || target.dead || target.Distance(NPC.Center) > 5000) && attack > 0)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.type == ModContent.ProjectileType<FireBomb>() || proj.type == ModContent.ProjectileType<InfernoFireball>())
                        proj.Kill();
                }

                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.type == ModContent.ProjectileType<Firethrower>() || proj.type == ModContent.ProjectileType<SmallFireball>())
                        proj.Kill();
                }

                NPC.active = false;
            }

            ClamityGlobalNPC.pyrogenBoss = NPC.whoAmI;

            if (shieldDrawCounter == 0)
            {
                shieldDrawTimer++;
                if (shieldDrawTimer >= 200) shieldDrawCounter = 1;
            }
            else
            {
                shieldDrawTimer--;
                if (shieldDrawTimer <= 0) shieldDrawCounter = 0;
            }

            #region Idle
            if (NPC.ai[0] == 0)
            {
                if (BossRushEvent.BossRushActive) //skip straght to fight in Boss Rush
                {
                    attack = 1;
                    return;
                }

                if (!spawnedChains)
                {
                    Chains = [];

                    int segmentCount = 21;
                    for (int i = 0; i < 4; i++)
                    {
                        Chains.Add([]);

                        // Determine how far off the chains should go.
                        Vector2 checkDirection = (MathHelper.TwoPi * i / 4f + MathHelper.PiOver4).ToRotationVector2() * new Vector2(1f, 1.2f);
                        if (checkDirection.Y > 0f)
                            checkDirection.Y *= 0.3f;

                        Vector2 chainStart = NPC.Center;
                        float[] laserScanDistances = new float[16];
                        Collision.LaserScan(chainStart, checkDirection, 16f, 5000f, laserScanDistances);
                        Vector2 chainEnd = chainStart + checkDirection.SafeNormalize(Vector2.UnitY) * (laserScanDistances.Average() + 32f);

                        for (int j = 0; j < segmentCount; j++)
                        {
                            Vector2 chainPosition = Vector2.Lerp(chainStart, chainEnd, j / (float)(segmentCount - 1f));
                            Chains[i].Add(new(chainPosition, j == 0 || j == segmentCount - 1));
                        }
                    }

                    spawnedChains = true;
                }

                NPC.damage = 0;
                NPC.dontTakeDamage = true;
                NPC.ShowNameOnHover = false;

                NPC.boss = false;
                NPC.Calamity().ShouldCloseHPBar = true;
                NPC.Calamity().ProvidesProximityRage = false;
                BossHealthBarManager.Bars.RemoveAll(b => b.NPCIndex == NPC.whoAmI);
            }
            #endregion

            #region Main Fight AI
            else
            {
                if (CalamityServerConfig.Instance.BossesStopWeather)
                    CalamityWorld.StopRain();

                #region Boss Rush Move To Player
                if (NPC.ai[0] == 1)
                {
                    NPC.ai[0] = 0;
                    NPC.rotation = NPC.velocity.X / 15f;
                    timer++;
                    if (timer == 1)
                    {
                        Vector2 pos = target.Center + new Vector2(0, -400);
                        data = pos.X; data2 = pos.Y;

                        if (Main.netMode == NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
                    }
                    else if (NPC.Distance(new Vector2(data, data2)) > 300)
                    {
                        Vector2 pos = new Vector2(data, data2);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, (pos - NPC.Center).SafeNormalize(Vector2.Zero) * 30, 0.05f);

                    }
                    if (NPC.Distance(new Vector2(data, data2)) < 300)
                    {
                        NPC.velocity /= 1.1f;
                    }
                    if (NPC.velocity.Length() < 1 && timer > 20)
                    {
                        NPC.rotation = 0;
                        data = 0;
                        data2 = 0;
                        timer = 0;
                        attack = 2;
                        NPC.velocity *= 0;

                        if (Main.netMode == NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
                    }

                }
                #endregion

                #region Phase 1 - Chained with Shield
                if (NPC.ai[0] == 2)
                {
                    NPC.velocity = Vector2.Zero;

                    if (spawnTimer > 0)
                    {
                        if (spawnTimer % 10 == 0)
                        {
                            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Firebrick, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 10f, 0f, 20));
                            SoundEngine.PlaySound(SoundID.Tink, NPC.Center);
                        }

                        spawnTimer--;
                        return;
                    }

                    if (!spawnedChains)
                    {
                        Chains = [];

                        int segmentCount = 21;
                        for (int i = 0; i < 4; i++)
                        {
                            Chains.Add([]);

                            // Determine how far off the chains should go.
                            Vector2 checkDirection = (MathHelper.TwoPi * i / 4f + MathHelper.PiOver4).ToRotationVector2() * new Vector2(1f, 1.2f);
                            if (checkDirection.Y > 0f)
                                checkDirection.Y *= 0.3f;

                            Vector2 chainStart = NPC.Center;
                            float[] laserScanDistances = new float[16];
                            Collision.LaserScan(chainStart, checkDirection, 16f, 5000f, laserScanDistances);
                            Vector2 chainEnd = chainStart + checkDirection.SafeNormalize(Vector2.UnitY) * (laserScanDistances.Average() + 32f);

                            for (int j = 0; j < segmentCount; j++)
                            {
                                Vector2 chainPosition = Vector2.Lerp(chainStart, chainEnd, j / (float)(segmentCount - 1f));
                                Chains[i].Add(new(chainPosition, j == 0 || j == segmentCount - 1));
                            }
                        }

                        spawnedChains = true;
                    }

                    if (NPC.AnyNPCs(ModContent.NPCType<PyrogenShield>()) || NPC.AnyNPCs(ModContent.NPCType<PyrogenMinion>()))
                    {
                        NPC.dontTakeDamage = true;
                    }
                    else
                    {
                        NPC.dontTakeDamage = false;
                    }

                    timer++;
                    if (timer == 2)
                    {
                        if (!ModLoader.HasMod("InfernumMode"))
                        {
                            if ((Main.player[NPC.target].name is "AlikEspess" or "Alik" or "Алекс Шаррн"))
                                DialogueDisplaySystem.StartDialogue("Mods.Clamity.Pyrogen.Alik", NPC, 0, 120, false, new BossText());
                            else if (Main.zenithWorld)
                                ClamityUtils.BossIntroDialogue("Pyrogen", NPC, "IntroGFB");
                            else
                                ClamityUtils.BossIntroDialogue("Pyrogen", NPC);
                        }

                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 20f, 40));
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            SoundEngine.PlaySound(CalamityMod.NPCs.Cryogen.Cryogen.ShieldRegenSound, NPC.Center);
                            int shield = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<PyrogenShield>(), ai0: NPC.whoAmI);
                            Main.npc[shield].netUpdate = true;

                            ArenaBox = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenBox>(), NPC.damage, 0f, Main.myPlayer, 0f, NPC.whoAmI);
                        }

                        NPC.boss = true;
                        NPC.ShowNameOnHover = true;
                        NPC.damage = 69;

                        DustExplode(NPC);
                    }

                    void ToIdle()
                    {
                        ref float attackChoice = ref NPC.localAI[0];
                        ref float attackTimer = ref NPC.localAI[1];
                        attackChoice = (int)Attacks.Idle;
                        attackTimer = 0;
                    }
                    switch ((Attacks)attackChoice)
                    {
                        case Attacks.Idle:
                            {
                                if (attackTimer == 120)
                                {
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneBigShoot"), NPC.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        int bombs = CalamityWorld.death ? 48 : CalamityWorld.revenge ? 40 : 32;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 128f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 96f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 64f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 32f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }
                                    }
                                }

                                attackTimer++;
                                if (attackTimer > 120 + 60)
                                {
                                    attackChoice = (int)Main.rand.NextFromCollection(AttackChoicesChainedShield);
                                    //attackChoice = (int)Attacks.InfernoStorm; //DEBUG
                                    attackTimer = 0;

                                    if (Main.netMode == NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                                    NPC.netUpdate = true;
                                }
                            }
                            break;
                        case Attacks.InfernoStorm:
                            {
                                if (!CalamityWorld.revenge)
                                {
                                    ToIdle();
                                    break;
                                }

                                if (attackTimer == 0)
                                {
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"));
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        int bolts = CalamityWorld.death ? 5 : 3;
                                        int pattern = Main.rand.NextFromList(-1, 1);
                                        for (int i = 0; i < bolts; i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, pattern).RotatedBy(MathHelper.ToRadians(360f / bolts * i)), ModContent.ProjectileType<InfernoFireball>(), FireBlastDamage, 0, ai0: 1);
                                        }
                                    }
                                }
                                attackTimer++;
                                if (attackTimer > 80)
                                {
                                    ToIdle();
                                }
                            }
                            break;
                        case Attacks.FireballStorm:
                            {
                                if (attackTimer == 0)
                                {
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"));
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        for (int i = 0; i < 12; i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(MathHelper.ToRadians(360f / 12 * i)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                        }

                                        if (CalamityWorld.revenge)
                                        {
                                            for (int i = 0; i < 12; i++)
                                            {
                                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 3).RotatedBy(MathHelper.ToRadians(360f / 12 * i)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                            }
                                        }

                                        if (CalamityWorld.death)
                                        {
                                            for (int i = 0; i < 12; i++)
                                            {
                                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 2).RotatedBy(MathHelper.ToRadians(360f / 12 * i + 12)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                            }
                                        }
                                    }
                                }
                                attackTimer++;
                                if (attackTimer > 30)
                                {
                                    ToIdle();
                                }

                            }
                            break;
                    }
                }
                #endregion

                #region Phase 2 - Chained with Minions
                if (attack == 3)
                {
                    NPC.velocity = Vector2.Zero;

                    timer++;
                    if (timer == 2)
                    {
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 20f, 40));

                        foreach (Projectile proj in Main.projectile)
                        {
                            if (proj.type == ModContent.ProjectileType<FireBomb>() || proj.type == ModContent.ProjectileType<InfernoFireball>())
                                proj.Kill();
                        }

                        foreach (Projectile proj in Main.projectile)
                        {
                            if (proj.type == ModContent.ProjectileType<Firethrower>() || proj.type == ModContent.ProjectileType<SmallFireball>())
                                proj.Kill();
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float radius = 120f;

                            for (int i = 0; i < 6; i++)
                            {
                                float angle = MathHelper.TwoPi * i / 6f;
                                Vector2 offset = angle.ToRotationVector2() * radius;

                                Vector2 spawnPos = NPC.Center + offset;

                                int minion = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<PyrogenMinion>(), ai0: NPC.whoAmI);

                                Main.npc[minion].netUpdate = true;
                            }

                            SoundEngine.PlaySound(CalamityMod.NPCs.Cryogen.Cryogen.DeathSound, NPC.Center);
                        }

                        DustExplode(NPC);
                    }

                    if (NPC.AnyNPCs(ModContent.NPCType<PyrogenShield>()) || NPC.AnyNPCs(ModContent.NPCType<PyrogenMinion>()))
                    {
                        NPC.dontTakeDamage = true;
                    }
                    else
                    {
                        NPC.dontTakeDamage = false;
                    }

                    void ToIdle()
                    {
                        ref float attackChoice = ref NPC.localAI[0];
                        ref float attackTimer = ref NPC.localAI[1];
                        attackChoice = (int)Attacks.Idle;
                        attackTimer = 0;
                    }
                    switch ((Attacks)attackChoice)
                    {
                        case Attacks.Idle:
                            {
                                if (attackTimer == 120)
                                {
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneBigShoot"), NPC.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        int bombs = CalamityWorld.death ? 40 : CalamityWorld.revenge ? 32 : 24;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 128f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 96f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 64f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }

                                        /*
                                        bombs /= 2;

                                        for (int i = 0; i < bombs; i++)
                                        {
                                            Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / bombs);

                                            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 32f;

                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                        }
                                        */
                                    }
                                }

                                attackTimer++;
                                if (attackTimer > 120 + 60)
                                {
                                    attackChoice = (int)Main.rand.NextFromCollection(AttackChoicesChainedMinions);
                                    //attackChoice = (int)Attacks.InfernoStorm; //DEBUG
                                    attackTimer = 0;

                                    if (Main.netMode == NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                                    NPC.netUpdate = true;
                                }
                            }
                            break;
                        case Attacks.ShardSweep:
                            {
                                const float totalRotation = MathHelper.TwoPi / 6f;
                                const int AttackTime = 60 * 3;

                                if (Chains is null || Chains.Count <= 0)
                                {
                                    ToIdle();
                                    break;
                                }

                                if (attackTimer == 0)
                                {
                                    int closestChain = -1;

                                    Vector2 toPlayer = NPC.DirectionTo(target.Center);

                                    for (int i = 0; i < Chains.Count; i++)
                                    {
                                        List<VerletSimulatedSegment> chain = Chains[i];

                                        if (chain is null || chain.Count < 2)
                                            continue;

                                        Vector2 chainEnd = chain[^1].position;
                                        Vector2 chainDirection = NPC.DirectionTo(chainEnd);

                                        if (closestChain == -1)
                                        {
                                            closestChain = i;
                                            continue;
                                        }

                                        Vector2 oldEnd = Chains[closestChain][^1].position;
                                        Vector2 oldDirection = NPC.DirectionTo(oldEnd);

                                        float oldDifference = Math.Abs(ClamityUtils.RotationDifference(oldDirection, toPlayer));
                                        float newDifference = Math.Abs(ClamityUtils.RotationDifference(chainDirection, toPlayer));

                                        if (newDifference < oldDifference)
                                            closestChain = i;
                                    }

                                    if (closestChain == -1)
                                    {
                                        ToIdle();
                                        break;
                                    }

                                    data3 = closestChain;
                                    NPC.netUpdate = true;
                                }

                                if (attackTimer > 0 && attackTimer % 30 == 0)
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"), NPC.Center);

                                if (attackTimer > 0 && attackTimer % 5 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int chainIndex = (int)data3;

                                    if (Chains is null || !chainIndex.WithinBounds(Chains.Count) || Chains[chainIndex].Count < 2)
                                    {
                                        ToIdle();
                                        break;
                                    }

                                    List<VerletSimulatedSegment> chain = Chains[chainIndex];

                                    Vector2 chainEnd = chain[^1].position;
                                    Vector2 chainDir = NPC.DirectionTo(chainEnd);

                                    Vector2 toPlayer = NPC.DirectionTo(target.Center);
                                    float rotDir = Math.Sign(ClamityUtils.RotationDifference(chainDir, toPlayer));

                                    if (rotDir == 0f)
                                        rotDir = Main.rand.NextBool() ? 1f : -1f;

                                    float progress = attackTimer / (float)AttackTime;
                                    Vector2 velDir = chainDir.RotatedBy(rotDir * totalRotation * progress);

                                    float speed = 5f;
                                    Projectile.NewProjectile(
                                        NPC.GetSource_FromAI(),
                                        NPC.Center,
                                        velDir * speed,
                                        ModContent.ProjectileType<SmallFireball>(),
                                        FireRainDamage,
                                        0f,
                                        Main.myPlayer,
                                        ai0: 1f
                                    );
                                }

                                attackTimer++;

                                if (attackTimer >= AttackTime)
                                    ToIdle();

                                break;
                            }
                        case Attacks.FireballStorm:
                        case Attacks.InfernoStorm:
                            {
                                if (attackTimer == 0)
                                {
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"));
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        for (int i = 0; i < 12; i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(MathHelper.ToRadians(360f / 12 * i)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                        }

                                        if (Main.expertMode)
                                        {
                                            for (int i = 0; i < 12; i++)
                                            {
                                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 3).RotatedBy(MathHelper.ToRadians(360f / 12 * i)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                            }
                                        }

                                        if (CalamityWorld.revenge)
                                        {
                                            for (int i = 0; i < 12; i++)
                                            {
                                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 2).RotatedBy(MathHelper.ToRadians(360f / 12 * i + 12)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                            }
                                        }
                                    }
                                }
                                attackTimer++;
                                if (attackTimer > 30)
                                {
                                    ToIdle();
                                }

                            }
                            break;
                    }
                }
                #endregion
            }
            #endregion

            // Update chains.
            if (Chains is not null)
                UpdateChains(NPC);
        }

        public void DustExplode(NPC NPC)
        {
            for (int i = 0; i < 200; i++)
            {
                Vector2 speed = new Vector2(0, Main.rand.Next(0, 15)).RotatedByRandom(MathHelper.TwoPi);
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Torch, speed.X, speed.Y, Scale: 1.5f);
                d.noGravity = true;
            }
        }

        public static void UpdateChains(NPC npc)
        {
            // Get out of here if the chains are not initialized yet.
            if (Chains is null)
                return;

            for (int i = 0; i < Chains.Count; i++)
            {
                // Check to see if a player is moving through the chains.
                foreach (Player p in Main.ActivePlayers)
                {
                    if (p.dead) // No ghost check here. Poltergeist.
                        continue;

                    MoveChainBasedOnEntity(Chains[i], p, npc);
                }

                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.hostile)
                        continue;

                    MoveChainBasedOnEntity(Chains[i], proj, npc);
                }

                Vector2 chainStart = Chains[i][0].position;
                Vector2 chainEnd = Chains[i].Last().position;
                float segmentDistance = Vector2.Distance(chainStart, chainEnd) / Chains[i].Count;
                Chains[i] = VerletSimulatedSegment.SimpleSimulation(Chains[i], segmentDistance, 10, 0.6f);
            }
        }

        public static void MoveChainBasedOnEntity(List<VerletSimulatedSegment> chain, Entity e, NPC npc)
        {
            // Cap the velocity to ensure it doesn't make the chains go flying.
            Vector2 entityVelocity = (e.velocity * 0.425f).ClampMagnitude(0f, 5f);

            for (int i = 1; i < chain.Count - 1; i++)
            {
                VerletSimulatedSegment segment = chain[i];
                VerletSimulatedSegment next = chain[i + 1];

                // Check to see if the entity is between two verlet segments via line/box collision checks.
                // If they are, add the entity's velocity to the two segments relative to how close they are to each of the two.
                float _ = 0f;
                if (Collision.CheckAABBvLineCollision(e.TopLeft, e.Size, segment.position, next.position, 20f, ref _))
                {
                    // Weigh the entity's distance between the two segments.
                    // If they are close to one point that means the strength of the movement force applied to the opposite segment is weaker, and vice versa.
                    float distanceBetweenSegments = segment.position.Distance(next.position);
                    float distanceToChains = e.Distance(segment.position);
                    float currentMovementOffsetInterpolant = Utils.GetLerpValue(distanceToChains, distanceBetweenSegments, distanceBetweenSegments * 0.2f, true);
                    float nextMovementOffsetInterpolant = 1f - currentMovementOffsetInterpolant;

                    // Move the segments based on the weight values.
                    segment.position += entityVelocity * currentMovementOffsetInterpolant;
                    if (!next.locked)
                        next.position += entityVelocity * nextMovementOffsetInterpolant;

                    // Play some cool chain sounds.
                    if (npc.soundDelay <= 0 && entityVelocity.Length() >= 0.1f)
                    {
                        SoundEngine.PlaySound(SoundID.Coins with { Volume = 0.75f, PitchVariance = 0.05f }, e.Center);
                        npc.soundDelay = 27;
                    }
                }
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dusttype = 235;
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 40; i++)
                {
                    int icyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[icyDust].scale = 0.5f;
                        Main.dust[icyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int j = 0; j < 70; j++)
                {
                    int icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 3f);
                    Main.dust[icyDust2].noGravity = true;
                    Main.dust[icyDust2].velocity *= 5f;
                    icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust2].velocity *= 2f;
                }
                /*
                if (!Main.dedServ && !Main.zenithWorld)
                {
                    float randomSpread = Main.rand.Next(-200, 201) / 100f;
                    for (int i = 1; i < 4; i++)
                    {
                        //Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * randomSpread, Mod.Find<ModGore>("CryoDeathGore" + i).Type, NPC.scale);
                        //Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * randomSpread, Mod.Find<ModGore>("CryoChipGore" + i).Type, NPC.scale);
                    }
                }
                */
            }
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<PyrogenBag>()));

            var normalOnly = npcLoot.DefineNormalOnlyDropSet();
            {
                int[] weapons = new int[]
                {
                    ModContent.ItemType<SearedShredder>(),
                    ModContent.ItemType<Obsidigun>(),
                    ModContent.ItemType<TheGenerator>(),
                    ModContent.ItemType<HellsBells>(),
                    ModContent.ItemType<MoltenPiercer>()
                };
                normalOnly.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, weapons));

                normalOnly.Add(ItemDropRule.Common(ModContent.ItemType<PyrogenMask>(), 7));
                normalOnly.Add(ItemDropRule.Common(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt));

                // item has been chosen as the "Expert gatekept" item for this boss
                //normalOnly.Add(DropHelper.PerPlayer(ModContent.ItemType<SoulOfPyrogen>()));
                normalOnly.Add(ModContent.ItemType<PyroStone>(), DropHelper.NormalWeaponDropRateFraction);
                normalOnly.Add(ModContent.ItemType<HellFlare>(), DropHelper.NormalWeaponDropRateFraction);
            }

            //Trophy
            npcLoot.Add(ModContent.ItemType<PyrogenTrophy>(), 10);

            //Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<PyrogenRelic>());

            //Lore
            npcLoot.AddConditionalPerPlayer(() => !ClamitySystem.downedPyrogen, ModContent.ItemType<LorePyrogen>(), ui: true, DropHelper.FirstKillText);

            //GFB drop
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.Hellstone, 1, 1, 9999, hideLootReport: true);
        }

        public override void OnKill()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.type == ModContent.ProjectileType<FireBomb>() || proj.type == ModContent.ProjectileType<InfernoFireball>())
                    proj.Kill();
            }

            foreach (Projectile proj in Main.projectile)
            {
                if (proj.type == ModContent.ProjectileType<Firethrower>() || proj.type == ModContent.ProjectileType<SmallFireball>())
                    proj.Kill();
            }

            if (BossRushEvent.BossRushActive)
                return;

            AzafureFurnaceTile.WaitingForPlayersToLeaveArea = true;

            CalamityGlobalNPC.SetNewBossJustDowned(NPC);

            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0.2f, 20f, 30));
            int index = Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenKillExplosion>(), 0, 0);
            Main.projectile[index].scale = 1f;

            // Mark Pyrogen as dead
            ClamitySystem.downedPyrogen = true;
            CalamityNetcode.SyncWorld();
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetHitbox = target.Hitbox;

            float hitboxTopLeft = Vector2.Distance(NPC.Center, targetHitbox.TopLeft());
            float hitboxTopRight = Vector2.Distance(NPC.Center, targetHitbox.TopRight());
            float hitboxBotLeft = Vector2.Distance(NPC.Center, targetHitbox.BottomLeft());
            float hitboxBotRight = Vector2.Distance(NPC.Center, targetHitbox.BottomRight());

            float minDist = hitboxTopLeft;
            if (hitboxTopRight < minDist)
                minDist = hitboxTopRight;
            if (hitboxBotLeft < minDist)
                minDist = hitboxBotLeft;
            if (hitboxBotRight < minDist)
                minDist = hitboxBotRight;

            return minDist <= 40f * NPC.scale;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw chains.
            DrawChains(Color.White);

            return true;
        }

        public static void DrawChain(List<VerletSimulatedSegment> chain, Color colorFactor)
        {
            Texture2D chainTexture = ModContent.Request<Texture2D>("Clamity/Content/Bosses/Pyrogen/Projectiles/MoltenChain").Value;

            // Collect chain draw positions.
            Vector2[] bezierPoints = chain.Select(x => x.position).ToArray();
            BezierCurve bezierCurve = new(bezierPoints);

            float chainScale = 0.8f;
            int totalChains = (int)(Vector2.Distance(chain.First().position, chain.Last().position) / chainTexture.Height / chainScale);
            totalChains = (int)MathHelper.Clamp(totalChains, 30f, 1200f);
            for (int i = 0; i < totalChains - 1; i++)
            {
                Vector2 drawPosition = bezierCurve.Evaluate(i / (float)totalChains);
                float completionRatio = i / (float)totalChains + 1f / totalChains;
                float angle = (bezierCurve.Evaluate(completionRatio) - drawPosition).ToRotation();
                Color baseChainColor = Lighting.GetColor((int)drawPosition.X / 16, (int)drawPosition.Y / 16) * 2f;
                Main.EntitySpriteDraw(chainTexture, drawPosition - Main.screenPosition, null, baseChainColor.MultiplyRGBA(colorFactor), angle, chainTexture.Size() * 0.5f, chainScale, SpriteEffects.None, 0);
            }
        }


        public static void DrawChains(Color colorFactor)
        {
            if (Chains is not null)
            {
                foreach (var chain in Chains)
                    DrawChain(chain, colorFactor);
            }
        }
    }
    //[AutoloadBossHead]
    public class PyrogenShield : ModNPC
    {
        public static readonly SoundStyle BreakSound = new("CalamityMod/Sounds/NPCKilled/CryogenShieldBreak");

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 60; // 120
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.noTileCollide = true;
            NPC.width = 216;
            NPC.height = 216;
            NPC.scale *= (CalamityWorld.death || BossRushEvent.BossRushActive || Main.getGoodWorld) ? 0.8f : 1f;
            NPC.DR_NERD(0.4f);
            NPC.LifeMaxNERB(2800, 3360, 33600);
            NPC.Opacity = 0f;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath14;
            if (Main.zenithWorld)
            {
                NPC.Calamity().VulnerableToHeat = true;
                NPC.Calamity().VulnerableToCold = false;
                NPC.Calamity().VulnerableToSickness = false;
            }
            else
            {
                NPC.Calamity().VulnerableToHeat = false;
                NPC.Calamity().VulnerableToCold = true;
                NPC.Calamity().VulnerableToWater = true;
            }
        }

        public override void AI()
        {
            NPC.Opacity += 0.01f;
            if (NPC.Opacity >= 1f)
            {
                NPC.damage = NPC.defDamage;
                NPC.Opacity = 1f;
            }
            else
                NPC.damage = 0;

            NPC.rotation += 0.15f;
            NPC.scale = 1.5f;

            if (NPC.type == ModContent.NPCType<PyrogenShield>())
            {
                int mainPyrogen = (int)NPC.ai[0];
                if (Main.npc[mainPyrogen].active && Main.npc[mainPyrogen].type == ModContent.NPCType<PyrogenBoss>())
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.position = Main.npc[mainPyrogen].Center;
                    NPC.ai[1] = Main.npc[mainPyrogen].velocity.X;
                    NPC.ai[2] = Main.npc[mainPyrogen].velocity.Y;
                    NPC.ai[3] = Main.npc[mainPyrogen].target;
                    NPC.position.X = NPC.position.X - (NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                    return;
                }

                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetHitbox = target.Hitbox;

            float hitboxTopLeft = Vector2.Distance(NPC.Center, targetHitbox.TopLeft());
            float hitboxTopRight = Vector2.Distance(NPC.Center, targetHitbox.TopRight());
            float hitboxBotLeft = Vector2.Distance(NPC.Center, targetHitbox.BottomLeft());
            float hitboxBotRight = Vector2.Distance(NPC.Center, targetHitbox.BottomRight());

            float minDist = hitboxTopLeft;
            if (hitboxTopRight < minDist)
                minDist = hitboxTopRight;
            if (hitboxBotLeft < minDist)
                minDist = hitboxBotLeft;
            if (hitboxBotRight < minDist)
                minDist = hitboxBotRight;

            return minDist <= (100f * NPC.scale) && NPC.Opacity >= 1f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC.DrawBackglow(PyrogenBoss.BackglowColor, 4f, SpriteEffects.None, NPC.frame, screenPos);

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(TextureAssets.Npc[Type].Value.Width / 2, TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type] / 2);
            Vector2 drawPos = NPC.Center - screenPos;
            drawPos -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsOverPlayers.Add(index);
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override bool CheckActive() => false;

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dusttype = 235;
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    int fireDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[fireDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[fireDust].scale = 0.5f;
                        Main.dust[fireDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 50; j++)
                {
                    int fireDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 3f);
                    Main.dust[fireDust2].noGravity = true;
                    Main.dust[fireDust2].velocity *= 5f;
                    fireDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[fireDust2].velocity *= 2f;
                }

                if (!Main.dedServ && !Main.zenithWorld)
                {
                    int totalGores = 16;
                    double radians = MathHelper.TwoPi / totalGores;
                    Vector2 spinningPoint = new Vector2(0f, -1f);
                    for (int k = 0; k < totalGores; k++)
                    {
                        Vector2 goreRotation = spinningPoint.RotatedBy(radians * k);
                        for (int x = 1; x <= 2; x++)
                        {
                            float randomSpread = Main.rand.Next(-200, 201) / 100f;
                            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Vector2.Normalize(goreRotation) * 80f, goreRotation * new Vector2(NPC.ai[1], NPC.ai[2]) * randomSpread, Mod.Find<ModGore>("PyrogenShieldGore" + x).Type, NPC.scale);
                        }
                    }
                }
            }
        }

        public override void OnKill()
        {
            if (Main.npc[ClamityGlobalNPC.pyrogenBoss].ai[0] == 2)
            {
                Main.npc[ClamityGlobalNPC.pyrogenBoss].ai[0] = 3;
                Main.npc[ClamityGlobalNPC.pyrogenBoss].ai[1] = 0;
            }
        }
    }
}
