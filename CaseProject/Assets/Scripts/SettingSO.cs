using _project.Enums;
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

    public enum CarTypeEnum
    {
        CarTwoPeople = 1,
        CarfourPeople = 2
    }
}

namespace _project.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
    public class SettingSO : ScriptableObject
    {
        public Material[] ColorMats;
        public GameObject HumanPre;

        public Material PickColor(ColorEnum colorenum) => ColorMats[(int)colorenum - 1];
        public int GetMaxPassengerWithType(CarTypeEnum type) => (int)type == 1 ? 2 : 4;

        public static SettingSO Instance => _instance != null ? _instance : LoadInstance();
        private static SettingSO _instance;

        private static SettingSO LoadInstance()
        {
            _instance = Resources.Load<SettingSO>("GameSettings");
            if (_instance == null)
                Debug.LogError("GameSettings is Not Here");
            return _instance;
        }
    }
}