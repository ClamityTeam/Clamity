using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Tools;
using CalamityMod.NPCs;
using CalamityMod.World;
using Clamity.Content.Biomes.FrozenHell.Items;
using Clamity.Content.Bosses.WoB.Drop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Clamity.Commons.CalRemixCompatibilitySystem;

namespace Clamity.Content.Bosses.WoB.NPCs
{
    [AutoloadBossHead]
    public class WallOfBronze : ModNPC
    {
        public enum Attacks
        {
            Idle = 0,

            Claw = 1,
            Mininguns = 2,
            Laser = 3,
            LaserXMiniguns = 4,
            ClawXMiniguns = 5,

            PlaseTransition = -1,

            IdlePhase2 = 10,
            BIGFUCKINLASER = 11,

            DespirationPhase = 100
        }
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
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            //NPCID.Sets.MPAllowedEnemies[this.Type] = true;
            Main.npcFrameCount[Type] = 2;

            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = Texture + "_Bestiary",
                //CustomTexturePath = "CalamityMod/Projectiles/InvisibleProj",
                Scale = 0.75f,
                Position = new Vector2(50, 10),
                PortraitScale = 0.5f,
                PortraitPositionXOverride = 0,
                PortraitPositionYOverride = 0
            });

            var fanny1 = new FannyDialog("WallOfBronze", "FannyNuhuh").WithDuration(4f).WithCondition(_ => { return Myself is not null; });
            var fanny2 = new FannyDialog("WallOfBronzeRoD", "FannyNuhuh").WithDuration(4f).WithCondition(_ => { return Myself is not null && (Main.LocalPlayer.HasItem(ItemID.RodofDiscord) || Main.LocalPlayer.HasItem(ModContent.ItemType<NormalityRelocator>())); }).WithParentDialog(fanny1, 4f);

            fanny1.Register();
            fanny2.Register();
        }
        public override void SetDefaults()
        {
            NPC.width = 72;
            NPC.height = 146;
            NPC.aiStyle = -1;
            NPC.damage = 200;
            NPC.defense = 70;
            //NPC.lifeMax = 1500000;
            NPC.LifeMaxNERB(1500300, 288090, 288090);
            //NPC.LifeMaxNERB(1000000, 1500000, 1700000);
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            NPC.knockBackResist = 0.0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.value = Item.sellPrice(1, 50, 25, 75);
            //NPC.npcSlots = 15f;
            NPC.npcSlots = 6f;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            //if (Main.getGoodWorld)
            //    NPC.scale = 1.5f;

            if (!Main.dedServ)
            {
                Music = Clamity.mod.GetMusicFromMusicMod("WallOfBronzeOld") ?? MusicID.Boss3;
            }
            NPC.netAlways = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange((IEnumerable<IBestiaryInfoElement>)new List<IBestiaryInfoElement>()
        {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
            new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.WallOfBronze.Bestiary")
        });
        public override void BossLoot(ref int potionType) => potionType = ModContent.ItemType<OmegaHealingPotion>();
        public override void OnSpawn(IEntitySource source)
        {
            ClamityUtils.BossIntroDialogue("WallOfBronze", NPC);

            //Myself = NPC;

            /*if (Main.netMode != NetmodeID.MultiplayerClient)
                for (int i = 0; i < 4; i++)
                    NPC.NewNPC(NPC.GetBossSpawnSource(Player.FindClosest(NPC.Center, 1, 1)), (int)NPC.Center.X, (int)NPC.Center.Y, ListOfGuns[(Main.rand.Next(0, ListOfGuns.Length))]);
            */

            /*if (Main.netMode == NetmodeID.MultiplayerClient || !(source is EntitySource_BossSpawn entitySourceBossSpawn) || !(entitySourceBossSpawn.Target is Player target))
                return;
            NPC.position.X = (float)((target.Center.X < 2400f ? 480 : Main.maxTilesX * 16 - 480) - NPC.width / 2);
            NPC.position.Y = target.Center.Y - NPC.height / 2f;
            if (Main.netMode != NetmodeID.Server)
                return;
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, (NetworkText)null, NPC.whoAmI, 0.0f, 0.0f, 0.0f, 0, 0, 0);*/
        }
        private readonly int[] ListOfGuns = new int[3]
        {
            ModContent.NPCType<WallOfBronzeTorret>(),
            ModContent.NPCType<WallOfBronzeLaser>(),
            ModContent.NPCType<WallOfBronzeClaw>()
        };
        //Consts with VS'c Hot Compile feature
        private static int AttackCountP1 => 5;
        private static int AttackCountP2 => 1;
        private static float HPPercentPerIdle => 0.05f;
        private static float P2HpPercent => 0.3f;
        private static int GunCraftingTime = 30;


        private ref float CurrentAttackInt => ref NPC.ai[0];
        private Attacks CurrentAttack 
        {
            set => NPC.ai[0] = (int)value;
            get => (Attacks)NPC.ai[0];
        }
        private ref float AttackTimer => ref NPC.ai[1];
        private ref float AttacksPassed => ref NPC.ai[2];
        public Player target => Main.player[NPC.target];
        public override void AI()
        {
            Myself = NPC;
            int num1 = 0;
            Mod mod1;
            if (ModLoader.TryGetMod("CalamityMod", out mod1))
            {
                if ((bool)mod1.Call(new object[2]
                {
                    "GetDifficultyActive",
                    "Death"
                }))
                    num1 = 2;
                else if ((bool)mod1.Call(new object[2]
                {
                    "GetDifficultyActive",
                    "Revengeance"
                }))
                    num1 = 1;
            }
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (!player.active || player.dead || Vector2.Distance(player.Center, NPC.Center) > 6400f)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead || Vector2.Distance(player.Center, NPC.Center) > 6400f)
                {
                    NPC.active = false;
                    return;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            //Base movement
            Vector2 center = target.Center;
            if (NPC.velocity.X == 0f)
            {
                NPC.velocity.X = Math.Sign(center.X - NPC.Center.X) * 2f;
            }
            else
            {
                NPC.spriteDirection = NPC.direction = Math.Sign(NPC.velocity.X);
                NPC.velocity.X = Math.Sign(NPC.velocity.X) * (Main.expertMode ? MathHelper.Lerp(7f + num1, 4f, (float)NPC.life / (float)NPC.lifeMax) : 2f);
                NPC.velocity.Y = Math.Sign(center.Y - NPC.Center.Y) * 2;
            }

            //Horrified debuff and You can t escape
            foreach (Player p in Main.player)
            {
                if (p == null) continue;
                if (!p.active) continue;
                if (p.Center.Y < Main.UnderworldLayer * 16)
                    p.AddBuff(BuffID.Horrified, 2);

                if (NPC.direction > 0)
                {
                    if (p.Center.X < NPC.Center.X)
                    {
                        p.velocity.X = 10;
                        p.position.X += 10;
                    }
                }
                else if (NPC.direction < 0)
                {
                    if (p.Center.X > NPC.Center.X)
                    {
                        p.velocity.X = -10;
                        p.position.X -= 10;
                    }
                }
            }

            //Shield
            //Will reworked
            int gunCount = 0;
            foreach (Terraria.NPC npc in Main.npc)
            {
                if (npc == null) continue;
                if (!npc.active) continue;
                if (ListOfGuns.Contains<int>(npc.type))
                    gunCount++;
            }
            if (gunCount == 0 || !PhaseTwo)
                NPC.dontTakeDamage = false;
            else
                NPC.dontTakeDamage = true;

            //Attacks
            switch ((Attacks)CurrentAttack)
            {
                case Attacks.Idle:
                    //Something passive attacks

                    if (NPC.life / (float)NPC.lifeMax < 1 - AttacksPassed * HPPercentPerIdle)
                    {
                        CurrentAttackInt = AttacksPassed < AttackCountP1 ? AttacksPassed+1 : Main.rand.Next(1, AttackCountP1+1);
                        CurrentAttack = Attacks.Claw;
                        AttackTimer = 0;
                    }
                    break;
                case Attacks.Claw:
                    if (AttackTimer == GunCraftingTime)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            for (int i = 0; i < 3; i++)
                                NPC.NewNPC(NPC.GetBossSpawnSource(Player.FindClosest(NPC.Center, 1, 1)), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<WallOfBronzeClaw>());
                    }
                    else if (AttackTimer > GunCraftingTime)
                    {
                        if (gunCount == 0)
                        {
                            CurrentAttack = Attacks.Idle;
                            AttackTimer = 0;
                            AttacksPassed++;
                        }
                    }
                    break;
            }
            AttackTimer++;
        }
        private bool PhaseTwo
        {
            get => NPC.life < (NPC.lifeMax * P2HpPercent);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D wall = ModContent.Request<Texture2D>(Texture + "_ExtraBack").Value;
            if (Main.getGoodWorld || Main.xMas)
                wall = ModContent.Request<Texture2D>(Texture + "_ExtraBack_GFB").Value;
            int num = Main.screenHeight / 32 + 1;
            int height = wall.Height;
            SpriteEffects spriteEffects = NPC.spriteDirection != 1 ? (SpriteEffects)1 : (SpriteEffects)0;

            float offset = 0;
            /*if (CurrentAttack is not (Attacks.Idle or Attacks.IdlePhase2 or Attacks.PlaseTransition))
                offset = CalamityUtils.SineInOutEasing(MathHelper.Clamp(AttackTimer, 0, GunCraftingTime) / GunCraftingTime, 1) * height;*/

            for (int index = -num; index <= num; ++index)
            {
                if (Main.UnderworldLayer < (int)(Main.LocalPlayer.Center.Y / 16f) + index)
                    spriteBatch.Draw(wall,
                                    new Vector2(
                                            NPC.Center.X - NPC.spriteDirection * wall.Width * 0.5f + Math.Sign(NPC.velocity.X) * 100, 
                                            (int)Main.LocalPlayer.Center.Y / height * height + index * wall.Height + offset
                                        )- screenPos,
                                    new Rectangle?(),
                                    Lighting.GetColor((int)(NPC.Center.X - NPC.spriteDirection * wall.Width * 0.5) / 16, (int)(Main.LocalPlayer.Center.Y / 16) + index),
                                    0.0f,
                                    Utils.Size(wall) * 0.5f,
                                    1f,
                                    spriteEffects,
                                    0.0f);
            }
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D wall = ModContent.Request<Texture2D>(Texture + "_Extra").Value;
            if (Main.getGoodWorld || Main.xMas)
                wall = ModContent.Request<Texture2D>(Texture + "_Extra_GFB").Value;
            int num = Main.screenHeight / 32 + 1;
            int height = wall.Height;
            SpriteEffects spriteEffects = NPC.spriteDirection != 1 ? (SpriteEffects)1 : (SpriteEffects)0;

            float offset = 0;
            /*if (CurrentAttack is not (Attacks.Idle or Attacks.IdlePhase2 or Attacks.PlaseTransition))
                offset = CalamityUtils.SineInOutEasing(MathHelper.Clamp(AttackTimer, 0, GunCraftingTime) / GunCraftingTime, 1) * height;*/

            for (int index = -num; index <= num; ++index)
            {
                if (Main.UnderworldLayer < (int)(Main.LocalPlayer.Center.Y / 16f) + index)
                    spriteBatch.Draw(wall,
                                    new Vector2(
                                            NPC.Center.X - NPC.spriteDirection * wall.Width * 0.5f - Math.Sign(NPC.velocity.X), 
                                            (int)Main.LocalPlayer.Center.Y / height * height + index * wall.Height - offset
                                        ) - screenPos,
                                    new Rectangle?(),
                                    Lighting.GetColor((int)(NPC.Center.X - NPC.spriteDirection * wall.Width * 0.5) / 16, (int)(Main.LocalPlayer.Center.Y / 16) + index),
                                    0.0f,
                                    Utils.Size(wall) * 0.5f,
                                    1f,
                                    spriteEffects,
                                    0.0f);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (PhaseTwo)
                NPC.frame = new Rectangle(0, 146, NPC.width, NPC.height);
            else
                NPC.frame = new Rectangle(0, 0, NPC.width, NPC.height);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<WoBTreasureBag>()));
            LeadingConditionRule mainRule = npcLoot.DefineNormalOnlyDropSet();
            int[] itemIDs = new int[]
            {
                ModContent.ItemType<AMS>(),
                ModContent.ItemType<TheWOBbler>(),
            };
            mainRule.Add(ItemDropRule.OneFromOptions(1, itemIDs));

            mainRule.Add(ItemDropRule.Common(ModContent.ItemType<LargeFather>()));
            mainRule.Add(ItemDropRule.Common(ModContent.ItemType<ThankYouPainting>(), 100));
            //Trophy
            npcLoot.Add(ModContent.ItemType<WoBTrophy>(), 10);
            //Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<WoBRelic>());
            //Mask
            mainRule.Add(ItemDropRule.Common(ModContent.ItemType<WoBMask>(), 7));
            //Lore
            npcLoot.AddConditionalPerPlayer(() => !ClamitySystem.downedWallOfBronze, ModContent.ItemType<LoreWallOfBronze>(), ui: true, DropHelper.FirstKillText);
            //GFB drop
            /*for (int i = 0; i < 20; i++)
            {
                npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.CopperBar, 1, 1, 10, true);
                npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.TinBar, 1, 1, 10, true);
            }*/
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.CopperPlating, 1, 1, 9999, hideLootReport: true);
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.TinPlating, 1, 1, 9999, hideLootReport: true);

        }
        public override void OnKill()
        {
            if (!ClamitySystem.downedWallOfBronze)
            {
                //int basePosX = (int)MathHelper.Clamp((int)NPC.Center.X / 16, 500, Main.maxTilesX - 500);
                int basePosX = Main.maxTilesX / 2;
                for (int i = -300; i < 300; i++)
                {
                    for (int j = Main.UnderworldLayer; j < Main.bottomWorld / 16 - 1; j++)
                    {
                        int posX = (int)MathHelper.Clamp(basePosX + i, 0, Main.maxTilesX);
                        //Tile tile = Main.tile[posX, j];

                        if (Main.tile[posX, j].TileType == TileID.Ash || Main.tile[posX, j].TileType == TileID.AshGrass)
                        {
                            Main.tile[posX, j].TileType = (ushort)ModContent.TileType<FrozenAshTile>();
                            WorldGen.SquareTileFrame(posX, j);
                            NetMessage.SendTileSquare(-1, posX, j, 1);
                        }

                        if (Main.tile[posX, j].TileType == TileID.Hellstone)
                        {
                            Main.tile[posX, j].TileType = (ushort)ModContent.TileType<FrozenHellstoneTile>();
                            WorldGen.SquareTileFrame(posX, j);
                            NetMessage.SendTileSquare(-1, posX, j, 1);
                        }
                        if (Main.tile[posX, j].LiquidType == LiquidID.Lava && Main.tile[posX, j].LiquidAmount > 0 && !Main.tile[posX, j].HasTile)
                        {
                            //Main.tile[posX, j].TileType = 162;
                            Main.tile[posX, j].LiquidAmount = 0;
                            WorldGen.PlaceTile(posX, j, 162, forced: true);
                            WorldGen.SquareTileFrame(posX, j);
                            NetMessage.SendTileSquare(-1, posX, j, 1);
                        }
                    }
                }
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetTextValue("Mods.Clamity.Misc.FrozenHellMessege")), Color.LightCyan);
                ClamitySystem.generatedFrozenHell = true;
                CalamityNetcode.SyncWorld();
            }

            //NPC.SetEventFlagCleared(ref ClamitySystem.downedWallOfBronze, -1);
            ClamitySystem.downedWallOfBronze = true;
            CalamityNetcode.SyncWorld();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            //writer.Write(NPC.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            //NPC.localAI[1] = reader.ReadSingle();
        }
    }
}
