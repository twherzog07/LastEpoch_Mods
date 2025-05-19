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
        #region .ctor
        public LastEpochMod() : base() { }
        public LastEpochMod(System.IntPtr pointer) : base(pointer) { }
        #endregion

        #region Properties
        public static bool CanRun
        {
            get
            {
                return Scenes.IsGameScene() && Save_Manager.instance != null && !Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed();
            }
        }
        #endregion
    }
}
