using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.NPCs
{
    [LongDistanceNetSync(SyncWith = typeof(IhorHead))]
    public class IhorBody : ModNPC
    {
        public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Clamity.NPCs.IhorHead.DisplayName");
        public override void SetDefaults()
        {
            NPC.GetNPCDamageClamity();
            NPC.width = 72;
            NPC.height = 72;
            NPC.defense = 6;
            NPC.DR_NERD(0.05f);

            NPC.LifeMaxNERB(95000, 182400, 1650000);

            double HPBoost = CalamityServerConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            //NPC.alpha = 255;
            NPC.boss = true;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.netAlways = true;
            NPC.dontCountMe = true;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
        public override void AI()
        {
            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool masterMode = Main.masterMode || bossRush;

            if (NPC.ai[2] > 0f)
                NPC.realLife = (int)NPC.ai[2];

            if (NPC.life > Main.npc[(int)NPC.ai[1]].life)
                NPC.life = Main.npc[(int)NPC.ai[1]].life;

            NPC.dontTakeDamage = Main.npc[(int)NPC.ai[1]].dontTakeDamage;
            NPC.Opacity = Main.npc[(int)NPC.ai[1]].Opacity;

            bool shouldDespawn = !NPC.AnyNPCs(ModContent.NPCType<IhorHead>());
            if (!shouldDespawn)
            {
                if (NPC.ai[1] <= 0f)
                    shouldDespawn = true;
                else if (Main.npc[(int)NPC.ai[1]].life <= 0)
                    shouldDespawn = true;
            }
            if (shouldDespawn)
            {
                NPC.life = 0;
                NPC.HitEffect(0, 10.0);
                NPC.checkDead();
                NPC.active = false;
            }

            if (Main.player[NPC.target].dead)
                NPC.TargetClosest(false);

            NPC aheadSegment = Main.npc[(int)NPC.ai[1]];

            Vector2 destination = aheadSegment.Center + new Vector2(0, -aheadSegment.height / 2).RotatedBy(aheadSegment.rotation);
            NPC.velocity = (destination - NPC.Center) * 0.2f + aheadSegment.velocity * 0.075f;
            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        }
        public override bool CheckActive() => false;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.dontTakeDamage);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.dontTakeDamage = reader.ReadBoolean();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D t = ModContent.Request<Texture2D>("Clamity/Content/Bosses/Ihor/NPCs/IhorConnection").Value;
            NPC aheadSegment = Main.npc[(int)NPC.ai[1]];
            Vector2 from = NPC.Center + new Vector2(0, NPC.height / 2 - 6).RotatedBy(NPC.rotation);
            Vector2 to = aheadSegment.Center - new Vector2(0, aheadSegment.height / 2 - 6).RotatedBy(aheadSegment.rotation);

            spriteBatch.Draw(t, (from + to) / 2 - Main.screenPosition, null, drawColor * NPC.Opacity, (to - from).ToRotation() - MathHelper.PiOver2, t.Size() / 2f, new Vector2(1, (to - from).Length() / (float)t.Height), SpriteEffects.None, 0);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
