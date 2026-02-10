using CalamityMod.MainMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Menu
{
    public class ClamityMenu : CalamityMainMenu
    {
        public override string DisplayName => "Clamity Style";

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Clamity/Content/Menu/Logo");

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            /*
             * I refactored this code to match the Vanilla Calamity Mod Music backgrounds, as the previous menu theme was somewhat unprofessional. 
             * I have left comments for each section.
             * This should help with the overall view of the mod.
             * - Akira
            */ 

            // === Background ===
            Texture2D texture = ModContent.Request<Texture2D>("Clamity/Content/Menu/MenuAlt").Value;

            Vector2 drawOffset = Vector2.Zero;
            float xScale = (float)Main.screenWidth / texture.Width;
            float yScale = (float)Main.screenHeight / texture.Height;
            float scale = xScale;

            if (xScale != yScale)
            {
                if (yScale > xScale)
                {
                    scale = yScale;
                    drawOffset.X -= (texture.Width * scale - Main.screenWidth) * 0.5f;
                }
                else
                {
                    drawOffset.Y -= (texture.Height * scale - Main.screenHeight) * 0.5f;
                }
            }

            spriteBatch.Draw(texture, drawOffset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // === Cinders ===
            static Color selectCinderColor()
            {
                if (Main.rand.NextBool(3))
                    return Color.Lerp(Color.DarkGray, Color.LightGray, Main.rand.NextFloat());

                return Color.Lerp(Color.Blue, Color.Cyan, Main.rand.NextFloat(0.9f));
            }

            for (int i = 0; i < 5; i++)
            {
                if (Main.rand.NextBool(4))
                {
                    int lifetime = Main.rand.Next(200, 300);
                    float depth = Main.rand.NextFloat(1.8f, 5f);
                    Vector2 startingPosition = new Vector2(
                        Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f),
                        Main.screenHeight * 1.05f
                    );

                    Color color = selectCinderColor();
                    Cinders.Add(new Cinder(lifetime, Cinders.Count, depth, color, startingPosition, new Vector2(0f, -2f)));
                }
            }

            for (int j = 0; j < Cinders.Count; j++)
            {
                Cinders[j].Scale = Utils.GetLerpValue(Cinders[j].Lifetime, Cinders[j].Lifetime / 3, Cinders[j].Time, clamped: true);
                Cinders[j].Scale *= MathHelper.Lerp(0.6f, 0.9f, Cinders[j].IdentityIndex % 6f / 6f);

                if (Cinders[j].IdentityIndex % 13 == 12)
                    Cinders[j].Scale *= 2f;

                Cinders[j].Velocity *= 1.01f;
                Cinders[j].Time++;
                Cinders[j].Center += Cinders[j].Velocity;
            }

            Cinders.RemoveAll(c => c.Time >= c.Lifetime);

            Texture2D cinderTexture = ModContent.Request<Texture2D>("CalamityMod/Skies/CalamitasCinder").Value;

            for (int k = 0; k < Cinders.Count; k++)
            {
                Vector2 center = Cinders[k].Center;
                spriteBatch.Draw(
                    cinderTexture,
                    center,
                    null,
                    Cinders[k].DrawColor,
                    0f,
                    cinderTexture.Size() * 0.5f,
                    Cinders[k].Scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // === Logo / time and world-seed behavior ===
            drawColor = Color.White;
            Main.time = 27000;
            Main.dayTime = true;

            float drawScale = WorldGen.drunkWorldGen ? logoScale : 1f;
            Vector2 position = new Vector2(Main.screenWidth / 2f, 100f);

            // === Switch state & draw orbiting Logo_Back ring + logo ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            Texture2D logoBackTex = ModContent.Request<Texture2D>("Clamity/Content/Menu/Logo_Back").Value;

            // Orbiting logo ring
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 8 * i) *
                                 (1.01f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f / 3f)) * 2f;

                spriteBatch.Draw(
                    logoBackTex,
                    position + offset,
                    null,
                    Color.Lerp(Color.LightBlue, Color.Cyan, MathF.Sin(Main.GlobalTimeWrappedHourly) / 2f + 0.5f),
                    0f,
                    Logo.Value.Size() * 0.5f,
                    drawScale,
                    SpriteEffects.None,
                    0f
                );
            }

            // Main logo
            spriteBatch.Draw(
                Logo.Value,
                position,
                null,
                drawColor,
                0f,
                Logo.Value.Size() * 0.5f,
                drawScale,
                SpriteEffects.None,
                0f
            );

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            return false;
        }
        //public override int Music => MusicLoader.GetMusicSlot("Clamity/Sounds/Music/Title");
        public override int Music => Clamity.mod.GetMusicFromMusicMod("Title") ?? MusicID.Title;
    }
}
