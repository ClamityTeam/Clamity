using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.NPCs
{
    public abstract class BaseWoBGunAI : ModNPC
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            //NPCID.Sets.TrailingMode[NPC.type] = 3;
            //NPCID.Sets.TrailCacheLength[NPC.type] = NPC.oldPos.Length;
        }
        public ref float WoB => ref NPC.ai[0];
        public ref float PosY => ref NPC.ai[1];
        public ref float Timer => ref NPC.ai[2];
        public ref float ParticleTimer => ref NPC.ai[3];
        public ref float AIState => ref NPC.Calamity().newAI[0];
        public ref float RandomSpeed => ref NPC.Calamity().newAI[1];
        public abstract int MaxTimer { get; }
        public abstract int MaxParticleTimer { get; }
        public Terraria.NPC GetWoB() => Main.npc[(int)WoB];
        public AresCannonChargeParticleSet EnergyDrawer = new AresCannonChargeParticleSet(-1, 15, 40f, Color.Magenta);
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {

                int findWoB = NPC.FindFirstNPC(ModContent.NPCType<WallOfBronze>());
                if (findWoB != -1) WoB = findWoB;
                else NPC.active = false;

                PosY = Main.rand.NextFloat(200, 500) * (Main.rand.NextBool() ? -1 : 1);
                NPC.Center = GetWoB().Center;
                NPC.Center = new Vector2(GetWoB().Center.X, GetWoB().Center.Y + PosY);
                //NPC.spriteDirection = 1;

                AIState = 0;
                Timer = (int)(MaxTimer * Main.rand.NextFloat(0.7f, 0.9f));
                ParticleTimer = 0;
                RandomSpeed = Main.rand.NextFloat(0, 1);
                NPC.netUpdate = true;
            }
        }
        public override void AI()
        {
            if (!GetWoB().active || GetWoB() == null)
            {
                NPC.active = false;
            }
            if (NPC.target < 0 || NPC.target == byte.MaxValue || Main.player[NPC.target].Center.Y < Main.UnderworldLayer * 16 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(true);
                if ((NPC.target == (int)byte.MaxValue || Main.player[NPC.target].Center.Y < Main.UnderworldLayer * 16 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) && !NPC.despawnEncouraged)
                    NPC.EncourageDespawn(30);
            }
            Vector2 center = Main.player[NPC.target].Center + new Vector2(0, PosY);
            //NPC.spriteDirection = NPC.direction = GetWoB().spriteDirection;
            //NPC.spriteDirection = GetWoB().spriteDirection;
            NPC.Center = new Vector2(GetWoB().Center.X, NPC.Center.Y);
            NPC.velocity = new Vector2(0, Math.Sign(center.Y - NPC.Center.Y) * RandomSpeed);
            //NPC.position.Y += Math.Sign(center.Y - NPC.Center.Y) * RandomSpeed;
            //NPC.position.Y = MathHelper.Clamp(NPC.position.Y + Math.Sign(center.Y - NPC.Center.Y) * RandomSpeed, GetWoB().Center.Y - 100, GetWoB().Center.Y + 100);
            NPC.Opacity = MathHelper.Clamp(NPC.Opacity + 0.02f, 0, 1);
            EnergyDrawer.ParticleSpawnRate = 9999999;
            switch (AIState)
            {
                case 0:
                    Timer++;
                    if (Timer >= MaxTimer)
                    {
                        Timer = 0;
                        AIState = 1;
                        NPC.netUpdate = true;
                    }
                    break;
                case 1:
                    ParticleTimer++;
                    if (ParticleTimer >= MaxParticleTimer)
                    {
                        ParticleTimer = 0;
                        AIState = 2;
                        NPC.netUpdate = true;
                    }
                    AIinParticleState();
                    EnergyDrawer.ParticleSpawnRate = 5;
                    EnergyDrawer.SpawnAreaCompactness = 100f;
                    EnergyDrawer.chargeProgress = ParticleTimer / MaxParticleTimer;
                    break;
                case 2:
                    Attack();
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        PosY = Main.rand.NextFloat(200, 500) * (Main.rand.NextBool() ? -1 : 1);
                        NPC.netUpdate = true;
                    }
                    break;
            }

            EnergyDrawer.Update();
            ExtraAI();
        }
        public virtual void ExtraAI()
        {

        }
        public virtual void Attack()
        {

        }
        public virtual void AIinParticleState()
        {

        }
        public virtual void ExtraDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //AresTeslaCannon

            SpriteEffects effects = SpriteEffects.None;
            if (GetWoB().spriteDirection == -1)
            {
                effects = SpriteEffects.FlipVertically;
            }
            //Texture2D texture2D1 = TextureAssets.Npc[NPC.type].Value;
            Texture2D texture2D1 = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle rectangle = new Rectangle(0, 0, texture2D1.Width, texture2D1.Height);
            Vector2 offset = texture2D1.Size() / 2f;

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
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //ModContent.GetInstance<WallOfBronze>().PostDraw(spriteBatch, screenPos, drawColor);

            Texture2D texture2D1 = ModContent.Request<Texture2D>(ModContent.GetInstance<WallOfBronze>().Texture + "_Extra").Value;
            if (Main.getGoodWorld || Main.xMas)
                texture2D1 = ModContent.Request<Texture2D>(ModContent.GetInstance<WallOfBronze>().Texture + "_Extra_GFB").Value;

            int num = Main.screenHeight / 32 + 1;
            int num1 = texture2D1.Height;
            SpriteEffects spriteEffects = GetWoB().spriteDirection != 1 ? (SpriteEffects)1 : (SpriteEffects)0;
            for (int index = -num; index <= num; ++index)
            {
                if (Main.UnderworldLayer < (int)(Main.LocalPlayer.Center.Y / 16f) + index)
                    spriteBatch.Draw(texture2D1,
                                    new Vector2(NPC.Center.X - GetWoB().spriteDirection * texture2D1.Width * 0.5f - Math.Sign(NPC.velocity.X), (float)((int)Main.LocalPlayer.Center.Y / num1 * num1 + index * texture2D1.Height)) - screenPos,
                                    new Rectangle?(),
                                    Lighting.GetColor((int)(NPC.Center.X - NPC.spriteDirection * texture2D1.Width * 0.5) / 16, (int)(Main.LocalPlayer.Center.Y / 16) + index),
                                    0.0f,
                                    Utils.Size(texture2D1) * 0.5f,
                                    1f,
                                    spriteEffects,
                                    0.0f);
            }
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsOverPlayers.Add(index);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.dontTakeDamage);
            writer.Write(PosY);
            writer.Write(AIState);
            writer.Write(RandomSpeed);

            //for (int index = 0; index < 4; index++)
            //    writer.Write(NPC.Calamity().newAI[index]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.dontTakeDamage = reader.ReadBoolean();
            PosY = reader.ReadSingle();
            AIState = reader.ReadSingle();
            RandomSpeed = reader.ReadSingle();

            //for (int index = 0; index < 4; index++)
            //    NPC.Calamity().newAI[index] = reader.ReadSingle();
        }
        //public override bool CheckActive() => false;
        public override void ApplyDifficultyAndPlayerScaling(
            int numPlayers,
            float balance,
            float bossAdjustment)
        {
            this.NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance);
            this.NPC.damage = (int)(NPC.damage * 0.8f);
        }
    }
}
