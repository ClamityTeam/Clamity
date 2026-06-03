using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.NPCs
{
    public class PyrogenMinion : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][31] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][24] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][323] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][20] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][39] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<BrimstoneFlames>()] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 64;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.damage = 30; //60
            NPC.defense = 15;
            NPC.LifeMaxNERB(787, 945, 9450);
            NPC.knockBackResist = 1f;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            AIType = -1;

            if (Main.zenithWorld)
            {
                NPC.Calamity().VulnerableToHeat = true;
                NPC.Calamity().VulnerableToCold = false;
                NPC.Calamity().VulnerableToSickness = false;
            }
            else
            {
                NPC.Calamity().VulnerableToHeat = false;
                NPC.Calamity().VulnerableToCold = true;
                NPC.Calamity().VulnerableToWater = true;
            }
        }

        public override void AI()
        {
            if (ClamityGlobalNPC.pyrogenBoss < 0 || !Main.npc[ClamityGlobalNPC.pyrogenBoss].active)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            NPC.damage = 0;

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;

            NPC.TargetClosest(false);

            if (NPC.velocity.LengthSquared() > 0.01f)
            {
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                NPC.spriteDirection = 1;
            }

            float inertia = death ? 26f : revenge ? 27f : expertMode ? 28f : 30f;

            if (NPC.ai[0] == 0f)
            {
                float lungeSpeed = death ? 12f : revenge ? 11f : expertMode ? 10f : 8f;

                Vector2 npcCenter = NPC.Center;
                Vector2 targetCenter = Main.player[NPC.target].Center;
                Vector2 targetDirection = targetCenter - npcCenter;
                Vector2 beginLungeYDist = targetDirection - Vector2.UnitY * 300f * NPC.scale;
                float targetDist = targetDirection.Length();
                targetDirection = Vector2.Normalize(targetDirection) * lungeSpeed;
                beginLungeYDist = Vector2.Normalize(beginLungeYDist) * lungeSpeed;
                bool canHitPlayer = Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1);
                if (NPC.ai[3] >= 120f)
                {
                    canHitPlayer = true;
                }
                canHitPlayer = canHitPlayer && targetDirection.ToRotation() > MathHelper.Pi / 8f && targetDirection.ToRotation() < MathHelper.Pi - MathHelper.Pi / 8f;
                if (targetDist > 800f * NPC.scale || !canHitPlayer)
                {
                    NPC.velocity.X = (NPC.velocity.X * (inertia - 1f) + beginLungeYDist.X) / inertia;
                    NPC.velocity.Y = (NPC.velocity.Y * (inertia - 1f) + beginLungeYDist.Y) / inertia;
                    if (!canHitPlayer)
                    {
                        NPC.ai[3] += 1f;
                        if (NPC.ai[3] == 120f)
                        {
                            NPC.netUpdate = true;
                        }
                    }
                    else
                    {
                        NPC.ai[3] = 0f;
                    }
                }
                else
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[2] = targetDirection.X;
                    NPC.ai[3] = targetDirection.Y;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                NPC.velocity *= 0.8f;
                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 5f)
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                    Vector2 velocity = new Vector2(NPC.ai[2], NPC.ai[3]);
                    velocity.Normalize();
                    velocity *= 10f;
                    NPC.velocity = velocity;
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                NPC.ai[1] += 1f;
                bool doLunge = NPC.Center.Y + 50f > Main.player[NPC.target].Center.Y;
                if ((NPC.ai[1] >= 90f && doLunge) || NPC.velocity.Length() < 8f)
                {
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = 45f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.velocity /= 2f;
                    NPC.netUpdate = true;
                }
                else
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    Vector2 npcCenterAgain = NPC.Center;
                    Vector2 targetCenterAgain = Main.player[NPC.target].Center;
                    Vector2 vec2 = targetCenterAgain - npcCenterAgain;
                    vec2.Normalize();
                    if (vec2.HasNaNs())
                    {
                        vec2 = new Vector2((float)NPC.direction, 0f);
                    }
                    NPC.velocity = (NPC.velocity * (inertia - 1f) + vec2 * (NPC.velocity.Length() + (0.111111117f * inertia))) / inertia;
                }
            }
            else if (NPC.ai[0] == 3f)
            {
                NPC.ai[1] -= 1f;
                if (NPC.ai[1] <= 0f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
                NPC.velocity *= 0.98f;
            }

            if (death)
            {
                float pushVelocity = 0.5f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.whoAmI != NPC.whoAmI && n.type == NPC.type)
                    {
                        if (Vector2.Distance(NPC.Center, n.Center) < 80f * NPC.scale)
                        {
                            if (NPC.position.X < n.position.X)
                                NPC.velocity.X -= pushVelocity;
                            else
                                NPC.velocity.X += pushVelocity;

                            if (NPC.position.Y < n.position.Y)
                                NPC.velocity.Y -= pushVelocity;
                            else
                                NPC.velocity.Y += pushVelocity;
                        }
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
                target.AddBuff(BuffID.Bleeding, 120);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dusttype = 235;
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    int icyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[icyDust].scale = 0.5f;
                        Main.dust[icyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 50; j++)
                {
                    int icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 3f);
                    Main.dust[icyDust2].noGravity = true;
                    Main.dust[icyDust2].velocity *= 5f;
                    icyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, dusttype, 0f, 0f, 100, default, 2f);
                    Main.dust[icyDust2].velocity *= 2f;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC.DrawBackglow(PyrogenBoss.BackglowColor, 4f, SpriteEffects.None, NPC.frame, screenPos);

            return true;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override bool CheckActive() => false;
    }
}
