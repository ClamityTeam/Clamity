using System;
using CalamityMod.Systems.Mechanic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Buffs.DamageOverTime;
using Clamity.Content.Bosses.Pyrogen.NPCs;

namespace Clamity.Content.Bosses.Pyrogen.Projectiles
{
    public class PyrogenBox : ModProjectile
    {
        private const float BoxHalfSize = PyrogenBoss.ArenaRadius;
        private const float BorderThickness = 96f;
        private const float OuterExpansion = 1200f;

        public ArenaWallSystem.Box ArenaBox;

        public override string Texture => ModContent.GetInstance<SmallFireball>().Texture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.netImportant = true;
            Projectile.hide = true;
            CooldownSlot = -1;
        }

        private NPC FindPyrogen()
        {
            int index = (int)Projectile.ai[1];

            if (index >= 0 &&
                index < Main.maxNPCs &&
                Main.npc[index].active &&
                Main.npc[index].type == ModContent.NPCType<PyrogenBoss>())
            {
                if (Main.npc[index].ai[0] >= 2)
                    return Main.npc[index];
            }

            return null;
        }

        private void UpdateArena(ArenaWallSystem.Box box)
        {
            for (int i = 0; i < box.Size.Y / 300f; i++)
            {
                Vector2 p = Vector2.Lerp(box.BottomRight, box.TopRight, Main.rand.NextFloat());
                Dust.NewDustPerfect(p, DustID.Torch, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0f, 5f), 0, box.borderColor, Main.rand.NextFloat(0.8f, 1.4f)).noGravity = true;

                p = Vector2.Lerp(box.TopLeft, box.BottomLeft, Main.rand.NextFloat());
                Dust.NewDustPerfect(p, DustID.Torch, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0f, 5f), 0, box.borderColor, Main.rand.NextFloat(0.8f, 1.4f)).noGravity = true;
            }

            for (int i = 0; i < box.Size.X / 300f; i++)
            {
                Vector2 p = Vector2.Lerp(box.TopLeft, box.TopRight, Main.rand.NextFloat());
                Dust.NewDustPerfect(p, DustID.Torch, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0f, 5f), 0, box.borderColor, Main.rand.NextFloat(0.8f, 1.4f)).noGravity = true;

                p = Vector2.Lerp(box.BottomRight, box.BottomLeft, Main.rand.NextFloat());
                Dust.NewDustPerfect(p, DustID.Torch, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0f, 5f), 0, box.borderColor, Main.rand.NextFloat(0.8f, 1.4f)).noGravity = true;
            }
        }
        private void DrawArena(ArenaWallSystem.Box box)
        {
            float outerDistance = box.borderThickness + OuterExpansion;

            // Fill the whole gap between inner box and outer box.
            box.DrawBoxWithOffset(outerDistance * 0.5f, outerDistance, Color.Black * 0.65f);

            // Inner arena edge.
            box.DrawBoxWithOffset(4f, 8f, box.borderColor);

            const float afterimageCount = 6f;

            for (float i = Main.GlobalTimeWrappedHourly % 1f; i < afterimageCount; i++)
            {
                float completion = i / afterimageCount;
                float offset = MathHelper.Lerp(4f, outerDistance, completion);
                float opacity = 1f - completion;

                box.DrawBoxWithOffset(offset, 4f, box.borderColor * opacity);
            }

            // Outer edge.
            box.DrawBoxWithOffset(outerDistance, 8f, box.borderColor * 0.9f);
        }

        public override bool? CanDamage() => Projectile.alpha <= 0;

        public override bool CanHitPlayer(Player target)
        {
            return target.whoAmI == (int)Projectile.ai[2];
        }

        public override void AI()
        {
            NPC pyrogen = FindPyrogen();

            if (pyrogen is null)
            {
                Projectile.alpha += 6;

                if (ArenaBox is not null)
                    ArenaBox.boxDimensions += new Vector4(64f);

                if (Projectile.alpha >= 255)
                    Projectile.Kill();

                Projectile.timeLeft = 2;
                return;
            }

            Projectile.alpha -= 4;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Projectile.Center = Vector2.Lerp(Projectile.Center, pyrogen.Center, 0.08f);
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;

            Projectile.ai[2] = pyrogen.target;

            if (ArenaBox is null)
            {
                ArenaBox = new ArenaWallSystem.Box()
                {
                    position = Projectile.Center,
                    boxDimensions = new Vector4(BoxHalfSize),
                    borderThickness = BorderThickness,
                    borderColor = Color.LightCyan,
                    RemovalCondition = () =>
                    {
                        NPC npc = FindPyrogen();
                        return npc is null || !Projectile.active;
                    },
                    UpdateBox = UpdateArena,
                    DrawBox = DrawArena,
                    DespawnAction = box =>
                    {
                        box.boxDimensions += new Vector4(64f);
                        return box.Size.X > BoxHalfSize * 4f;
                    }
                };

                ArenaWallSystem.ActiveBoxes.Add(ArenaBox);
            }

            ArenaBox.position = Projectile.Center;
            ArenaBox.NewDimensions = Vector4.Lerp(ArenaBox.boxDimensions, new Vector4(BoxHalfSize),  0.08f);
            ArenaBox.borderColor = Color.Lerp(ArenaBox.borderColor, Color.Lerp(Color.PaleVioletRed, Color.Firebrick, (MathF.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) * 0.5f), 0.08f);
        }

        public override void PostAI()
        {
            Projectile.hide = false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor * Projectile.Opacity * ((int)Projectile.ai[2] == Main.myPlayer ? 1f : 0.15f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override bool? CanCutTiles() => false;
    }
}
