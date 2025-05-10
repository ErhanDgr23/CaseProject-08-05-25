using UnityEngine;

namespace _project.Enums
{
    public enum ColorEnum
    {
        Red = 1,
        Blue = 2,
        Green = 3,
        Orange = 4
    }
}

namespace _project.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
    public class SettingSO : ScriptableObject
    {
        public Material[] ColorMats;
        public GameObject HumanPre;

        public static SettingSO Instance => _instance != null ? _instance : LoadInstance();
        private static SettingSO _instance;

        private static SettingSO LoadInstance()
        {
            _instance = Resources.Load<SettingSO>("GameSettings");
            if (_instance == null)
                Debug.LogError("GameSettings asset not found in Resources folder!");
            return _instance;
        }
    }
}