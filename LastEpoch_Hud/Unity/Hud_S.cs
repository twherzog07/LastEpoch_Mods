using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Unity
{
    [RegisterTypeInIl2Cpp]
    public class Hud_S : MonoBehaviour
    {
        public Hud_S(System.IntPtr ptr) : base(ptr) { }
        public static Hud_S instance;

        public Button Character_btn;

        void Awake()
        {
            instance = this;
        }
        void Update()
        {
            
        }
    }
}
