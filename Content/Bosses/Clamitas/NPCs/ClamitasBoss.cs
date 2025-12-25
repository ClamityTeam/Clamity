using CalamityMod;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Furniture.DevPaintings;
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
    //[AutoloadBossHead]
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

        private int hitAmount;
        private bool hasBeenHit;
        private bool statChange;


        //Used for animations
        private bool attackAnim;
        private bool hide;
        private int flareFrame;
        private int flareFrameCounter;


        //Boss Icon
        public static int giantClamIconIndex;
        public static int clamitasIconIndex;

        internal static void LoadHeadIcons()
        {
            string phase1IconPath = "Clamity/Content/Bosses/Clamitas/NPCs/GiantClam_Head_Boss";
            string phase2IconPath = "Clamity/Content/Bosses/Clamitas/NPCs/ClamitasBoss_Head_Boss";

            Clamity.mod.AddBossHeadTexture(phase1IconPath, -1);
            giantClamIconIndex = ModContent.GetModBossHeadSlot(phase1IconPath);

            Clamity.mod.AddBossHeadTexture(phase2IconPath, -1);
            clamitasIconIndex = ModContent.GetModBossHeadSlot(phase2IconPath);
        }
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

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 5f;
            NPC.damage = 50;
            NPC.width = 160;
            NPC.height = 120;
            NPC.defense = 9999;
            NPC.DR_NERD(0.3f);
            //base.NPC.lifeMax = (Main.hardMode ? 7500 : 1250);
            //NPC.lifeMax = 50000;
            NPC.LifeMaxNERB(50000, 58000, 500000);
            NPC.aiStyle = -1;
            AIType = -1;
            //base.NPC.value = (Main.hardMode ? Item.buyPrice(0, 8) : Item.buyPrice(0, 1));
            NPC.value = Item.buyPrice(0, 20);
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0f;
            NPC.rarity = 2;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<BrimstoneCragsBiome>().Type };
            NPC.boss = true;

            if (!Main.dedServ)
            {
                //Music = 0;
                Music = Clamity.mod.GetMusicFromMusicMod("Clamitas") ?? MusicID.Boss3;
            }
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
            writer.Write(hitAmount);
            writer.Write(attack);
            writer.Write(attackAnim);
            writer.Write(NPC.dontTakeDamage);
            writer.Write(NPC.chaseable);
            writer.Write(hasBeenHit);
            writer.Write(statChange);
            writer.Write(hide);
            writer.Write(flareFrame);
            writer.Write(flareFrameCounter);
            for (int i = 0; i < 2; i++)
            {
                writer.Write(NPC.Calamity().newAI[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hitAmount = reader.ReadInt32();
            attack = reader.ReadInt32();
            attackAnim = reader.ReadBoolean();
            NPC.dontTakeDamage = reader.ReadBoolean();
            NPC.chaseable = reader.ReadBoolean();
            hasBeenHit = reader.ReadBoolean();
            statChange = reader.ReadBoolean();
            hide = reader.ReadBoolean();
            flareFrame = reader.ReadInt32();
            flareFrameCounter = reader.ReadInt32();
            for (int i = 0; i < 2; i++)
            {
                NPC.Calamity().newAI[i] = reader.ReadSingle();
            }
        }



        /*public override bool CheckActive()
        {
            return Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 5600f;
        }*/

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }

            return null;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > (attackAnim ? 2.0 : 5.0))
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = NPC.frame.Y + frameHeight;
            }

            if ((hitAmount < 10 || hide) && !NPC.IsABestiaryIconDummy)
            {
                NPC.frame.Y = frameHeight * 11;
            }
            else if (attackAnim)
            {
                if (NPC.frame.Y < frameHeight * 3)
                {
                    NPC.frame.Y = frameHeight * 3;
                }

                if (NPC.frame.Y > frameHeight * 10)
                {
                    hide = true;
                    attackAnim = false;
                }
            }
            else if (NPC.frame.Y > frameHeight * 3)
            {
                NPC.frame.Y = 0;
            }

            flareFrameCounter++;
            if (flareFrameCounter >= 5)
            {
                flareFrameCounter = 0;
                flareFrame++;
                if (flareFrame >= 6)
                    flareFrame = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneCalamity && DownedBossSystem.downedCalamitasClone && !NPC.AnyNPCs(ModContent.NPCType<ClamitasBoss>()))
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.24f;
            }

            return 0f;
        }

        public override void ModifyTypeName(ref string typeName)
        {
            /*if (Main.zenithWorld)
            {
                if (Main.hardMode)
                {
                    typeName = CalamityUtils.GetTextValue("NPCs.SupremeClamitas");
                }
                else
                {
                    typeName = CalamityUtils.GetTextValue("NPCs.Clamitas");
                }
            }*/

            if (BattleIsStarted || NPC.IsABestiaryIconDummy)
            {
                typeName = Language.GetTextValue($"Mods.Clamity.NPCs.{nameof(ClamitasBoss)}.DisplayName");
            }
            else
            {
                typeName = Language.GetTextValue($"Mods.Clamity.NPCs.{nameof(ClamitasBoss)}.DisplayNameAlt");
            }
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
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModLoader.GetMod("Clamity").Find<ModGore>("Clamitas1").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModLoader.GetMod("Clamity").Find<ModGore>("Clamitas2").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModLoader.GetMod("Clamity").Find<ModGore>("Clamitas3").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModLoader.GetMod("Clamity").Find<ModGore>("Clamitas4").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModLoader.GetMod("Clamity").Find<ModGore>("Clamitas5").Type);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            string t = Texture;
            if (!BattleIsStarted && !NPC.IsABestiaryIconDummy)
                t = ModContent.GetInstance<GiantClam>().Texture;
            else
                Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Extra").Value, NPC.Center - Vector2.UnitY * 20f * NPC.scale - screenPos, new Rectangle(0, flareFrame * 174, 116, 174), NPC.GetAlpha(Color.White), NPC.rotation, new Vector2(116, 174) * 0.5f, NPC.scale, SpriteEffects.None);

            Main.EntitySpriteDraw(ModContent.Request<Texture2D>(t).Value, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            return false;
        }

        public override void BossHeadSlot(ref int index)
        {
            if (!BattleIsStarted)
                index = giantClamIconIndex;
            else
                index = clamitasIconIndex;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            string t = Texture;
            if (!BattleIsStarted)
                t = ModContent.GetInstance<GiantClam>().Texture;

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
    }
}