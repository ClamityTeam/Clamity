using CalamityMod;
using CalamityMod.BiomeManagers;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.SunkenSea;
using Clamity.Content.Bosses.Clamitas.Drop;
using Clamity.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Clamity.Content.Bosses.Clamitas.NPCs
{
    [AutoloadBossHead]
    public partial class ClamitasBoss : ModNPC
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
        public static readonly SoundStyle SlamSound = new SoundStyle("CalamityMod/Sounds/Item/ClamImpact");
        private int biomeEnrageTimer = 300;

        //Used for animations
        private bool attackAnim;
        private bool hide;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 12;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers nPCBestiaryDrawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers();
            nPCBestiaryDrawModifiers.Scale = 0.4f;
            NPCID.Sets.NPCBestiaryDrawModifiers value = nPCBestiaryDrawModifiers;
            value.Position.Y += 40f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;




            //var fanny1 = new FannyDialog("Clamitas", "FannyNuhuh").WithDuration(4f).WithCondition(_ => { return Myself is not null; });

            //fanny1.Register();

        }

        public static int DartDamage = 19; // 76
        public static int HellblastDamage = 24; // 96
        public static int HellfireballDamage = 24; // 96
        public static int RayDamage = 35; // 140

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 5f;
            NPC.width = 160;
            NPC.height = 120;
            NPC.aiStyle = -1;
            AIType = -1;
            //base.NPC.value = (Main.hardMode ? Item.buyPrice(0, 8) : Item.buyPrice(0, 1));
            NPC.value = Item.buyPrice(0, 20);
            NPC.HitSound = SoundID.NPCHit4;
            NPC.boss = true;


            NPC.damage = 50;
            //NPC.defense = 9999;
            NPC.defense = Main.hardMode ? 35 : 10;
            NPC.DR_NERD(0.3f);
            //base.NPC.lifeMax = (Main.hardMode ? 7500 : 1250);
            //NPC.lifeMax = 50000;
            NPC.LifeMaxNERB(50000, 58000, 500000);
            //base.NPC.value = (Main.hardMode ? Item.buyPrice(0, 8) : Item.buyPrice(0, 1));
            NPC.value = Item.buyPrice(0, 20);
            NPC.knockBackResist = 0f;
            NPC.rarity = 0;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<BrimstoneCragsBiome>().Type };

            /*if (!Main.dedServ)
            {
                //Music = 0;
                Music = Clamity.mod.GetMusicFromMusicMod("Clamitas") ?? MusicID.Boss3;
            }*/
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[1]
            {
                new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.ClamitasBoss.Bestiary")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(biomeEnrageTimer);
            writer.Write(attack);
            writer.Write(attackAnim);
            writer.Write(NPC.dontTakeDamage);
            writer.Write(NPC.chaseable);
            writer.Write(hide);
            //writer.Write(flareFrame);
            //writer.Write(flareFrameCounter);
            for (int i = 0; i < 2; i++)
            {
                writer.Write(NPC.Calamity().newAI[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            biomeEnrageTimer = reader.ReadInt32();
            attack = reader.ReadInt32();
            attackAnim = reader.ReadBoolean();
            NPC.dontTakeDamage = reader.ReadBoolean();
            NPC.chaseable = reader.ReadBoolean();
            hide = reader.ReadBoolean();
            //flareFrame = reader.ReadInt32();
            //flareFrameCounter = reader.ReadInt32();
            for (int i = 0; i < 2; i++)
            {
                NPC.Calamity().newAI[i] = reader.ReadSingle();
            }
        }




        /*public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }

            return null;
        }*/
        /// <summary>
        /// Frames
        /// 0-3 closed idle
        /// 4-7 opening
        /// 8-11 open idle
        /// 13-14 clisng
        /// </summary>
        /// <param name="frameHeight"></param>
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > 2)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = NPC.frame.Y + frameHeight;
            }

            if (hide)
            {
                if (NPC.IsABestiaryIconDummy)
                {
                    if (NPC.frame.Y >= frameHeight * 3)
                    {
                        NPC.frame.Y = frameHeight * 0;
                    }
                }
                else
                {
                    if (NPC.frame.Y >= frameHeight * 8)
                    {
                        if (NPC.frame.Y >= frameHeight * 14)
                        {
                            NPC.frame.Y = frameHeight * 0;
                        }
                    }
                    else
                    {
                        if (NPC.frame.Y >= frameHeight * 3)
                        {
                            NPC.frame.Y = frameHeight * 0;
                        }
                    }
                }
            }
            else
            {
                if (NPC.frame.Y >= frameHeight * 11)
                {
                    NPC.frame.Y = frameHeight * 8;
                }
            }
        }

        /*public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneCalamity && DownedBossSystem.downedCalamitasClone && !NPC.AnyNPCs(ModContent.NPCType<ClamitasBoss>()))
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.24f;
            }

            return 0f;
        }*/

        /*public override void ModifyTypeName(ref string typeName)
        {
            if (clamitasForm || NPC.IsABestiaryIconDummy)
            {
                typeName = Language.GetTextValue($"Mods.Clamity.NPCs.{nameof(ClamitasBoss)}.DisplayName");
            }
            else
            {
                typeName = Language.GetTextValue($"Mods.Clamity.NPCs.{nameof(ClamitasBoss)}.DisplayNameAlt");
            }
        }*/

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Obsidian, hit.HitDirection, -1f);
            }

            if (NPC.life <= 0)
            {
                for (int j = 0; j < 50; j++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Obsidian, hit.HitDirection, -1f);
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 1; i < 6; i++)
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Clamitas"+i.ToString()).Type);
                }
            }
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Extra").Value, NPC.Center - Vector2.UnitY * 20f * NPC.scale - screenPos, new Rectangle(0, ((int)(Main.GlobalTimeWrappedHourly * 7) % 6) * 174, 116, 174), NPC.GetAlpha(Color.White), NPC.rotation, new Vector2(116, 174) * 0.5f, NPC.scale, SpriteEffects.None);
            if (CurrentAttack == ClamitasAttacks.StartingCutscene)
            {
                Texture2D t = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
                float rotation = 0;
                float scale = 0;

                Main.EntitySpriteDraw(t, NPC.Center, null, Color.OrangeRed, rotation, t.Size()/2, new Vector2(1, scale), SpriteEffects.None, 0);

                float a = 
            }

            Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            string t = Texture;
            Texture2D value = ModContent.Request<Texture2D>(t).Value;
            Texture2D glow = ModContent.Request<Texture2D>(t + "Glow").Value;
            SpriteEffects effects = SpriteEffects.None;
            Vector2 vector = new Vector2(NPC.Center.X, NPC.Center.Y);
            Vector2 vector2 = new Vector2(value.Width / 2, value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 position = vector - screenPos;
            position -= new Vector2(glow.Width, glow.Height / Main.npcFrameCount[NPC.type]) * 1f / 2f;
            position += vector2 * 1f + new Vector2(0f, 4f + NPC.gfxOffY);
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Color.Red);
            Main.EntitySpriteDraw(glow, position, NPC.frame, color, NPC.rotation, vector2, NPC.scale, effects);
        }

        public override void OnKill()
        {
            /*if (NPC.FindFirstNPC(ModContent.NPCType<SEAHOE>()) == -1 && Main.netMode != 1)
            {
                NPC.NewNPC(base.NPC.GetSource_Death(), (int)base.NPC.Center.X, (int)base.NPC.Center.Y, ModContent.NPCType<SEAHOE>());
            }*/

            //DownedBossSystem.downedCLAM = true;
            //DownedBossSystem.downedCLAMHardMode = Main.hardMode || DownedBossSystem.downedCLAMHardMode;

            ClamitySystem.downedClamitas = true;
            CalamityNetcode.SyncWorld();
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //LeadingConditionRule mainRule = npcLoot.DefineConditionalDropSet(DropHelper.Hardmode());
            npcLoot.Add(ModContent.ItemType<BrimstoneSlag>(), 1, 30, 40);
            npcLoot.Add(ModContent.ItemType<HuskOfCalamity>(), 1, 25, 30);
            npcLoot.Add(ModContent.ItemType<ClamitousPearl>(), 1, 2, 4);
            npcLoot.Add(ModContent.ItemType<SlagspitterPauldron>(), 2, 1, 4);
            npcLoot.Add(ModContent.ItemType<TheSubcommunity>(), 10);
            //npcLoot.Add(ModContent.ItemType<Calamitea>(), 1, 3, 3);
            npcLoot.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, ModContent.ItemType<Brimlash>(), ModContent.ItemType<BrimstoneFury>(), ModContent.ItemType<BurningSea>(), ModContent.ItemType<IgneousExaltation>(), ModContent.ItemType<Brimblade>()));
            npcLoot.AddConditionalPerPlayer(() => !ClamitySystem.downedClamitas, ModContent.ItemType<LoreWhat>(), ui: true, DropHelper.FirstKillText);
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<ClamitasRelic>());
            npcLoot.Add(ModContent.ItemType<ThankYouPainting>(), 100);
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.BlackPearl, 1, 1, 9999, hideLootReport: true);
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.WhitePearl, 1, 1, 9999, hideLootReport: true);
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.PinkPearl, 1, 1, 9999, hideLootReport: true);
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.GalaxyPearl, 1, 1, 9999, hideLootReport: true);

            /*int[] itemIDs = new int[4]
            {
                ModContent.ItemType<ClamCrusher>(),
                ModContent.ItemType<ClamorRifle>(),
                ModContent.ItemType<Poseidon>(),
                ModContent.ItemType<ShellfishStaff>()
            };
            mainRule.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, itemIDs));
            npcLoot.Add(4412, 2);
            npcLoot.Add(4413, 4);
            npcLoot.Add(4414, 10);
            npcLoot.Add(ModContent.ItemType<GiantPearl>(), 3);
            npcLoot.Add(ModContent.ItemType<AmidiasPendant>(), 3);
            npcLoot.Add(ModContent.ItemType<GiantClamTrophy>(), 10);
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<GiantClamRelic>());
            */
        }
        public override bool CheckDead()
        {
            if (BossRushEvent.BossRushActive)
                return true;

            NPC.life = 1;
            NPC.active = true;
            NPC.dontTakeDamage = true;
            NPC.netUpdate = true;

            return false;
        }
        public override bool CheckActive()
        {
            return false;
        }
        /*public override bool CheckActive()
        {
            return Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 5600f;
        }*/
    }
}