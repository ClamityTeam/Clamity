using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.World;
using Terraria.Audio;
using Clamity.Content.Bosses.Pyrogen.NPCs;

namespace Clamity.Content.Bosses.Pyrogen
{
    public class AzafureFurnaceTile : ModTile
    {
        public static readonly SoundStyle SummonSound = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneMonsterSpawn");

        public const int Width = 8;
        public const int Height = 5;

        public static bool WaitingForPlayersToLeaveArea
        {
            get;
            set;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 8;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(43, 19, 42), CalamityUtils.GetItemName<AzafureFurnace>());
        }
        public override bool CanExplode(int i, int j) => false;

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Red torch dust.
            type = 60;
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (WaitingForPlayersToLeaveArea && !Main.player[Player.FindClosest(GetRitualSpawnPos(i, j), 1, 1)].WithinRange(GetRitualSpawnPos(i, j), 2000f))
                WaitingForPlayersToLeaveArea = false;

            if (!closer && !WaitingForPlayersToLeaveArea)
                AttemptToSummonPyrogen(i, j);
        }

        public override bool RightClick(int i, int j) => StartPyogenFight(i, j);
        public override void MouseOver(int i, int j) => HoverItemIcon(i, j);
        public override void MouseOverFar(int i, int j) => HoverItemIcon(i, j);
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == ModContent.ProjectileType<SCalAltarArenaVisual>())
                {
                    p.Kill();
                    break;
                }
            }
        }

        public static void HoverItemIcon(int i, int j)
        {
            if (Main.npc[ClamityGlobalNPC.pyrogenBoss].ai[0] > 0 || BossRushEvent.BossRushActive)
                return;

            Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<PyroKey>();

            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.cursorItemIconEnabled = true;

            // Checks if the player has the Ruler lines or Ruler grid toggled
            if (Main.LocalPlayer.builderAccStatus[0] == 0 || (Main.LocalPlayer.builderAccStatus[1] == 0 && Main.LocalPlayer.rulerGrid))
            {
                // Don't spawn the arena visual if one already exists or if SCal is alive or spawning
                if (CalamityUtils.AnyProjectiles(ModContent.ProjectileType<SCalAltarArenaVisual>()) ||
                    CalamityUtils.AnyProjectiles(ModContent.ProjectileType<SCalRitualDrama>()) ||
                    NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
                    return;

                Tile t = Main.tile[i, j];
                Vector2 arenaCenter = new Vector2(i - t.TileFrameX / 18 + Width / 2, j - t.TileFrameY / 18).ToWorldCoordinates() - Vector2.UnitY * 24f;
                Projectile.NewProjectile(new EntitySource_WorldEvent(), arenaCenter, Vector2.Zero, ModContent.ProjectileType<SCalAltarArenaVisual>(), 0, 0f, Main.myPlayer, CalamityWorld.death.ToInt());
            }
        }

        public static void AttemptToSummonPyrogen(int i, int j)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<PyrogenBoss>()) || BossRushEvent.BossRushActive)
                return;

            if (CalamityUtils.CountProjectiles(ModContent.ProjectileType<PyrogenSummonAnimation>()) > 0)
                return;

            Projectile.NewProjectile(new EntitySource_WorldEvent(), GetRitualSpawnPos(i, j), Vector2.Zero, ModContent.ProjectileType<PyrogenSummonAnimation>(), 0, 0f, Main.myPlayer, 0);
        }

        public static bool StartPyogenFight(int i, int j)
        {
            if (!Main.LocalPlayer.HasItem(ModContent.ItemType<PyroKey>()))
            {
                return false;
            }

            AttemptToSummonPyrogen(i, j);

            if (ClamityGlobalNPC.pyrogenBoss != -1)
            {
                NPC pyrogen = Main.npc[ClamityGlobalNPC.pyrogenBoss];

                if (pyrogen.ai[0] == 0)
                {
                    SoundEngine.PlaySound(SummonSound, pyrogen.Center);
                    pyrogen.ai[0] = 2;
                    return true;
                }
            }
            return false;
        }

        public static Vector2 GetRitualSpawnPos(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int left = i - tile.TileFrameX / 18;
            int top = j - tile.TileFrameY / 18;
            Vector2 ritualSpawnPosition = new Vector2(left + Width / 2, top).ToWorldCoordinates();
            ritualSpawnPosition += new Vector2(-7f, 29f);

            return ritualSpawnPosition;
        }
    }
}
