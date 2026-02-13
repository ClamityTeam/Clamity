using CalamityMod;
using Clamity.Content.Biomes.EntropicSpace.Biomes;
using Clamity.Content.Biomes.EntropicSpace.Tiles;
using Clamity.Content.Biomes.EntropicSpace.Tiles.Banners;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.EntropicSpace.NPCs
{
    public class CosmiliteSlime : ModNPC
    {
        public enum State : int
        {
            Idle,
            //PrepareToJump,
            Jump,
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
        }
        public override void SetDefaults()
        {
            NPC.width = 62;
            NPC.height = 50;
            NPC.defense = 30;
            NPC.lifeMax = 1200;
            //NPC.GravityMultiplier *= 2f;
            //NPC.noGravity = true;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = NPCAIStyleID.Slime;
            AIType = NPCID.ToxicSludge;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<CosmiliteSlimeBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
            SpawnModBiomes = new int[] { ModContent.GetInstance<EntropicSpaceBiome>().Type, ModContent.GetInstance<ShatteredIslandsBiome>().Type };

            // Scale stats in Expert and Master
            //CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            //CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }
        public State CurrentState
        {
            get => (State)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public int CurrentFrame
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = (float)value;
        }
        /*public int CurrentFrame
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }*/
        public override void AI()
        {
            //NPC.TargetClosest(false);
            Player p = Main.player[NPC.target];

            //Main.NewText(CurrentFrame);

            //NPC.velocity.Y *= 0.8f;
            /*NPC.velocity.Y += 0.5f;
            if (NPC.velocity.Y > 0)
                NPC.velocity.Y *= 1.05f;

            if (CurrentState == State.Idle)
            {
                //NPC.noTileCollide = false;

                if (p.Center.Y > NPC.Center.Y) NPC.noTileCollide = true;//ignore tiles
                else NPC.noTileCollide = false; //don t ignore it

                NPC.velocity.X *= 0.95f;
                if (NPC.velocity.Length() < 0.1f && NPC.frameCounter == frameDelay - 1 && Main.rand.NextBool(2))
                {
                    NPC.velocity = new Vector2(p.Center.X > NPC.Center.X ? -10 : 10, -20);
                    CurrentState = State.Jump;
                }
            }
            else if (CurrentState == State.Jump)
            {
                if (p.Center.Y > NPC.Center.Y) NPC.noTileCollide = true; //ignore tiles
                else NPC.noTileCollide = false; //don t ignore it

                if (NPC.velocity.Y < 0.1f || NPC.velocity.Y > -0.1f) CurrentState = State.Idle;

                //NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -10, 10);
                if (p.Center.X > NPC.Center.X) //right side. Positive velocity
                    NPC.velocity.X = NPC.velocity.X < 10 ? NPC.velocity.X + 0.1f : NPC.velocity.X;
                else
                    NPC.velocity.X = NPC.velocity.X > -10 ? NPC.velocity.X - 0.1f : NPC.velocity.X;
            }

            if (NPC.Center.Y - p.Center.X > 16 * 25)
            {
                NPC.velocity = new Vector2(p.Center.X > NPC.Center.X ? -10 : 10, -20);
                CurrentState = State.Jump;
            }*/


            if (p.Center.Y > NPC.Center.Y) NPC.noTileCollide = true; //ignore tiles
            else NPC.noTileCollide = false; //don t ignore it

            if (NPC.Center.Y - p.Center.X > 16 * 25)
            {
                NPC.velocity = new Vector2(p.Center.X > NPC.Center.X ? -10 : 10, -20);
            }
        }
        public override void PostAI()
        {
            NPC.ai[0]++;
            if (NPC.ai[0] > -10)
            {
                NPC.velocity.X *= 1.2f;
                NPC.velocity.Y *= 1.3f;
            }
        }
        public const int frameDelay = 9;
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            CurrentFrame = NPC.frame.Y / frameHeight;


            if (NPC.velocity.Length() < 0.1f) //Standing
            {
                if (NPC.frameCounter > frameDelay)
                {
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                    else
                    {
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 4)
                        {
                            NPC.frame.Y = frameHeight * 0;
                        }
                    }
                    NPC.frameCounter = 0;
                }
            }
            else //Moving in jump state/Falling in idle state (if you break tiles under slime)
            {
                if (NPC.frameCounter > frameDelay)
                {
                    //NPC.frame.Y += frameHeight;
                    switch (CurrentFrame)
                    {
                        case 0: NPC.frame.Y = frameHeight * (NPC.velocity.Y > 0f || NPC.velocity.Y < -1f ? 4 : 0); break;
                        case 1: NPC.frame.Y = frameHeight * 0; break;
                        case 2: NPC.frame.Y = frameHeight * 3; break;
                        case 3: NPC.frame.Y = frameHeight * 4; break;
                        case 4: NPC.frame.Y = frameHeight * (NPC.velocity.Y > 0f || NPC.velocity.Y < -1f ? 4 : 1); break;
                    }
                    NPC.frameCounter = 0;
                }
            }
            if (NPC.frame.Y > frameHeight * 4)
            {
                NPC.frame.Y = 0;
            }


        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (/*spawnInfo.PlayerSafe || */!spawnInfo.Player.Clamity().ZoneShatteredIslands)
            {
                return 0f;
            }
            return 1f;
            //return SpawnCondition.Cavern.Chance;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //TODO - Comsilite Ore
            npcLoot.AddIf(DropHelper.If(() => DownedBossSystem.downedDoG, true, Language.GetTextValue("Mods.CalamityMod.Condition.Drops.DownedDoG")), ModContent.ItemType<CosmiliteOre>(), 1, 10, 26);
            //npcLoot.Add(ItemDropRule.ByCondition(CalamityConditions.DownedDevourerOfGods, ModContent.ItemType<CosmiliteOre>(), 1, 10, 26));
            npcLoot.Add(ItemID.SlimeStaff, 100);
        }
    }
}
