using CalamityMod;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using Clamity.Content.Biomes.FrozenHell.Items;
using Clamity.Content.Items;
using Clamity.Content.Items.Accessories;
using Clamity.Content.Items.Accessories.GemCrawlerDrop;
using Clamity.Content.Items.Accessories.Sentry;
using Clamity.Content.Items.Potions.Food;
using Clamity.Content.Items.SolynBooks;
using Clamity.Content.Items.Weapons.Melee.Shortswords;
using Clamity.Content.Items.Weapons.Typeless;
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
        //public int wCleave;
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            Conditions.IsHardmode hm = new Conditions.IsHardmode();
            LeadingConditionRule mainRule = npcLoot.DefineNormalOnlyDropSet();

            //Boss Drop
            if (npc.type == ModContent.NPCType<PlaguebringerGoliath>())
            {
                mainRule.Add(ItemDropRule.Common(ModContent.ItemType<Disease>(), 4));
                //mainRule.Add(ItemDropRule.Common(ModContent.ItemType<PlagueStation>()));
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
                npcLoot.Add(ItemDropRule.ByCondition(DropHelper.If(info => info.npc.type == ModContent.NPCType<GiantClam>() && ClamitySystem.downedClamitas, false), SolynBookRegistry.GetBookItem(SolynBooks.HowToClamity)));
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
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Merchant)
                shop.Add(SolynBookRegistry.GetBookItem(SolynBooks.BaseBook1));
            if (shop.NpcType == NPCID.Clothier)
                shop.Add(SolynBookRegistry.GetBookItem(SolynBooks.BaseBook2));
            if (shop.NpcType == NPCID.Wizard)
                shop.Add(SolynBookRegistry.GetBookItem(SolynBooks.BaseBook4), Condition.InHallow);
            if (shop.NpcType == NPCID.Steampunker)
                shop.Add<CyanSolution>(new Condition(Language.GetOrRegister("Mods.Clamity.Misc.DefeatedWoB"), () => ClamitySystem.downedWallOfBronze));
            if (shop.NpcType == ModContent.NPCType<Archmage>())
            {
                shop.Add(SolynBookRegistry.GetBookItem(SolynBooks.BaseBook3));
                shop.Add<EnchantedMetal>(new Condition(Language.GetOrRegister("Mods.Clamity.Misc.GeneratedFrozenHell"), () => !ClamitySystem.generatedFrozenHell || ClamityConfig.Instance.PermafrostSoldEnchantedMetal), new Condition(Language.GetOrRegister("Mods.Clamity.Misc.DefeatedWoB"), () => ClamitySystem.downedWallOfBronze));
            }

        }
    }
}
