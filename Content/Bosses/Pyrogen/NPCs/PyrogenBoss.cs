using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.NPCs;
using CalamityMod.Particles;
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

        private readonly Mod Calamity = ModLoader.GetMod("CalamityMod");

        private int currentPhase = 1;
        private int teleportLocationX = 0;

        public static Color BackglowColor => new Color(238, 102, 70, 80) * 0.6f;

        //public override string Texture => "CalamityMod/NPCs/Cryogen/Cryogen_Phase1";

        public static readonly SoundStyle HitSound = SoundID.NPCHit41;
        public static readonly SoundStyle TransitionSound = new("CalamityMod/Sounds/NPCHit/CryogenPhaseTransitionCrack");
        public static readonly SoundStyle ShieldRegenSound = new("CalamityMod/Sounds/Custom/CryogenShieldRegenerate");
        public static readonly SoundStyle DeathSound = SoundID.NPCDeath14;

        public FireParticleSet FireDrawer = null;

        public int[] Chains = [-1, -1, -1, -1];
        public float shieldDrawTimer;
        public float shieldDrawCounter;

        public int ArenaBox;

        public const int ArenaRadius = 1000;

        private enum Attacks
        {
            Idle,
            InfernoStorm,
            ShardSweep,
            ShardStorm
        };
        private List<Attacks> AttackChoicesChained =
        [
            Attacks.InfernoStorm,
            Attacks.ShardSweep,
            Attacks.ShardStorm
        ];
        private List<Attacks> AttackChoicesMinions =
        [
            Attacks.ShardSweep,
            Attacks.ShardStorm
        ];

        const int chainTime = MoltenChain.ActiveTime;
        const int chainStartTime = 62;
        bool evenChain(NPC NPC) => NPC.ai[1] % (chainTime * 2) >= chainTime;

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
            NPC.LifeMaxNERB(25000, 48000, 300000);
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

            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int shield = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<PyrogenShield>(), ai0: NPC.whoAmI);
                Main.npc[shield].netUpdate = true;
            }
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            ref float attack = ref NPC.ai[0];
            ref float timer = ref NPC.ai[1];
            ref float data = ref NPC.ai[2];
            ref float data2 = ref NPC.ai[3];

            ref float attackChoice = ref NPC.localAI[0];
            ref float attackTimer = ref NPC.localAI[1];
            ref float data3 = ref NPC.localAI[2];

            NPC.damage = NPC.defDamage;

            if (CalamityServerConfig.Instance.BossesStopWeather)
                CalamityWorld.StopRain();

            if (NPC.AnyNPCs(ModContent.NPCType<PyrogenShield>()))
            {
                NPC.dontTakeDamage = true;
            }
            else
            {
                NPC.dontTakeDamage = false;
            }

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

            //move towards player
            if (NPC.ai[0] == 0 || NPC.ai[0] == 1)
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
            if (NPC.ai[0] == 2)
            {

                NPC.velocity = Vector2.Zero;
                timer++;
                if (timer == 2)
                {
                    SoundEngine.PlaySound(CalamityMod.NPCs.Cryogen.Cryogen.ShieldRegenSound, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Chains = [-1, -1, -1, -1];

                        Chains[0] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MoltenChain>(), NPC.damage, 0, ai0: NPC.whoAmI, ai1: 0f);
                        Chains[1] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MoltenChain>(), NPC.damage, 0, ai0: NPC.whoAmI, ai1: MathHelper.PiOver2);
                        Chains[2] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MoltenChain>(), NPC.damage, 0, ai0: NPC.whoAmI, ai1: MathHelper.Pi);
                        Chains[3] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MoltenChain>(), NPC.damage, 0, ai0: NPC.whoAmI, ai1: MathHelper.PiOver2 * 3f);

                        ArenaBox = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenBox>(), NPC.damage, 0f, Main.myPlayer, 0f, NPC.whoAmI);
                    }
                    DustExplode(NPC);
                }
                float chainCycleTime = timer % chainTime;
                if (chainCycleTime > chainStartTime - 10 && chainCycleTime < chainStartTime && attackChoice == (float)Attacks.ShardSweep)
                {
                    timer--;
                }

                if (timer % chainTime == chainStartTime)
                {
                    int even = evenChain(NPC) ? 1 : 0;
                    for (int i = 0; i < Chains.Length; i++)
                    {
                        if (i % 2 == even)
                        {
                            int p = Chains[i];
                            if (p.WithinBounds(Main.maxProjectiles))
                                Main.projectile[p].ai[2] = 200;
                        }
                    }
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
                                    for (int i = 0; i < 48; i++)
                                    {
                                        Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / 24f);

                                        Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 128f;

                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                    }

                                    for (int i = 0; i < 24; i++)
                                    {
                                        Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / 24f);

                                        Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 96f;

                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                    }

                                    for (int i = 0; i < 12; i++)
                                    {
                                        Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / 12f);

                                        Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 64f;

                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                    }

                                    for (int i = 0; i < 6; i++)
                                    {
                                        Vector2 offset = (Vector2.UnitY * NPC.height / 2f).RotatedBy(MathHelper.TwoPi * i / 6f);

                                        Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 32f;

                                        Projectile.NewProjectile( NPC.GetSource_FromAI(), NPC.Center + offset, velocity, ModContent.ProjectileType<FireBomb>(), FireBombDamage, 0);
                                    }
                                }
                            }

                            attackTimer++;
                            if (attackTimer > 120 + 60)
                            {
                                attackChoice = (int)Main.rand.NextFromCollection(AttackChoicesChained);
                                attackTimer = 0;

                                if (Main.netMode == NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                                NPC.netUpdate = true;
                            }
                        }
                        break;
                    case Attacks.InfernoStorm:
                        {
                            if (attackTimer == 0)
                            {
                                SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"));
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(MathHelper.ToRadians(360f / 5 * i)), ModContent.ProjectileType<InfernoFireball>(), FireBlastDamage, 0, ai0: 1);
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
                    case Attacks.ShardSweep:
                        {
                            if (attackTimer == 0)
                            {
                                // Find closest active ice chain in terms of rotation, to originate sweep from
                                int closestChain = -1;
                                int even = evenChain(NPC) ? 1 : 0;
                                for (int i = 0; i < Chains.Length; i++)
                                {
                                    if (i % 2 != even)
                                        continue;
                                    int p = Chains[i];

                                    if (closestChain == -1)
                                        closestChain = p;
                                    else
                                    {
                                        Vector2 oldRotVec = Main.projectile[closestChain].Center - NPC.Center;
                                        Vector2 newRotVec = Main.projectile[p].Center - NPC.Center;
                                        Vector2 toPlayer = NPC.DirectionTo(target.Center);
                                        float oldDif = Math.Abs(ClamityUtils.RotationDifference(oldRotVec, toPlayer));
                                        float newDif = Math.Abs(ClamityUtils.RotationDifference(newRotVec, toPlayer));

                                        if (newDif < oldDif)
                                            closestChain = p;
                                    }
                                }

                                if (closestChain == -1)
                                {
                                    ToIdle();
                                    break;
                                }
                                else
                                {
                                    data3 = closestChain;
                                }

                            }
                            const float totalRotation = MathHelper.TwoPi / 6;
                            const int AttackTime = 60 * 3;
                            if (attackTimer > 0 && attackTimer % 30 == 0)
                                SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound"));

                            if (attackTimer > 0 && attackTimer % 5 == 0)
                            {

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    float progress = attackTimer / AttackTime;
                                    int cIndex = (int)data3;
                                    if (!cIndex.WithinBounds(Main.maxProjectiles))
                                    {
                                        ToIdle();
                                        break;
                                    }
                                    Projectile chain = Main.projectile[cIndex];
                                    Vector2 chainDir = NPC.DirectionTo(chain.Center);
                                    float rotDir = Math.Sign(ClamityUtils.RotationDifference(chainDir, NPC.DirectionTo(target.Center)));

                                    Vector2 velDir = chainDir.RotatedBy(rotDir * totalRotation * progress);
                                    float speed = 5f;
                                    Vector2 vel = velDir * speed;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                }
                            }
                            attackTimer++;
                            if (attackTimer >= AttackTime)
                            {
                                attackChoice = (int)Attacks.Idle;
                                attackTimer = 0;
                            }
                        }
                        break;
                    case Attacks.ShardStorm:
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
                                    for (int i = 0; i < 12; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 3).RotatedBy(MathHelper.ToRadians(360f / 12 * i)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
                                    }
                                    for (int i = 0; i < 12; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 2).RotatedBy(MathHelper.ToRadians(360f / 12 * i + 12)), ModContent.ProjectileType<SmallFireball>(), FireRainDamage, 0, ai0: 1);
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
            if (attack == 3)
            {
                DustExplode(NPC);
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/NPCKilled/CryogenDeath") with { Volume = 2 }, target.Center);
            }
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
            if (BossRushEvent.BossRushActive)
                return;

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
            NPC.coldDamage = true;
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
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            NPC.DrawBackglow(Main.zenithWorld ? Color.Red : PyrogenBoss.BackglowColor, 4f, SpriteEffects.None, NPC.frame, screenPos);

            Vector2 origin = new Vector2(TextureAssets.Npc[Type].Value.Width / 2, TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type] / 2);
            Vector2 drawPos = NPC.Center - screenPos;
            drawPos -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            Color overlay = Main.zenithWorld ? Color.Red : drawColor;
            spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(overlay), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            return false;
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
                    int icyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[icyDust].scale = 0.5f;
                        Main.dust[icyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 50; j++)
                {
                    int icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 3f);
                    Main.dust[icyDust2].noGravity = true;
                    Main.dust[icyDust2].velocity *= 5f;
                    icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust2].velocity *= 2f;
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
    }
}
