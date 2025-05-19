using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods
{
    /// <summary>
    /// Base class for all mods.
    /// </summary>
    /// <remarks>
    /// Inherits from <see cref="MonoBehaviour"/> to allow for Unity lifecycle methods.
    /// </remarks>
    public class LastEpochMod : MonoBehaviour
    {
        #region Properties
        public static bool CanRun
        {
            get
            {
                return Scenes.IsGameScene() && Save_Manager.instance != null && !Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed();
            }
        }
        #endregion

        #region .ctor
        public LastEpochMod() : base() { }
        public LastEpochMod(System.IntPtr pointer) : base(pointer) { }
        #endregion

        #region Functions
        /// <summary>
        /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="System.ConsoleColor"/>.
        /// </summary>
        /// <param name="c">The <see cref="System.Drawing.Color"/> to convert.</param>
        /// <returns>Returns the closest <see cref="System.ConsoleColor"/> for the specified <see cref="System.Drawing.Color"/>.</returns>
        /// <remarks>https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor</remarks>
        public static System.ConsoleColor ConsoleColorFromColor(System.Drawing.Color c)
        {
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit

            return (System.ConsoleColor)index;
        }
        #endregion
    }
}
