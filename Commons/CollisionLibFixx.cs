using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Commons
{
    //Idk how he work and how he fix bug between, Cataclysm and Impact Library
    //Dow't Work :skull:
    /*public class CollisionFixx : ModSystem
    {
        private static bool _hookApplied = false;

        public override void Load()
        {
            try
            {
                Terraria.IL_Projectile.AI_007_GrapplingHooks += ILFix;
                _hookApplied = true;
            }
            catch { }
        }

        private void ILFix(ILContext context) { }

        public override void Unload()
        {
            try
            {
                if (_hookApplied)
                {
                    Terraria.IL_Projectile.AI_007_GrapplingHooks -= ILFix;
                    _hookApplied = false;
                }
            }
            catch { }
        }
    }*/
}