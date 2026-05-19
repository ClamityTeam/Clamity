using CalamityMod.Items;
using CalamityMod.Items.Materials;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Whips
{
    public class DryadsKarma : BaseWhipItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            // This method quickly sets the whip's properties.
            // Mouse over to see its parameters.
            Item.DefaultToWhip(ModContent.ProjectileType<DryadsKarmaProj>(), 100, 5, 12);
            Item.shootSpeed = 10;
            Item.useTime = Item.useAnimation = 27;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
        }

        // Makes the whip receive melee prefixes
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SwordWhip)
                .AddIngredient(ModContent.ItemType<LivingShard>(), 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class DryadsKarmaProj : BaseWhipProjectile
    {
        public override string ExtraPath => "DryadsKarma";
        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 5;
            Projectile.WhipSettings.RangeMultiplier = .8f;
            fishingLineColor = Color.Lime;
            segmentRotation = -MathF.PI / 2;

            dustAmount = 2;
            swingDust = DustID.Terra;

            tagDebuff = ModContent.BuffType<TerraWhipDebuff>();
        }
        /*public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].AddBuff(BuffID.SwordWhipPlayerBuff, 25*60);
        }*/

        /*public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.50f)
            {

                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.position, Vector2.Zero, ModContent.ProjectileType<WildfireBloomFire>(), 300, 0, Projectile.owner);
                Main.projectile[proj].minion = true;
                Main.projectile[proj].DamageType = DamageClass.Summon;

                proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.position, Vector2.Zero, ModContent.ProjectileType<TerratomereSwordBeam>(), 360, 0, Projectile.owner);
                Main.projectile[proj].minion = true;
                Main.projectile[proj].DamageType = DamageClass.Summon;
            }
        }*/

    }
    public class TerraWhipDebuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "Buffs.Whips";
        public override string Texture => Clamity.buffPath;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<GlobalTagDebuffs>().activeTag = Type;
            npc.GetGlobalNPC<GlobalTagDebuffs>().terrawhipIndex = buffIndex;
        }
    }
}
