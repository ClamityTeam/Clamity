using CalamityMod;
using CalamityMod.Sounds;
using Clamity.Commons;
using Clamity.Content.Bosses.WoB.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.NPCs
{
    public class WallOfBronzeClaw : BaseWoBGunAI
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 2;
        }
        public override void SetDefaults()
        {
            NPC.npcSlots = 5f;
            NPC.damage = 200;
            NPC.width = 100;
            NPC.height = 100;
            NPC.defense = 50;
            NPC.DR_NERD(0.2f);
            NPC.LifeMaxNERB(10000, 20000, 40000);
            NPC.lifeMax += (int)(NPC.lifeMax * CalamityConfig.Instance.BossHealthBoost * 0.01f);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.Opacity = 0.0f;
            NPC.knockBackResist = 0.0f;
            NPC.canGhostHeal = false;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.DeathSound = CommonCalamitySounds.ExoDeathSound;
            NPC.HitSound = SoundID.NPCHit4;
            //NPC.netUpdate = true;
            //NPC.netAlways = true;
            NPC.hide = true;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            if (Main.getGoodWorld)
                NPC.scale = 0.75f;
        }
        public override int MaxParticleTimer => 200;
        public override int MaxTimer => 500;
        public ref float ClawProj => ref NPC.Calamity().newAI[2];
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            ClawProj = -1;
            NPC.frame = new Rectangle(0, 0, 150, 60);
        }
        public override void Attack()
        {
            if (ClawProj == -1)
            {
                ClawProj = Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                                    NPC.Center,
                                                    Vector2.UnitX.RotatedBy(NPC.rotation) * 10,
                                                    ModContent.ProjectileType<WallOfBronzeClawProjectile>(),
                                                    NPC.GetProjectileDamageClamity(ModContent.ProjectileType<WallOfBronzeClawProjectile>()),
                                                    0,
                                                    Main.myPlayer,
                                                    NPC.whoAmI);
                if (Main.getGoodWorld)
                    Main.projectile[(int)ClawProj].scale = 0.75f;
            }
            else
            {
                //Main.NewText(ClawProj + " " + NPC.whoAmI);
                if (Main.projectile[(int)ClawProj] == null || !Main.projectile[(int)ClawProj].active)
                {
                    ClawProj = -1;
                    AIState = 0;
                    //NPC.netUpdate = true;
                }
            }
        }
        public override void ExtraAI()
        {
            if (ClawProj != -1)
            {
                //NPC.rotation = (Main.projectile[(int)ClawProj].Center - NPC.Center).ToRotation();
                NPC.rotation = (Main.projectile[(int)ClawProj].Center - NPC.Center).ToRotation();
            }
            else
            {
                //NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation() + (NPC.spriteDirection == 1 ? MathF.PI : 0);
                NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            if (GetWoB().spriteDirection == -1)
            {
                effects = SpriteEffects.FlipVertically;
            }
            //Texture2D texture2D1 = TextureAssets.Npc[NPC.type].Value;
            Texture2D texture2D1 = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle rectangle = new Rectangle(0, 0, texture2D1.Width, texture2D1.Height / 2);
            if (ClawProj != -1)
                rectangle.Y += 60;
            Vector2 offset = texture2D1.Size() / 2f;
            offset.Y /= 2;

            Vector2 vector2 = NPC.Center - screenPos;
            if (ParticleTimer < MaxParticleTimer && AIState == 1)
            {
                spriteBatch.EnterShaderRegion();
                Color color2 = Color.Lerp(Color.Magenta, Color.White, ParticleTimer / MaxParticleTimer);
                float num2 = MathHelper.Clamp(ParticleTimer / MaxParticleTimer * 4f, 0f, 3f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseColor(color2);
                GameShaders.Misc["CalamityMod:BasicTint"].Apply();
                for (float num3 = 0f; num3 < 1f; num3 += 0.125f)
                {
                    spriteBatch.Draw(texture2D1, vector2 + (num3 * (MathF.PI * 2f) + NPC.rotation).ToRotationVector2() * num2, rectangle, color2, base.NPC.rotation, offset, base.NPC.scale, effects, 0f);
                }
                spriteBatch.ExitShaderRegion();
            }

            Main.spriteBatch.Draw(texture2D1, vector2, rectangle, NPC.GetAlpha(drawColor), NPC.rotation, offset, NPC.scale, effects, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            if (ParticleTimer < MaxParticleTimer && AIState == 1)
            {
                ExtraDraw(spriteBatch, screenPos, drawColor);
                float num41 = MaxParticleTimer / 5f;
                float num4 = ParticleTimer % num41 / num41;
                float num5 = MathHelper.Lerp(0.1f, 0.6f, (float)Math.Floor(ParticleTimer / num41) / 4f);
                float num6 = MathHelper.Clamp((float)Math.Floor(ParticleTimer / num41) * 0.3f, 1f, 2f);
                spriteBatch.Draw(texture2D1, vector2, rectangle, Color.Magenta * MathHelper.Lerp(1f, 0f, num4) * num6, base.NPC.rotation, offset, base.NPC.scale + num4 * num5, effects, 0f);
                EnergyDrawer.DrawBloom(NPC.Center);
            }
            EnergyDrawer.DrawPulses(NPC.Center);
            EnergyDrawer.DrawSet(NPC.Center);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            //ModContent.GetInstance<WallOfBronze>().PostDraw(spriteBatch, screenPos, drawColor);
            return false;
        }
    }
}
