using CalamityMod;
using CalamityMod.NPCs;
using Clamity.Commons;
using Clamity.Content.Biomes.EntropicSpace.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.EntropicSpace.NPCs
{
    public class SeedOfFear : ModNPC
    {
        public SeedOfFearHook[] hooks = new SeedOfFearHook[3];
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 44;
            NPC.aiStyle = -1;
            AIType = -1;

            NPC.damage = 300;
            NPC.lifeMax = 10000;
            NPC.defense = 30;
            NPC.knockBackResist = 0f;

            SpawnModBiomes = new int[] { ModContent.GetInstance<EntropicSpaceBiome>().Type, ModContent.GetInstance<NightmareForestBiome>().Type };

            // Scale stats in Expert and Master
            //CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            //CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }
        public override void OnSpawn(IEntitySource source)
        {
            hooks.CreateAll(NPC.Center);
        }
        public int CurrentHookInt
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public SeedOfFearHook CurrentHook => hooks[CurrentHookInt];
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 3200f)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            hooks.UpdateAll();

            NPC.ai[2]++;
            if (CurrentHook.ai[1] == 1 && ((CurrentHook.Center.Distance(player.Center) > 16 * 20 && NPC.ai[1] > 240) || NPC.ai[1] > 600))
            {
                CurrentHook.ai[0] = 0;

                CurrentHookInt++;
                if (CurrentHookInt >= hooks.Length)
                    CurrentHookInt = 0;
            }

            if (NPC.Distance(CurrentHook.Center) > 16 * 10)
            {
                NPC.velocity += NPC.SafeDirectionTo(CurrentHook.Center) * 0.5f;
                NPC.rotation += NPC.SafeDirectionTo(CurrentHook.Center).X * 0.1f;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            hooks.NetSendAll(writer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hooks.NetReceiveAll(reader);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D eye = ModContent.Request<Texture2D>(Texture + "_Eye").Value;
            Texture2D chain = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Texture2D hook = ModContent.Request<Texture2D>(Texture + "_Hook").Value;

            //Hook drawing
            for (int i = 0; i < hooks.Length - 1; i++)
            {
                SeedOfFearHook part = hooks[i];
                Vector2 partCenter = part.Center;

                Vector2 center = new Vector2(NPC.Center.X, NPC.Center.Y);
                float drawPositionX = part.Center.X - NPC.Center.X;
                float drawPositionY = part.Center.Y - partCenter.Y;
                float rotation = (float)Math.Atan2((double)drawPositionY, (double)drawPositionX) - 1.57f;
                bool draw = true;
                while (draw)
                {
                    float totalDrawDistance = (float)Math.Sqrt((double)(drawPositionX * drawPositionX + drawPositionY * drawPositionY));
                    if (totalDrawDistance < 16f)
                    {
                        draw = false;
                    }
                    else
                    {
                        totalDrawDistance = 16f / totalDrawDistance;
                        drawPositionX *= totalDrawDistance;
                        drawPositionY *= totalDrawDistance;
                        center.X += drawPositionX;
                        center.Y += drawPositionY;
                        drawPositionX = Main.npc[CalamityGlobalNPC.energyFlame].Center.X - center.X;
                        drawPositionY = Main.npc[CalamityGlobalNPC.energyFlame].Center.Y - center.Y;
                        drawPositionY -= 10f;
                        Color color = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16f));
                        Main.spriteBatch.Draw(chain, new Vector2(center.X - screenPos.X, center.Y - screenPos.Y),
                            new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, chain.Width, chain.Height)), color, rotation,
                            new Vector2((float)chain.Width * 0.5f, (float)chain.Height * 0.5f), 1f, SpriteEffects.None, 0f);
                    }
                }


                spriteBatch.Draw(hook, partCenter, null, Lighting.GetColor((int)partCenter.X / 16, (int)(partCenter.Y / 16f)), part.AngleFrom(NPC.Center), hook.Size() / 2, 1f, SpriteEffects.None, 0);
            }

            //Main body drawing
            spriteBatch.Draw(t, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, t.Size() / 2, NPC.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(eye, NPC.Center - Main.screenPosition, null, drawColor, 0, eye.Size() / 2, NPC.scale, SpriteEffects.None, 0);
            return false;
        }
    }
    public class SeedOfFearHook : ModNPCPart
    {
        public SeedOfFearHook(int whoIam, Vector2 position, Vector2 velocity, int ownerNPC)
            : base(whoIam, position, velocity)
        {
            ai[0] = ownerNPC;
        }

        public override void SetDefaults()
        {
            width = height = 44;
        }
        public NPC Owner => Main.npc[(int)ai[0]];
        public float distance = 0;
        public float distanceMax = 0;
        public override void AI()
        {
            //Center += Owner.velocity;
            if (Owner.ai[0] == whoAmI)
            {
                if (ai[1] == 1) //Connecter Hook to Tile or Air after fly very long
                {
                    velocity = Vector2.Zero;
                }
                else //Fly Hook
                {
                    if (ai[2] == 0)
                    {
                        velocity = this.SafeDirectionTo(Main.player[Owner.target].Center);
                        distanceMax = this.Distance(Main.player[Owner.target].Center);
                        distance = 0;
                    }
                    else
                    {
                        Tile tile = Main.tile[(int)position.X / 16, (int)position.Y / 16];
                        if (tile.HasTile && tile.IsTileSolid() && distance >= distanceMax)
                        {
                            ai[1] = 1;
                            ai[2] = 0;
                        }
                        distance += velocity.Length();

                    }
                    ai[2]++;
                }
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(distance);
            writer.Write(distanceMax);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            distance = reader.ReadSingle();
            distanceMax = reader.ReadSingle();
        }
    }
}
