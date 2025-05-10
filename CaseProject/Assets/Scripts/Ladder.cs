using System.Collections.Generic;
using _project.Character;
using _project.Settings;
using _project.Enums;
using _project.Grid;
using _project.Car;
using UnityEngine;
using System;
using UniRx;
using System.Linq;

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
        public HumanClass[] Humans;
        public ColorEnum MyColor;

        /*[HideInInspector]*/ public MyGrid ListeningGrid;

        List<CharacterSc> HumanList = new List<CharacterSc>();
        Transform _lineStartTr;

        GridManager _gridManager;
        SettingSO _settingSO;

        void Start()
        {
            _gridManager = GridManager.GridManagerScript;
            _settingSO = SettingSO.Instance;

            _lineStartTr = transform.GetChild(0);

            ListeningGrid = _gridManager.FindMyGridWithPos(new Vector2
                (
                    (float)Math.Truncate(transform.position.x),
                    (float)Math.Truncate(transform.position.z)
                )
            );

            ListeningGrid.OnCarPartChanged += CarIsCamed;

            for (int i = 0; i < Humans.Length; i++)
            {
                for (int ii = 0; ii < Humans[i].HowManyH; ii++)
                {
                    GameObject humanClone = Instantiate(_settingSO.HumanPre, _lineStartTr);
                    CharacterSc HumanCharacter = humanClone.GetComponent<CharacterSc>();

                    Vector3 spawnPos = _lineStartTr.position + (-_lineStartTr.forward * HumanList.Count * 0.5f);
                    humanClone.transform.position = spawnPos;
                    HumanCharacter.ChangeColor(Humans[i].HColor);

                    HumanList.Add(HumanCharacter);
                }
            }

            UpdateColorWithFirstHuman();

            var meshrenderere = GetComponent<MeshRenderer>();
            var mats = meshrenderere.materials;
            mats[1] = _settingSO.ColorMats[(int)MyColor - 1];
            meshrenderere.materials = mats;
        }

        void UpdateColorWithFirstHuman()
        {
            MyColor = HumanList[0].MyColor;
        }

        void CarIsCamed(CarPart carPart)
        {
            if (carPart == null)
                return;

            CarCountainer _allCarParts = carPart.transform.parent.GetComponent<CarCountainer>();

            if (carPart.MyColor == MyColor)
            {
                //Debug.LogWarning("Thats My Car");

                foreach (var item in _allCarParts.AllPart)
                    item.StopTheCar();
            }
        }

        void RepeatingDecereaseHuman()
        {

        }
    }
}