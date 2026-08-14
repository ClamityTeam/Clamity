using CalamityMod;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Projectiles
{
    public class WallOfBronzeClawProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public ref float ClawGun => ref Projectile.ai[0];
        public ref float AttackType => ref Projectile.ai[1];
        public ref float HasBoulder => ref Projectile.ai[2];
        /*public Vector2 StartVelocity
        {
            get => new Vector2(Projectile.ai[1], Projectile.ai[2]);
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }*/
        /// <summary>
        /// 0 - start of shoot, 
        /// 1 - fly with boulder, 
        /// 2 - circular throw (type 2 only), 
        /// 3 - return claw back
        /// </summary>
        public ref float State => ref Projectile.Clamity().extraAI[1];
        public ref float Timer => ref Projectile.Clamity().extraAI[2];
        public Terraria.NPC GetClawGun => Main.npc[(int)ClawGun];

        public static float ClawVelocity => 10;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        public override void AI()
        {
            if (GetClawGun == null) Projectile.Kill();
            if (!GetClawGun.active) Projectile.Kill();

            Projectile.timeLeft = 2;

            Projectile.rotation = (GetClawGun.Center - Projectile.Center).ToRotation();


            switch (State) 
            {
                case 0:
                    if (Timer > 120)
                    {
                        State = 1;
                        Timer = 0;
                        HasBoulder = 1;
                    }
                    break;
                case 1:
                    Projectile.velocity = Vector2.Normalize(GetClawGun.Center - Projectile.position) * ClawVelocity;

                    if (AttackType == 1)
                    {
                        if (Timer > 60 && AttackType == 1)
                        {
                            Texture2D claw = ModContent.Request<Texture2D>(Texture).Value;
                            int boulder = ModContent.ProjectileType<GiantBoulder>();
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(claw.Width).RotatedBy(Projectile.rotation), -Projectile.velocity / 3, boulder, Projectile.damage, Projectile.knockBack, Projectile.owner, ClawGun);

                            State = 3;
                            Timer = 0;
                            HasBoulder = 0;
                        }
                    }
                    else if (AttackType == 2)
                    {
                        if (GetClawGun.Center.Distance(Projectile.Center) < 30)
                        {
                            State = 2;
                            Timer = 0;
                            HasBoulder = 0;
                        }
                    }

                    break;
                case 2:
                    int time = 60;

                    Projectile.rotation += CalamityUtils.CircInEasing(MathHelper.Clamp(Timer, 0, time) / time, 1);
                    Projectile.Center = Vector2.Zero;

                    if (Timer == time / 4 * 3)
                    {
                        Texture2D claw = ModContent.Request<Texture2D>(Texture).Value;
                        int boulder = ModContent.ProjectileType<GiantBoulder>();
                        Vector2 center = Projectile.Center + new Vector2(claw.Width).RotatedBy(Projectile.rotation);
                        Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), center, (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 10, boulder, Projectile.damage, Projectile.knockBack, Projectile.owner, ClawGun);


                        Projectile.Kill();
                    }
                    break;
                case 3:
                    Projectile.velocity = Vector2.Normalize(GetClawGun.Center - Projectile.position) * ClawVelocity;
                    if (GetClawGun.Center.Distance(Projectile.Center) < 30)
                        Projectile.Kill();

                    break;
            }
            Timer++;



            //Main.NewText(GetClawGun.Center);
            //Main.player[Main.myPlayer].Center = Projectile.Center;

            /*if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft <= 550)
                Projectile.tileCollide = true;
            if (Projectile.timeLeft == 100)
            {
                StartLength = (GetClawGun.Center - Projectile.position).Length();
            }
            if (Projectile.timeLeft < 100)
            {
                Projectile.velocity = Vector2.Normalize(GetClawGun.Center - Projectile.position) / 10f * StartLength;
                Projectile.tileCollide = false;
                if (Collision.CheckAABBvAABBCollision(Projectile.position, Projectile.Hitbox.BottomRight(), GetClawGun.position, GetClawGun.Hitbox.BottomRight()))
                    Projectile.Kill();
            }*/
            //Terraria.Collision.CheckAABBvAABBCollision
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft >= 100)
            {
                Projectile.velocity = Vector2.Zero;
                //Projectile.ai[1] = 1;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D boulder = ModContent.Request<Texture2D>("Clamity/Content/Bosses/WoB/Projectiles/GiantBoulder").Value;
            Texture2D claw = ModContent.Request<Texture2D>(Texture).Value;

            //Boulder
            if (HasBoulder == 1)
                Main.spriteBatch.Draw(boulder, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, boulder.Size() + new Vector2(boulder.Width - claw.Width / 2, 0), 1, SpriteEffects.None, 0);

            //Chain
            Vector2 mountedCenter = GetClawGun.Center;
            Texture2D value = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Vector2 center = Projectile.Center;
            Rectangle? sourceRectangle = null;
            Vector2 origin = new Vector2(value.Width * 0.5f, value.Height * 0.5f);
            float num = value.Height;
            Vector2 vector = mountedCenter - center;
            float rotation = (float)Math.Atan2(vector.Y, vector.X) - MathF.PI / 2f;
            bool flag = true;
            if (float.IsNaN(center.X) && float.IsNaN(center.Y))
            {
                flag = false;
            }

            if (float.IsNaN(vector.X) && float.IsNaN(vector.Y))
            {
                flag = false;
            }

            while (flag)
            {
                if (vector.Length() < num + 1f)
                {
                    flag = false;
                    continue;
                }

                Vector2 vector2 = vector;
                vector2.Normalize();
                center += vector2 * num;
                vector = mountedCenter - center;
                Color color = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16f));
                Main.spriteBatch.Draw(value, center - Main.screenPosition, sourceRectangle, color, rotation, origin, 1f, SpriteEffects.None, 0f);
            }

            //Claw Itself
            Main.spriteBatch.Draw(claw, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(claw.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0);

            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.getGoodWorld)
                target.AddBuff(BuffID.Frozen, 180);
        }
    }
}
