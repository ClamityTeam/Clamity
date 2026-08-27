using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Armor.FathomSwarmer;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class AradirMask : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
                ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.value = 0;
            Item.vanity = true;
            Item.Calamity().devItem = true;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) => true;

        public override void PreUpdateVanitySet(Player player)
        {
            player.Clamity().aradirVanity = true;
        }
    }
    public class AradirMaskTenticleFront : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            var modPlayer = drawPlayer.Clamity();
            //return false;
            return drawInfo.shadow == 0f && !drawPlayer.dead && modPlayer.aradirVanity;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Clamity/Content/Items/Armor/Vanity/AradirMask_Tenticle").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("Clamity/Content/Items/Armor/Vanity/AradirMask_Tenticle_End").Value;
            Player drawPlayer = drawInfo.drawPlayer;
            List<Vector2> offsets = new List<Vector2>() {
                new Vector2(-4, -9),
                new Vector2(-4, 3),
                new Vector2(8, 1),
            };
            int dyeShader = drawPlayer.dye?[1].dye ?? 0;
            int j = 0;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 perTenticleOffset = new Vector2(7, 0);
            for (int i = 4; i < 7; i++)
            {
                Vector2 offset = offsets[j];
                offset.X *= drawPlayer.direction;
                float drawX = (int)(drawInfo.Center.X - Main.screenPosition.X - (3 * drawPlayer.direction));
                float drawY = (int)(drawInfo.Center.Y - Main.screenPosition.Y - 4f);
                for (int k = 0; k < drawPlayer.Clamity().aradirTenticleRotation[i].Length; k++)
                {
                    float rot = drawPlayer.Clamity().aradirTenticleRotation[i][k];
                    Texture2D textureTemp = texture;
                    if (k == 3 && i != 0)
                        textureTemp = texture2;


                    DrawData tenticleDrawData = new DrawData(textureTemp, new Vector2(drawX, drawY) + offset, null, drawInfo.colorPants, rot, new Vector2(0, textureTemp.Height / 2f), new Vector2(1, 1), drawInfo.playerEffect, 0)
                    {
                        shader = dyeShader
                    };
                    drawInfo.DrawDataCache.Add(tenticleDrawData);

                    Vector2 v2 = perTenticleOffset.RotatedBy(rot);
                    drawX += v2.X;
                    drawY += v2.Y;

                }
                j++;
            }
        }
    }
    public class AradirMaskTenticleBack : PlayerDrawLayer
    {
        //public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head); //new BeforeParent(PlayerDrawLayers.HeadBack);
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeadBack);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            var modPlayer = drawPlayer.Clamity();
            return drawInfo.shadow == 0f && !drawPlayer.dead && modPlayer.aradirVanity;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Clamity/Content/Items/Armor/Vanity/AradirMask_Tenticle_Back").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("Clamity/Content/Items/Armor/Vanity/AradirMask_Tenticle_Back_End").Value;
            Player drawPlayer = drawInfo.drawPlayer;
            List<Vector2> offsets = new List<Vector2>() { 
                new Vector2(-4, -2),
                new Vector2(-4, -8),
                new Vector2(-4, 2),
                new Vector2(8, 0),
            };
            int dyeShader = drawPlayer.dye?[1].dye ?? 0;
            int j = 0;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 perTenticleOffset = new Vector2(7, 0);
            for (int i = 0; i < 4; i++)
            {
                float tenticleScale = 1;
                if (i == 0)
                    tenticleScale = 3;
                Vector2 offset = offsets[j];
                offset.X *= drawPlayer.direction;
                float drawX = (int)(drawInfo.Center.X - Main.screenPosition.X - (3 * drawPlayer.direction));
                float drawY = (int)(drawInfo.Center.Y - Main.screenPosition.Y - 4f); 
                for (int k = 0; k < drawPlayer.Clamity().aradirTenticleRotation[i].Length; k++) 
                {
                    float rot = drawPlayer.Clamity().aradirTenticleRotation[i][k];
                    Texture2D textureTemp = texture;
                    if (k == 3 && i != 0)
                        textureTemp = texture2;


                    DrawData tenticleDrawData = new DrawData(textureTemp, new Vector2(drawX, drawY) + offset, null, drawInfo.colorPants, rot, new Vector2(0, textureTemp.Height / 2f), new Vector2(1, 1 * tenticleScale), drawInfo.playerEffect, 0)
                    {
                        shader = dyeShader
                    };
                    drawInfo.DrawDataCache.Add(tenticleDrawData);

                    Vector2 v2 = perTenticleOffset.RotatedBy(rot);
                    drawX += v2.X;
                    drawY += v2.Y;
                    if (i == 0)
                        tenticleScale -= 1f;

                }
                j++;
            }
        }
    }
}
