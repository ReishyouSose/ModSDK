using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.THCompass.Compasses.CompassData;

namespace Assets.THCompass.Compasses
{
    public abstract class Compass
    {
        public abstract BossID BossID { get; }
        public abstract void RegisterLoot()
        {

        }
    }
}
