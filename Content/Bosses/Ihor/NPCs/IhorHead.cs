using CalamityMod;
using CalamityMod.NPCs;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.NPCs
{
    /*public enum IhorMagicAttacks : int
    {
        Summon = 0,
        MagicBurst,
        HomingStowballs,

        StormPillars,
    }
    public enum IhorMeleeAttacks : int
    {
        Summon = 0,
        LinearDash,
        HomingDash,
        DoGLikeDash,
    }*/
    public partial class IhorHead : ModNPC
    {
        private int biomeEnrageTimer = CalamityGlobalNPC.biomeEnrageTimerMax;
        private bool tailSpawned = false;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);

        }
        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.GetNPCDamageClamity();
            NPC.defense = 4;
            NPC.npcSlots = 12f;
            NPC.width = 114;
            NPC.height = 114;

            NPC.LifeMaxNERB(95000, 114400, 1650000);

            double HPBoost = CalamityServerConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            //NPC.alpha = 255;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.netAlways = true;

            if (!Main.dedServ)
            {
                Music = Clamity.mod.GetMusicFromMusicMod("Ihor") ?? MusicID.Boss2;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(biomeEnrageTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            biomeEnrageTimer = reader.ReadInt32();
        }
        public override void OnSpawn(IEntitySource source)
        {
            Attack = (int)Attacks.SnowAbsorbtionStar;
            //PreviousAttack = (int)IhorAttacks.MagicBurst;
            AttackTimer = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("Mods.Clamity.NPCs.IhorHead.Bestiary")
            });
        }
        public override bool CheckActive() => false;
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AttackEnum is Attacks.IcePathDash)
            {
                //Texture2D line = ModContent.Request<Texture2D>("CalamityMod/ExtraTexture/LaserWallTelegraphBeam").Value;

                SpriteEffects effects = SpriteEffects.None;
                if (NPC.spriteDirection == 1)
                {
                    effects = SpriteEffects.FlipHorizontally;
                }

                Texture2D value6 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLine").Value;
                float path = (AttackTimer % (LineTime + DashTime)) > LineTime ? 0 : AttackTimer / LineTime;
                Color color3 = Color.Lerp(Color.Blue, Color.White, path);
                spriteBatch.Draw(value6,
                                 NPC.Center /*- base.NPC.rotation.ToRotationVector2() * base.NPC.spriteDirection * 104f*/ - screenPos,
                                 null,
                                 color3,
                                 base.NPC.rotation + MathF.PI / 2f,
                                 new Vector2((float)value6.Width / 2f, value6.Height),
                                 new Vector2(1f * path, 4200f),
                                 effects,
                                 0f);
            }

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
