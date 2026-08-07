using CalamityMod;
using Clamity.Content.Biomes.FrozenHell.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.NPCs
{
    public class ClockworkBat : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
        }
        public override void SetDefaults()
        {
            //NPC.CloneDefaults(NPCID.CaveBat);
            //AIType = NPCID.CaveBat;
            NPC.width = 22;
            NPC.height = 32;
            NPC.aiStyle = NPCAIStyleID.Bat;
            AIType = NPCID.CaveBat;
            AnimationType = NPCID.CaveBat;
            NPC.value = 35000;
            NPC.npcSlots = 0.5f;

            NPC.knockBackResist = 0.25f;
            NPC.DR_NERD(0.1f);
            NPC.defense = 12;
            NPC.lifeMax = 1100;
            NPC.damage = 70;
            //NPC.Calamity().VulnerableToHeat = false;
            //NPC.Calamity().VulnerableToSickness = true;

            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            SpawnModBiomes = new int[1]
              {
                ((ModSceneEffect) ModContent.GetInstance<Content.Biomes.FrozenHell.Biome.FrozenHell>()).Type
              };

        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
        {
            //BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
            new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.ClockworkBat.Bestiary")
        });
        public override void PostAI()
        {
            NPC.TargetClosest();
            NPC.Calamity().newAI[0]++;
            if (NPC.Calamity().newAI[0] >= 60)
            {
                NPC.Calamity().newAI[0] = 0;
                Vector2 velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10f;
                int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ProjectileID.BallofFire, NPC.damage, 0f, Main.myPlayer);
                Main.projectile[index].friendly = false;
                Main.projectile[index].hostile = true;
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cog, 1, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MetalFeather>(), 100));
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo) => !spawnInfo.Player.Clamity().ZoneFrozenHell ? 0.0f : 0.25f;
    }
}
