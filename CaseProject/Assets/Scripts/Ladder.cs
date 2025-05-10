using _project.Settings;
using _project.Enums;
using _project.Grid;
using UnityEngine;
using System;

namespace _project.Ladder
{
    [Serializable]
    public class HumanClass
    {
        public ColorEnum HColor;
        public int HowManyH;
    }

    public class Ladder : MonoBehaviour
    {
        public MyGrid ListeningGrid;
        public HumanClass[] Humans;
        public ColorEnum MyColor;

        GridManager _gridManager;
        SettingSO _settingSO;

        void Start()
        {
            _gridManager = GridManager.GridManagerScript;
            _settingSO = SettingSO.Instance;

            ListeningGrid = _gridManager.FindMyGridWithPos(new Vector2
                (
                    (float)Math.Truncate(transform.position.x),
                    (float)Math.Truncate(transform.position.z)
                )
            );

            var meshrenderere = GetComponent<MeshRenderer>();
            var mats = meshrenderere.materials;
            mats[1] = _settingSO.ColorMats[(int)MyColor - 1];
            meshrenderere.materials = mats;

            for (int i = 0; i < Humans.Length; i++)
            {
                for (int ii = 0; ii < Humans[i].HowManyH; ii++)
                {

                }
            }
        }
    }
}