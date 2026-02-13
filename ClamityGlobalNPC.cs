using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using Clamity.Content.Biomes.FrozenHell.Items;
using Clamity.Content.Items;
using Clamity.Content.Items.Accessories;
using Clamity.Content.Items.Accessories.GemCrawlerDrop;
using Clamity.Content.Items.Accessories.Sentry;
using Clamity.Content.Items.Mounts;
using Clamity.Content.Items.Potions.Food;
using Clamity.Content.Items.Weapons.Melee.Shortswords;
using Clamity.Content.Items.Weapons.Typeless;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity
{
    public class ClamityGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool IncreasedHeatEffects_PyroStone;
        //public int wCleave;
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            Conditions.IsHardmode hm = new Conditions.IsHardmode();
            LeadingConditionRule mainRule = npcLoot.DefineNormalOnlyDropSet();

            //Boss Drop
            if (npc.type == ModContent.NPCType<PlaguebringerGoliath>())
            {
                mainRule.Add(ItemDropRule.Common(ModContent.ItemType<Disease>(), 4));
                mainRule.Add(ItemDropRule.Common(ModContent.ItemType<PlagueStation>()));
                npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<TrashOfMagnus>(), 4, 3));
            }
            if (npc.type == ModContent.NPCType<SupremeCalamitas>())
            {
                mainRule.Add(ItemDropRule.Common(ModContent.ItemType<Calamitea>(), 1, 10, 10));
                npcLoot.Add(ItemDropRule.ByCondition(DropHelper.If(info => info.npc.type == ModContent.NPCType<SupremeCalamitas>() && info.npc.ModNPC<SupremeCalamitas>().permafrost, false), ModContent.ItemType<WitherOnAStick>()));
            }

            //Other Drop
            if (npc.type == NPCID.SeaSnail)
            {
                npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<SeaShell>(), 2, 1));
            }
            if (npc.type == ModContent.NPCType<CalamityEye>())
            {
                var hardmode = npcLoot.DefineConditionalDropSet(DropHelper.Hardmode());
                hardmode.Add(ModContent.ItemType<BlightedSpyglass>(), 6);
            }
            if (npc.type == ModContent.NPCType<Clam>())
            {
                npcLoot.Add(ModContent.ItemType<CyanPearl>(), 6);
            }
            if (npc.type == ModContent.NPCType<CrawlerDiamond>())
            {
                npcLoot.Add(ModContent.ItemType<MagicDiamond>(), 6);
            }
            if (npc.type == ModContent.NPCType<CrawlerAmethyst>())
            {
                npcLoot.Add(ModContent.ItemType<SharpAmethyst>(), 6);
            }

            //Essence of Flame drop
            /*if (ContainType(npc.type, NPCID.Mummy, NPCID.LightMummy, NPCID.DarkMummy, NPCID.BloodMummy,
                NPCID.DesertBeast, NPCID.DesertScorpionWalk, NPCID.DesertScorpionWall,
                NPCID.DesertDjinn, NPCID.DesertLamiaDark, NPCID.DesertLamiaLight,
                NPCID.DesertGhoul, NPCID.DesertGhoulCorruption, NPCID.DesertGhoulCrimson, NPCID.DesertGhoulHallow,
                NPCID.DuneSplicerHead)
            )
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EssenceOfFlame>(), 4));
            }
            if (ContainType(npc.type, NPCID.Vulture, NPCID.TombCrawlerHead))
            {
                npcLoot.Add(ItemDropRule.ByCondition(hm, ModContent.ItemType<EssenceOfFlame>(), 4));
            }
            if (ContainType(npc.type, NPCID.Antlion, NPCID.WalkingAntlion, NPCID.GiantWalkingAntlion, NPCID.FlyingAntlion,
                NPCID.GiantFlyingAntlion)
            )
            {
                //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MandibleClaws>(), 50));
                npcLoot.Add(ItemDropRule.ByCondition(hm, ModContent.ItemType<EssenceOfFlame>(), 4));
            }*/


            //Food drop
            if (ContainType(npc.type, ModContent.NPCType<SeaSerpent1>(), ModContent.NPCType<EutrophicRay>(), ModContent.NPCType<GhostBell>(), ModContent.NPCType<PrismBack>(), ModContent.NPCType<SeaFloaty>(), ModContent.NPCType<BlindedAngler>()))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ClamChowder>(), 20));
            }
            if (ContainType(npc.type, ModContent.NPCType<Clam>()))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ClamChowder>(), 10));
            }
            if (ContainType(npc.type, ModContent.NPCType<GiantClam>()))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ClamChowder>(), 2));
            }
            if (ContainType(npc.type, ModContent.NPCType<ChaoticPuffer>(), ModContent.NPCType<GiantSquid>(), ModContent.NPCType<Laserfish>(), ModContent.NPCType<OarfishHead>(), ModContent.NPCType<Eidolist>(), ModContent.NPCType<MirageJelly>(), ModContent.NPCType<Bloatfish>()))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Barolegs>(), 20));
            }
            if (ContainType(npc.type, ModContent.NPCType<EidolonWyrmHead>(), ModContent.NPCType<GulperEelHead>(), ModContent.NPCType<ColossalSquid>(), ModContent.NPCType<ReaperShark>(), ModContent.NPCType<BobbitWormHead>()))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Barolegs>(), 4));
            }
        }
        private bool ContainType(int npcid, params int[] array)
        {
            bool num = false;
            foreach (int i in array)
            {
                if (i == npcid)
                {
                    num = true;
                    break;
                }
            }
            return num;
        }
        /*public override void PostAI(NPC npc)
        {
            if (wCleave > 0)
                --wCleave;
            int num = npc.defense - wCleave > 0 ? WarCleave.DefenseReduction : 0;
            if (num < 0)
                num = 0;
        }*/
        public void ApplyDPSDebuff(int lifeRegenValue, int damageValue, ref int lifeRegen, ref int damage)
        {
            if (lifeRegen > 0)
                lifeRegen = 0;

            lifeRegen -= lifeRegenValue;

            if (damage < damageValue)
                damage = damageValue;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            int[] wormBossTypes =
            [
                ModContent.NPCType<DesertScourgeHead>(),
                ModContent.NPCType<DesertScourgeBody>(),
                ModContent.NPCType<DesertScourgeTail>(),
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail,
                ModContent.NPCType<PerforatorHeadSmall>(),
                ModContent.NPCType<PerforatorHeadMedium>(),
                ModContent.NPCType<PerforatorHeadLarge>(),
                ModContent.NPCType<PerforatorBodySmall>(),
                ModContent.NPCType<PerforatorBodyMedium>(),
                ModContent.NPCType<PerforatorBodyLarge>(),
                ModContent.NPCType<PerforatorTailSmall>(),
                ModContent.NPCType<PerforatorTailMedium>(),
                ModContent.NPCType<PerforatorTailLarge>(),
                ModContent.NPCType<AquaticScourgeHead>(),
                ModContent.NPCType<AquaticScourgeBody>(),
                ModContent.NPCType<AquaticScourgeTail>(),
                ModContent.NPCType<AstrumDeusHead>(),
                ModContent.NPCType<AstrumDeusBody>(),
                ModContent.NPCType<AstrumDeusTail>(),
                ModContent.NPCType<StormWeaverHead>(),
                ModContent.NPCType<StormWeaverBody>(),
                ModContent.NPCType<StormWeaverTail>()
            ];

            int[] slimeGodTypes =
            [
                ModContent.NPCType<SlimeGodCore>(),
                ModContent.NPCType<CrimulanPaladin>(),
                ModContent.NPCType<EbonianPaladin>()
            ];

            bool wormBoss = wormBossTypes.Contains(npc.type);
            bool slimeGod = slimeGodTypes.Contains(npc.type);
            bool slimed = npc.drippingSlime || npc.drippingSparkleSlime;

            //Vulnerability
            double heatDamageMult = slimed ? ((wormBoss || slimeGod) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
            if (npc.Calamity().VulnerableToHeat.HasValue)
            {
                if (npc.Calamity().VulnerableToHeat.Value)
                    heatDamageMult *= slimed ? ((wormBoss || slimeGod) ? 1.25 : 1.5) : ((wormBoss || slimeGod) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
                else
                    heatDamageMult *= slimed ? ((wormBoss || slimeGod) ? 0.66 : 0.5) : 0.5;
            }

            double coldDamageMult = CalamityGlobalNPC.BaseDoTDamageMult;
            if (npc.Calamity().VulnerableToCold.HasValue)
            {
                if (npc.Calamity().VulnerableToCold.Value)
                    coldDamageMult *= wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult;
                else
                    coldDamageMult *= 0.5;
            }

            double sicknessDamageMult = npc.Calamity().irradiated ? (wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
            if (npc.Calamity().VulnerableToSickness.HasValue)
            {
                if (npc.Calamity().VulnerableToSickness.Value)
                    sicknessDamageMult *= npc.Calamity().irradiated ? (wormBoss ? 1.25 : 1.5) : (wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
                else
                    sicknessDamageMult *= npc.Calamity().irradiated ? (wormBoss ? 0.66 : 0.5) : 0.5;
            }

            bool increasedElectricityDamage = npc.wet || npc.honeyWet || npc.lavaWet || npc.dripping;
            double electricityDamageMult = increasedElectricityDamage ? (wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
            if (npc.Calamity().VulnerableToElectricity.HasValue)
            {
                if (npc.Calamity().VulnerableToElectricity.Value)
                    electricityDamageMult *= increasedElectricityDamage ? (wormBoss ? 1.25 : 1.5) : (wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
                else
                    electricityDamageMult *= increasedElectricityDamage ? (wormBoss ? 0.66 : 0.5) : 0.5;
            }

            double waterDamageMult = CalamityGlobalNPC.BaseDoTDamageMult;
            if (npc.Calamity().VulnerableToWater.HasValue)
            {
                if (npc.Calamity().VulnerableToWater.Value)
                    waterDamageMult *= wormBoss ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult;
                else
                    waterDamageMult *= 0.5;
            }

            //Calamity Debuff buffs
            if (npc.Calamity().IncreasedColdEffects_EskimoSet)
                coldDamageMult += 0.25;
            if (npc.Calamity().IncreasedColdEffects_CryoStone)
                coldDamageMult += 0.5;
            if (npc.Calamity().IncreasedHeatEffects_Fireball)
                heatDamageMult += 0.25;
            /*
            if (npc.Calamity().IncreasedHeatEffects_FlameWakerBoots)
                heatDamageMult += 0.25;
            */
            if (npc.Calamity().IncreasedHeatEffects_CinnamonRoll)
                heatDamageMult += 0.5;
            /*
            if (npc.Calamity().IncreasedHeatEffects_HellfireTreads)
                heatDamageMult += 0.5;
            if (npc.Calamity().IncreasedElectricityEffects_Transformer)
                electricityDamageMult += 0.5;
            */
            if (npc.Calamity().IncreasedSicknessEffects_ToxicHeart)
                sicknessDamageMult += 0.5;
            if (npc.Calamity().IncreasedSicknessAndWaterEffects_EvergreenGin)
            {
                sicknessDamageMult += 0.25;
                waterDamageMult += 0.25;
            }

            //Clamity Debuff buffs
            float vanillaColdDamageMult = 0f;
            float vanillaHeatDamageMult = 0f;
            if (IncreasedHeatEffects_PyroStone)
                vanillaHeatDamageMult += 0.5f;
            float vanillaSicknessDamageMult = 0f;
            float vanillaWaterDamageMult = 0f;
            float vanillaElectricityDamageMult = 0f;


            //"vanilla" debuffs

            // OnFire
            if (npc.onFire)
            {
                int num15 = (int)(12.0 * vanillaHeatDamageMult);
                npc.lifeRegen -= num15;
                if (damage < num15 / 4)
                    damage = num15 / 4;
            }

            // Cursed Inferno
            if (npc.onFire2)
            {
                int num16 = (int)(48.0 * vanillaHeatDamageMult);
                npc.lifeRegen -= num16;
                if (damage < num16 / 4)
                    damage = num16 / 4;
            }

            // Hellfire
            if (npc.onFire3)
            {
                int num17 = (int)(30.0 * vanillaHeatDamageMult);
                npc.lifeRegen -= num17;
                if (damage < num17 / 4)
                    damage = num17 / 4;
            }

            // Daybroken
            if (npc.daybreak)
            {
                int num18 = 0;
                for (int index = 0; index < Main.maxProjectiles; ++index)
                {
                    if (((Entity)Main.projectile[index]).active && Main.projectile[index].type == 636 && (double)Main.projectile[index].ai[0] == 1.0 && (double)Main.projectile[index].ai[1] == (double)((Entity)npc).whoAmI)
                        ++num18;
                }
                int num19 = (int)((num18 <= 1 ? 1.0 : 1.0 + 0.25 * (double)(num18 - 1)) * 2.0 * 100.0 * vanillaHeatDamageMult);
                npc.lifeRegen -= num19;
                if (damage < num19 / 4)
                    damage = num19 / 4;
            }

            // Shadowflame
            if (npc.shadowFlame)
            {
                int num20 = (int)(60.0 * vanillaHeatDamageMult);
                npc.lifeRegen -= num20;
                if (damage < num20 / 4)
                    damage = num20 / 4;
            }

            // Brimstone Flames
            if (npc.Calamity().brimstoneFlames)
            {
                int baseBrimstoneFlamesDoTValue = (int)(60 * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseBrimstoneFlamesDoTValue, baseBrimstoneFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Holy Flames
            if (npc.Calamity().holyFlames)
            {
                int baseHolyFlamesDoTValue = (int)(200 * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseHolyFlamesDoTValue, baseHolyFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // God Slayer Inferno
            if (npc.Calamity().godSlayerInferno)
            {
                int baseGodSlayerInfernoDoTValue = (int)(250 * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseGodSlayerInfernoDoTValue, baseGodSlayerInfernoDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Dragonfire
            if (npc.Calamity().dragonFire)
            {
                int baseDragonFireDoTValue = (int)(960 * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseDragonFireDoTValue, baseDragonFireDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Banishing Fire
            if (npc.Calamity().banishingFire)
            {
                int baseBanishingFireDoTValue = (int)((npc.lifeMax >= 1000000 ? npc.lifeMax / 500 : 4000) * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseBanishingFireDoTValue, baseBanishingFireDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            /* IDK ngl
            // Vulnerability Hex
            if (npc.Calamity().vulnerabilityHex)
            {
                int baseVulnerabilityHexDoTValue = (int)(VulnerabilityHex.DPS * vanillaHeatDamageMult);
                ApplyDPSDebuff(baseVulnerabilityHexDoTValue, VulnerabilityHex.TickNumber, ref npc.lifeRegen, ref damage);
            }
            */

            // Frostburn
            if (npc.onFrostBurn)
            {
                int baseFrostBurnDoTValue = (int)(16 * vanillaColdDamageMult);
                npc.lifeRegen -= baseFrostBurnDoTValue;
                if (damage < baseFrostBurnDoTValue / 4)
                    damage = baseFrostBurnDoTValue / 4;
            }

            // Frostbite
            if (npc.onFrostBurn2)
            {
                int baseFrostBiteDoTValue = (int)(50 * vanillaColdDamageMult);
                npc.lifeRegen -= baseFrostBiteDoTValue;
                if (damage < baseFrostBiteDoTValue / 4)
                    damage = baseFrostBiteDoTValue / 4;
            }


            bool hasColdOil = npc.onFrostBurn || npc.onFrostBurn2;
            bool hasHotOil = npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame;
            bool hasModHotOil = npc.Calamity().brimstoneFlames || npc.Calamity().holyFlames || npc.Calamity().godSlayerInferno || npc.Calamity().dragonFire || npc.Calamity().banishingFire || npc.Calamity().vulnerabilityHex;
            if (npc.oiled && hasColdOil | hasHotOil | hasModHotOil)
            {
                double multiplier = 1.0;
                if (hasColdOil)
                    multiplier *= vanillaColdDamageMult;
                if (hasHotOil || hasModHotOil & hasColdOil)
                    multiplier *= vanillaHeatDamageMult;
                //if (hasModHotOil && !hasColdOil && !hasHotOil)
                //    multiplier *= heatDamageMult;

                int num24 = (int)(50.0 * multiplier);
                npc.lifeRegen -= num24;
                if (damage < num24 / 4)
                    damage = num24 / 4;
            }

            // Nightwither
            if (npc.Calamity().nightwither)
            {
                int baseNightwitherDoTValue = (int)(200 * vanillaColdDamageMult);
                ApplyDPSDebuff(baseNightwitherDoTValue, baseNightwitherDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Plague
            if (npc.Calamity().plague)
            {
                int basePlagueDoTValue = (int)(100 * vanillaSicknessDamageMult);
                ApplyDPSDebuff(basePlagueDoTValue, basePlagueDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Astral Infection
            if (npc.Calamity().astralInfection)
            {
                int baseAstralInfectionDoTValue = (int)(75 * vanillaSicknessDamageMult);
                ApplyDPSDebuff(baseAstralInfectionDoTValue, baseAstralInfectionDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Sulphuric Poisoning
            if (npc.Calamity().sulphurPoison)
            {
                int baseSulphurPoisonDoTValue = (int)(240 * vanillaSicknessDamageMult);
                ApplyDPSDebuff(baseSulphurPoisonDoTValue, baseSulphurPoisonDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Sage Poison
            if (npc.Calamity().sagePoison)
            {
                // npc.Calamity().sagePoisonDamage = 50 * (float)(Math.Pow(totalSageSpirits, 0.73D) + Math.Pow(totalSageSpirits, 1.1D)) * 0.5f
                // See SageNeedle.cs for details
                int baseSagePoisonDoTValue = (int)(npc.Calamity().sagePoisonDamage * vanillaSicknessDamageMult);
                ApplyDPSDebuff(baseSagePoisonDoTValue, baseSagePoisonDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Kami Debuff from Yanmei's Knife
            /*if (npc.Calamity().kamiFlu > 0)
            {
                int baseKamiFluDoTValue = (int)(250 * vanillaSicknessDamageMult);
                ApplyDPSDebuff(baseKamiFluDoTValue, baseKamiFluDoTValue / 10, ref npc.lifeRegen, ref damage);
            }*/

            //Absorber Affliction
            if (npc.Calamity().absorberAffliction)
            {
                int baseAbsorberDoTValue = (int)(400 * vanillaSicknessDamageMult);
                ApplyDPSDebuff(baseAbsorberDoTValue, baseAbsorberDoTValue / 65, ref npc.lifeRegen, ref damage);
            }

            // Poisoned
            if (npc.poisoned)
            {
                int basePoisonedDoTValue = (int)(12 * vanillaSicknessDamageMult);
                npc.lifeRegen -= basePoisonedDoTValue;
                if (damage < basePoisonedDoTValue / 4)
                    damage = basePoisonedDoTValue / 4;
            }

            // Venom
            if (npc.venom)
            {
                int baseVenomDoTValue = (int)(60 * vanillaSicknessDamageMult);
                npc.lifeRegen -= baseVenomDoTValue;
                if (damage < baseVenomDoTValue / 4)
                    damage = baseVenomDoTValue / 4;
            }

            // Electrified
            if (npc.Calamity().electrified)
            {
                int baseElectrifiedDoTValue = (int)(5 * (npc.velocity.X == 0 ? 1 : 4) * vanillaElectricityDamageMult);
                ApplyDPSDebuff(baseElectrifiedDoTValue, baseElectrifiedDoTValue / 5, ref npc.lifeRegen, ref damage);
            }

            // Crush Depth
            if (npc.Calamity().crushDepth)
            {
                int baseCrushDepthDoTValue = (int)(100 * vanillaWaterDamageMult);
                ApplyDPSDebuff(baseCrushDepthDoTValue, baseCrushDepthDoTValue / 2, ref npc.lifeRegen, ref damage);
            }

            //Riptide
            if (npc.Calamity().riptide)
            {
                int baseRiptideDoTValue = (int)(30 * vanillaWaterDamageMult);
                ApplyDPSDebuff(baseRiptideDoTValue, baseRiptideDoTValue / 3, ref npc.lifeRegen, ref damage);
            }
        }
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Steampunker)
                shop.Add<CyanSolution>(new Condition(Language.GetOrRegister("Mods.Clamity.Misc.DefeatedWoB"), () => ClamitySystem.downedWallOfBronze));
            if (shop.NpcType == ModContent.NPCType<Archmage>())
                shop.Add<EnchantedMetal>(new Condition(Language.GetOrRegister("Mods.Clamity.Misc.GeneratedFrozenHell"), () => !ClamitySystem.generatedFrozenHell || ClamityConfig.Instance.PermafrostSoldEnchantedMetal), new Condition(Language.GetOrRegister("Mods.Clamity.Misc.DefeatedWoB"), () => ClamitySystem.downedWallOfBronze));
        }
    }
}
