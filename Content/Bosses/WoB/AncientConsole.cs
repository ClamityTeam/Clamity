using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Bosses.WoB.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Clamity.Content.Bosses.WoB
{
    public class AncientConsole : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.SummonBoss";

        public override void SetDefaults()
        {
            Item.createTile = ModContent.TileType<AncientConsoleTile>();
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.width = 38;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MartianConduitPlating, 30)
                .AddIngredient<AuricBar>(5)
                .AddIngredient<CoreofCalamity>()
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class AncientConsoleTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[3]
            {
                16,
                16,
                16
            };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            this.AddMapEntry(new Color(43, 19, 42), CalamityUtils.GetItemName<AncientConsole>());
            TileID.Sets.DisableSmartCursor[Type] = true;
        }

        public static readonly SoundStyle SummonSound = new SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/SepulcherSpawn")
        {
            Volume = 1.1f,
            Pitch = 0.2f
        };
        public override bool CanExplode(int i, int j) => false;
        public override bool RightClick(int i, int j)
        {
            /*if (NPC.AnyNPCs(ModContent.NPCType<WallOfBronze>()) || BossRushEvent.BossRushActive || !Main.LocalPlayer.ZoneUnderworldHeight)
                return true;

            //CalamityUtils.
            Vector2 tilePosInWorld = new Vector2(i * 16, j * 16);
            Dictionary<int, float> distance = new Dictionary<int, float>();
            foreach (Player p in Main.player)
            {
                if (p == null) continue;
                if (!p.active || p.dead) continue;
                distance.Add(p.whoAmI, Vector2.Distance(p.Center, tilePosInWorld));
            }
            float min = float.MaxValue; int thisPlayer = -1;
            foreach (var d in distance)
            {
                if (d.Value < min)
                {
                    min = d.Value;
                    thisPlayer = d.Key;
                }
            }
            if (thisPlayer != -1)
            {
                Player player = Main.player[thisPlayer];
                int center = Main.maxTilesX * 16 / 2;

                Tile tile = Main.tile[i, j];
                int left = i - tile.TileFrameX / 18;
                int top = j - tile.TileFrameY / 18;
                Vector2 ritualSpawnPosition = new Vector2(left + 1.5f, top).ToWorldCoordinates();
                ritualSpawnPosition += new Vector2(0f, -24f);
                Projectile.NewProjectile(new EntitySource_WorldEvent(), ritualSpawnPosition, Vector2.Zero, ModContent.ProjectileType<WoBCutsceneDrama>(), 0, 0f, thisPlayer, player.Center.X > center ? -1 : 1);
                //NPC.NewNPC(player.GetSource_ItemUse(new Item(ModContent.ItemType<WoBSummonItem>())), (int)player.Center.X - 1000 * (player.Center.X > center ? -1 : 1), (int)player.Center.Y, ModContent.NPCType<WallOfBronze>());

                //NPC.NewNPC(player.GetSource_ItemUse(new Item(ModContent.ItemType<WoBSummonItem>())), (int)player.Center.X - 1000 * (player.Center.X > center ? -1 : 1), (int)player.Center.Y, ModContent.NPCType<WallOfBronze>());
                //Projectile.NewProjectile(NPC.GetSource_None(), Vector2.Zero, Vector2.Zero, ModContent.ProjectileType<AncientConsoleProjectile>(), 0, 0, Main.myPlayer);

                //if (Main.netMode != NetmodeID.MultiplayerClient)
                //    NPC.SpawnOnPlayer(Main.myPlayer, ModContent.NPCType<WallOfBronze>());
                //else
                //    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, Main.myPlayer, (int)ModContent.NPCType<WallOfBronze>());
            }*/


            Tile tile = Main.tile[i, j];

            int left = i - tile.TileFrameX / 18;
            int top = j - tile.TileFrameY / 18;
            int center = Main.maxTilesX / 2;

            if (NPC.AnyNPCs(ModContent.NPCType<WallOfBronze>()) || BossRushEvent.BossRushActive)
                return true;

            if (CalamityUtils.CountProjectiles(ModContent.ProjectileType<AncientConsoleProjectile>()) > 0)
                return true;

            Vector2 ritualSpawnPosition = new Vector2(left + 1.5f, top).ToWorldCoordinates(); //(int)player.Center.X - 1000 * (player.Center.X > center ? -1 : 1)
            ritualSpawnPosition += new Vector2(1000 * (left > center ? -1 : 1), 0f);

            SoundEngine.PlaySound(SummonSound, ritualSpawnPosition);
            Projectile.NewProjectile(new EntitySource_WorldEvent(), ritualSpawnPosition, Vector2.Zero, ModContent.ProjectileType<AncientConsoleProjectile>(), 0, 0f, Main.myPlayer);

            return true;
        }
    }
    public class AncientConsoleProjectile : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.timeLeft = 1;
        }
        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                //NPC.NewNPC(player.GetSource_ItemUse(new Item(ModContent.ItemType<WoBSummonItem>())), (int)player.Center.X - 1000 * (player.Center.X > Main.maxTilesX * 16 / 2 ? -1 : 1), (int)player.Center.Y, ModContent.NPCType<WallOfBronze>());
                NPC scal = CalamityUtils.SpawnBossBetter(Projectile.Center, ModContent.NPCType<WallOfBronze>(), null);
            }
        }
    }
    public class WoBCutsceneDrama : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float Direction => ref Projectile.ai[0];
        public const int PreFlyRitualTime = 270;
        public const int PostFlyRitualTime = 180;
        public const int SoemExtraTime = 420;
        public override void SetStaticDefaults()
        {
            //ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = PreFlyRitualTime + PostFlyRitualTime;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == PreFlyRitualTime + PostFlyRitualTime - 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Pink, 0f, 0.55f, PreFlyRitualTime + 1, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom2 = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Magenta, 0f, 0.5f, PreFlyRitualTime + 1, false);
                GeneralParticleHandler.SpawnParticle(bloom2);
            }
            if (Projectile.timeLeft < PostFlyRitualTime - 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Pink, 0.55f, 0.5f, 2, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom2 = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Magenta, 0.5f, 0.45f, 2, false);
                GeneralParticleHandler.SpawnParticle(bloom2);

                if (Projectile.timeLeft > PostFlyRitualTime - 11)
                {
                    //Projectile.velocity = new Vector2(Projectile.velocity.X + .1f * Direction, 0);
                    Projectile.velocity += new Vector2(.4f * Direction, 0);
                }
                //Projectile.velocity = new Vector2(Projectile.velocity.X - .05f * Direction, 0);
                Projectile.velocity -= new Vector2(.2f * Direction, 0);
            }
            if (Projectile.timeLeft == 4)
                SummonWallOfBronze(Projectile.Center);

            /*if (Projectile.timeLeft <= 2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs(ModContent.NPCType<WallOfBronze>()))
                    Projectile.Kill();
                return;
            }*/
        }

        public void SummonWallOfBronze(Vector2 center)
        {
            //NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<WallOfBronze>());
            //Projectile.velocity = Vector2.Zero;
            //Main.NewText(center.ToString());
            //Main.NewText(Projectile.velocity.ToString());

            //Summon WoB itself
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                //CalamityUtils.SpawnBossBetter(Projectile.Center, ModContent.NPCType<WallOfBronze>());
                NPC.NewNPC(/*NPC.GetBossSpawnSource(Player.FindClosest(center, 1, 1))*/ Projectile.GetSource_FromAI(), (int)center.X, (int)center.Y, ModContent.NPCType<WallOfBronze>());
            }

            //Cool Explosion

            for (int i = 0; i < 7; i++)
            {
                Projectile explosion = Projectile.NewProjectileDirect(/*new EntitySource_WorldEvent()*/ Projectile.GetSource_FromAI(), center, Vector2.Zero, ModContent.ProjectileType<WoBCutsceneBoom>(), 0, 0, Projectile.owner);
                if (explosion.whoAmI.WithinBounds(Main.maxProjectiles))
                {
                    explosion.ai[1] = Main.rand.NextFloat(620f, 1670f) + i * 45f; // Randomize the maximum radius.
                    explosion.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f); // And the interpolation step.
                    explosion.Opacity = MathHelper.Lerp(0.18f, 0.6f, i / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
                    explosion.netUpdate = true;
                }
            }

            //Main.player[Projectile.owner].Center = center;
            Projectile.Kill();
        }
    }
    public class WoBCutsceneBoom : BaseMassiveExplosionProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override int Lifetime => 60;
        public override bool UsesScreenshake => true;
        public override float GetScreenshakePower(float pulseCompletionRatio) => CalamityUtils.Convert01To010(pulseCompletionRatio) * 4f;
        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.Magenta * 1.6f, Color.White, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = Lifetime;
        }

        public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
    }
}
