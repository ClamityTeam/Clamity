using CalamityMod;
using CalamityMod.BiomeManagers;
using CalamityMod.World;
using Clamity.Commons;
using Clamity.Content.Bosses.ForbiddenLantern.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.ForbiddenLantern.NPCs
{
    public class TheForbiddenLantern : ModNPC
    {
        public enum Attacks
        {
            Idle = 0,
            SeashineMine = 1,
            WaterRay = 2,
            SoundWave = 3,
            SummonLanterns = 4,
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
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers nPCBestiaryDrawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers();
            nPCBestiaryDrawModifiers.Scale = 0.4f;
            NPCID.Sets.NPCBestiaryDrawModifiers value = nPCBestiaryDrawModifiers;
            value.Position.Y += 40f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;

        }
        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 5f;
            NPC.damage = 50;
            NPC.width = 86;
            NPC.height = 118;
            NPC.defense = 30;
            NPC.DR_NERD(0.15f);
            //base.NPC.lifeMax = (Main.hardMode ? 7500 : 1250);
            //NPC.lifeMax = 50000;
            NPC.LifeMaxNERB(40000, 50000, 500000);
            NPC.aiStyle = -1;
            AIType = -1;
            //base.NPC.value = (Main.hardMode ? Item.buyPrice(0, 8) : Item.buyPrice(0, 1));
            NPC.value = Item.buyPrice(0, 15);
            NPC.HitSound = SoundID.Item50;
            NPC.knockBackResist = 0f;
            NPC.rarity = 2;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
            NPC.boss = true;


            if (!Main.dedServ)
            {
                //Music = Clamity.mod.GetMusicFromMusicMod("Clamitas") ?? MusicID.Boss3;
                Music = MusicID.Boss3;
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[1]
            {
                new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.TheForbiddenLantern.Bestiary")
            });
        }
        public override void SendExtraAI(BinaryWriter writer)
        {

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {

        }
        public static int LightChargeUp => 30;
        public override void AI()
        {
            Myself = NPC;
            NPC.TargetClosest();

            bool anyAlive = false;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && p.Calamity().ZoneSunkenSea)
                {
                    anyAlive = true;
                    break;
                }
            }

            if (!anyAlive)
            {
                NPC.active = false;
                return;
            }

            NPC.velocity = Vector2.UnitY * MathF.Sin(Main.GlobalTimeWrappedHourly * 1) * 1f;

            switch ((Attacks)NPC.ai[0])
            {
                case Attacks.Idle:
                    if (NPC.ai[1] >= (CalamityWorld.death ? 120 : 180))
                    {
                        NPC.ai[0] = Main.rand.Next(1, 5);
                        NPC.ai[0] = 2; //termporal
                        NPC.ai[1] = 0;
                    }
                    break;
                case Attacks.SeashineMine:
                    if (NPC.ai[1] % SeashineMine.Delay == 0)
                    {
                        int mine = ModContent.ProjectileType<SeashineMine>();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, mine, NPC.GetProjectileDamageClamity(mine), 0f, Main.myPlayer, NPC.whoAmI, (int)NPC.ai[1] / SeashineMine.Delay);

                    }
                    if (NPC.ai[1] >= SeashineMine.Delay * SeashineMine.MineTotalCount)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[1] = 0;
                    }
                    break;
                case Attacks.WaterRay:
                    if (NPC.ai[1] == LightChargeUp)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int waterray = ModContent.ProjectileType<WaterRayLight>();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, waterray, NPC.GetProjectileDamageClamity(waterray), 0f, Main.myPlayer, NPC.whoAmI, i);

                        }
                    }
                    if (NPC.ai[1] >= 120)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[1] = 0;
                    }
                    break;
                case Attacks.SoundWave:
                    NPC.ai[0] = 0;
                    NPC.ai[1] = (CalamityWorld.death ? 120 : 180);
                    break;
                case Attacks.SummonLanterns:
                    NPC.ai[0] = 0;
                    NPC.ai[1] = (CalamityWorld.death ? 120 : 180);
                    break;
            }
            NPC.ai[1]++;

        }

        public override bool CheckActive()
        {
            bool anyAlive = false;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && p.Calamity().ZoneSunkenSea)
                {
                    anyAlive = true;
                    break;
                }
            }

            if (!anyAlive)
                return true;

            Player target = Main.player[NPC.target];
            return Vector2.Distance(target.Center, NPC.Center) > 5600f;
        }
        public override void OnKill()
        {

            //ClamitySystem.downedClamitas = true;
            //CalamityNetcode.SyncWorld();
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //npcLoot.Add(ModContent.ItemType<BrimstoneSlag>(), 1, 30, 40);

        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D l = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            float lightScale = 0;
            if (NPC.ai[0] == (int)Attacks.WaterRay) lightScale = MathF.Sin(MathHelper.Clamp(NPC.ai[1] / LightChargeUp, 0, 1));

            spriteBatch.Draw(l, NPC.Center - Main.screenPosition, null, Color.White, 0, l.Size()/2, new Vector2(1f, 4f) * NPC.scale * lightScale, SpriteEffects.None, 1);
            spriteBatch.Draw(l, NPC.Center - Main.screenPosition, null, Color.White, 0 + MathHelper.PiOver2, l.Size()/2, new Vector2(1f, 4f) * NPC.scale * lightScale, SpriteEffects.None, 1);

            spriteBatch.Draw(l, NPC.Center - Main.screenPosition, null, Color.LightCyan, 0, l.Size()/2, new Vector2(1f, 4f) * NPC.scale * lightScale * .8f, SpriteEffects.None, 1);
            spriteBatch.Draw(l, NPC.Center - Main.screenPosition, null, Color.LightCyan, 0 + MathHelper.PiOver2, l.Size()/2, new Vector2(1f, 4f) * NPC.scale * lightScale * .8f, SpriteEffects.None, 1);



        }
    }
}
