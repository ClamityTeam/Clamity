using CalamityMod;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Whips
{
    /// <summary>
    /// Base class for a whip that handles drawing, AI, and onHit. To make a simple whip, you only need to specify stats in <see cref="SetWhipStats"/>
    /// </summary>
    public abstract class BaseWhipProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon.Whips";

        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.>
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        //whip stats
        public Color fishingLineColor = Color.White;
        public Color lightingColor = Color.Transparent;
        public Color? drawColor = null;

        public int swingDust = -1;
        public int dustAmount = 0;
        public int altSegmentMod = 2;

        public readonly SoundStyle whipCrackSound = SoundID.Item153;

        public Texture2D whipSegment;
        public Texture2D whipSegment2 = null;
        public Texture2D whipTip;

        public int tagDebuff = -1;
        public int tagDuration = 240;
        public float multihitModifier = .8f;

        public float segmentRotation = 0;
        public bool flipped = false;
        internal bool runOnce = true;

        internal ref float Timer => ref Projectile.ai[0];

        //ADDED BY OBAMA - looks like shit rn do not use 
        //kinda peak now??!

        internal List<Vector2> TrailPoints = [];

        internal ref float TrailPointTimer => ref Projectile.localAI[2];

        public virtual bool DrawTrailAtTip { get; } = false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(fishingLineColor.R);
            writer.Write(fishingLineColor.G);
            writer.Write(fishingLineColor.B);

            writer.Write(swingDust);
            writer.Write(dustAmount);
            writer.Write(altSegmentMod);

            writer.Write(tagDebuff);
            writer.Write(tagDuration);
            writer.Write(multihitModifier);

            writer.Write(segmentRotation);
            writer.Write(flipped);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            fishingLineColor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

            swingDust = reader.ReadInt32();
            dustAmount = reader.ReadInt32();
            altSegmentMod = reader.ReadInt32();

            tagDebuff = reader.ReadInt32();
            tagDuration = reader.ReadInt32();
            multihitModifier = reader.ReadSingle();

            segmentRotation = reader.ReadSingle();
            flipped = reader.ReadBoolean();
        }

        public sealed override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.width = 20;
            Projectile.height = 20;

            whipSegment = ModContent.Request<Texture2D>(Texture + "_Segment").Value;
            whipTip = ModContent.Request<Texture2D>(Texture + "_Tip").Value;

            SetWhipStats();
        }

        /// <summary>
        /// Function is use to control custom whip stats, called in the parent class's set defaults
        /// </summary>
        public virtual void SetWhipStats()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.WhipSettings.Segments = 30;
            Projectile.WhipSettings.RangeMultiplier = 1f;
        }

        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            WhipOnHit(target, hit, damageDone);

            if (target.isLikeATownNPC || target.type == NPCID.TargetDummy || target.CountsAsACritter)
                return;

            if (tagDebuff != -1)
            {
                target.buffImmune[(int)tagDebuff] = false;
                target.AddBuff((int)tagDebuff, tagDuration);
            }

            Projectile.damage = (int)(Projectile.damage * .8f);

            if (Projectile.damage < 1)
                Projectile.damage = 1;

            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }

        /// <summary>
        /// Applies tag buff if there is one, applies multihit penalty, and focuses minions on target. 
        /// Called in OnHitNPC
        /// </summary>
        /// <param name="target"></param>
        public virtual void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        /// <summary>
        /// Runs whip AI similar to example mod, but the center is now on the whip tip. Called in AI
        /// </summary>
        public virtual void WhipAIMotion()
        {
            Player owner = Main.player[Projectile.owner];
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

            if (runOnce)
            {
                Projectile.WhipSettings.Segments = (int)((owner.whipRangeMultiplier + 1) * Projectile.WhipSettings.Segments);
                runOnce = false;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

            List<Vector2> lerpPoints = Projectile.WhipPointsForCollision;
            Projectile.FillWhipControlPoints(Projectile, lerpPoints);

            Projectile.Center = Vector2.Lerp(Projectile.Center, lerpPoints[lerpPoints.Count - 1], 1);

            // Vanilla uses Vector2.Dot(Projectile.velocity, Vector2.UnitX) here. Dot Product returns the difference between two vectors, 0 meaning they are perpendicular.
            // However, the use of UnitX basically turns it into a more complicated way of checking if the projectile's velocity is above or equal to zero on the X axis.
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            Projectile.spriteDirection *= (flipped ? -1 : 1);
            Timer++;

            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
        }

        /// <summary>
        /// Plays sound and runs dust, all the parameters should be set in whip stats, though you can override them. 
        /// Called in AI
        /// </summary>
        /// <param name="lightingCol"></param>
        /// <param name="dustID"></param>
        /// <param name="dustNum"></param>
        /// <param name="sound"></param>
        public virtual void WhipSFX(Color lightingCol, int dustID, int dustNum, SoundStyle? sound)
        {
            Player owner = Main.player[Projectile.owner];
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            //Main.NewText(lightingCol);

            owner.heldProj = Projectile.whoAmI;
            Vector2 tip = GetTipPosition();

            if (Timer == swingTime / 2 && sound != null)
            {
                // Plays a whipcrack sound at the tip of the whip.
                SoundEngine.PlaySound(sound, tip);
            }

            if ((Timer >= swingTime * .5f))
            {
                if (dustID != -1)
                {
                    for (int i = 0; i < dustNum; i++)
                    {
                        Dust.NewDust(tip, 2, 2, (int)dustID, 0, 0, Scale: .5f);
                    }
                }

                if (lightingCol != Color.Transparent)
                {
                    Lighting.AddLight(tip, lightingCol.R / 255f, lightingCol.G / 255f, lightingCol.B / 255f);
                }
            }
        }

        internal Vector2 GetTipPosition()
        {
            List<Vector2> list = [];

            Projectile.FillWhipControlPoints(Projectile, list);
            return list[^2];
        }

        public sealed override void AI()
        {
            WhipAIMotion();
            WhipSFX(lightingColor, swingDust, dustAmount, whipCrackSound);

            //handle trail points if needed
            if (DrawTrailAtTip)
            {
                if (Timer > 10)
                {
                    if (++TrailPointTimer >= 1)
                    {
                        Vector2 currentTipPosition = GetTipPosition();

                        TrailPoints.Add(currentTipPosition + Projectile.rotation.ToRotationVector2()/* + Main.rand.NextVector2CircularEdge(100, 100)*/);

                        if (TrailPoints.Count > 50)
                            TrailPoints.RemoveAt(0);

                        TrailPointTimer = 0;
                    }

                    //smoothen the trail (interpolate positions to prevent jaggedness near the tip of the arc)
                    for (int i = 1; i < TrailPoints.Count - 1; i++)
                        TrailPoints[i] = Vector2.Lerp(TrailPoints[i], TrailPoints[i + 1], 0.15f);
                }
            }
        }

        /// <summary>
        /// Draws a fishing line between a line of points
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lineCol"></param>
        internal static void DrawFishingLineBetweenPoints(List<Vector2> list, Color lineCol, bool useLighCol = true)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 2; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = lineCol;

                if (useLighCol)
                    color = Lighting.GetColor(element.ToTileCoordinates(), lineCol);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        public virtual float TrailWidth(float f, Vector2 vertexPos) => 10f * f;

        public Color? trailLineColorOverride = null;

        public virtual Color TrailColor(float f, Vector2 vertexPos) => (trailLineColorOverride is null ? fishingLineColor : trailLineColorOverride.Value) * f;

        public virtual void DrawTrail()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            MiscShaderData shader = GameShaders.Misc["CalamityMod:TrailStreak"];
            shader.SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/BasicTrail"));

            PrimitiveSettings settings = new(TrailWidth, TrailColor, null, true, false, shader); //TrailStreak

            PrimitiveRenderer.RenderTrail(TrailPoints, settings, 64);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (DrawTrailAtTip)
                DrawTrail();

            DrawWhip(fishingLineColor);

            return false;
        }

        /// <summary>
        /// Draws whip based on example mod, override if you want custom. 
        /// Called in PreDraw
        /// </summary>
        /// <param name="lineColor"> What color the fishing line is</param>
        /// <returns></returns>
        internal bool DrawWhip(Color lineColor)
        {
            //Gets every segment of the whip
            List<Vector2> list = [];
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawFishingLineBetweenPoints(list, lineColor);

            SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);

            //Load projectiles using file paths
            var texture = TextureAssets.Projectile[Type].Value;

            //Sets the frame which will be displayed
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = sourceRectangle.Size() / 2f;

            //TODO: currently the whip handle has a spritesheet of identicle sprites as otherwise it would animate
            //when it should not. At somepoint, this should be correct so that segment 0 (the handle) does not animate.

            Vector2 pos = list[0];

            //Repeats for each whip point
            for (int i = 0; i < list.Count - 1; i++)
            {
                float scale = 1;

                Texture2D textre = null; float scle = 1f; Vector2 offset = Vector2.Zero;

                if (UnstandartWhipTextureDrawing(i, list.Count - 1, ref textre, ref scle, ref offset))
                {
                    texture = textre;
                    scale = scle;
                    sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                    origin = sourceRectangle.Size() / 2f;
                }

                else
                {
                    //Tip of the whip
                    if (i == list.Count - 2)
                    {
                        //Sets image to tip texture
                        texture = whipTip;

                        //Moves the frame with the animation
                        sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                        origin = sourceRectangle.Size() / 2f;

                        // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                        Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                        float t = Timer / timeToFlyOut;
                        scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.2f, 0.3f, t, true));
                    }

                    else if (whipSegment2 != null && i % altSegmentMod == 0)
                    {
                        texture = whipSegment2;
                        //sets the frame accordingly

                        sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                        origin = sourceRectangle.Size() / 2f;
                    }

                    else
                    {
                        //Tip of the whip
                        if (i == list.Count - 2)
                        {
                            //Sets image to tip texture
                            texture = whipTip;

                            //Moves the frame with the animation
                            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                            origin = sourceRectangle.Size() / 2f;

                            // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                            float t = Timer / timeToFlyOut;
                            scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                        }

                        else if (whipSegment2 != null && i % altSegmentMod == 0)
                        {
                            texture = whipSegment2;
                            //sets the frame accordingly
                            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                            origin = sourceRectangle.Size() / 2f;
                        }

                        else if (i > 0)
                        {
                            //Sets image to segment texture
                            texture = whipSegment;
                            //sets the frame accordingly
                            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                            origin = sourceRectangle.Size() / 2f;
                        }
                    }
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                //For this projectile, rotation along the whip is disabled for aesthetic reasons
                //Normally it would be more similiar to the rotation in the if statment
                float rotation = diff.ToRotation() + segmentRotation;
                Color color = drawColor ?? Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos + offset - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, flip, 0);
                pos += diff;
            }

            return false;
        }

        public virtual bool UnstandartWhipTextureDrawing(int i, int maxLeight, ref Texture2D texture, ref float scale, ref Vector2 offset)
        {
            return false;
        }
    }
}