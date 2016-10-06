using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UappUI.AppCode.Touch
{
    public static class ValueMapHelper
    {
        /// <summary>
        /// Transform value from actual range to required range.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromMin">Mininam possible current value</param>
        /// <param name="fromMax">Maximal possible current value</param>
        /// <param name="toMin">Mininam possible required value</param>
        /// <param name="toMax">Maximal possible required value</param>
        /// <returns>Transfromed value</returns>
        public static double Map(this double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }
    }
}
