using System.Collections.Generic;
using _project.Character;
using _project.Settings;
using _project.Enums;
using _project.Grid;
using _project.Car;
using UnityEngine;
using System;
using UniRx;
using System.Collections;

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
        public List<HumanClass> Humans = new List<HumanClass>();
        public ColorEnum MyColor;

        /*[HideInInspector]*/ public MyGrid ListeningGrid;

        public List<CharacterSc> HumanList = new List<CharacterSc>();
        CarCountainer _carAllParts;
        Transform _lineStartTr;
        CarPart _carPart;
        int repeattime;

        GridManager _gridManager;
        SettingSO _settingSO;

        void StartHumanDecrease(CarPart part)
        {
            if (_carPart != null && part == null)
                CarIsCamed(_carPart);
        }

        void Start()
        {
            _gridManager = GridManager.GridManagerScript;
            _settingSO = SettingSO.Instance;

            _gridManager.CurrentMouseSelectedCarPart.Subscribe(StartHumanDecrease);

            _lineStartTr = transform.GetChild(0);

            ListeningGrid = _gridManager.FindMyGridWithPos(new Vector2
                (
                    (float)Math.Truncate(transform.position.x),
                    (float)Math.Truncate(transform.position.z)
                )
            );

            ListeningGrid.OnCarPartChanged += CarIsCamedAssingCarPart;

            for (int i = 0; i < Humans.Count; i++)
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
        }

        void UpdateColorWithFirstHuman()
        {
            if(HumanList.Count > 0)
            {
                MyColor = HumanList[0].MyColor;

                var meshrenderere = GetComponent<MeshRenderer>();
                var mats = meshrenderere.materials;
                mats[1] = _settingSO.ColorMats[(int)MyColor - 1];
                meshrenderere.materials = mats;
            }
        }

        void CarIsCamedAssingCarPart(CarPart carPart)
        {
            _carPart = carPart;
        }

        void CarIsCamed(CarPart carPart)
        {
            if (carPart == null)
                return;

            _carAllParts = carPart.transform.parent.GetComponent<CarCountainer>();

            if (carPart.MyColor == MyColor)
            {
                Debug.LogWarning("Thats My Car");

                foreach (var item in _carAllParts.AllPart)
                    item.StopTheCar();

                repeattime = 0;
                CancelInvoke("RepeatingDecereaseHuman");
                InvokeRepeating("RepeatingDecereaseHuman", 0f, 1f);
            }
        }

        void ResetPassengers()
        {
            for (int i = 0; i < HumanList.Count; i++)
            {
                Vector3 spawnPos = _lineStartTr.position + (-_lineStartTr.forward * i * 0.5f);
                HumanList[i].MovePos(spawnPos);
            }
        }

        IEnumerator CarMoveUnlocke()
        {
            yield return new WaitForSeconds(0.5f);

            ResetPassengers();
            UpdateColorWithFirstHuman();

            foreach (var item in _carAllParts.AllPart)
                item.IsStopped = false;

            _carPart = null;
        }

        void RepeatingDecereaseHuman()
        {
            repeattime++;

            if (Humans[0].HowManyH <= repeattime)
            {
                StopCoroutine(CarMoveUnlocke());
                StartCoroutine(CarMoveUnlocke());

                Humans.Remove(Humans[0]);
                CancelInvoke("RepeatingDecereaseHuman");
            }

            CharacterSc CloneChar = HumanList[0];
            CloneChar.Jump(_carAllParts.SeatPos[repeattime - 1]);
            HumanList.Remove(CloneChar);

            print("HumanDecreased " + repeattime + "   " + Humans[0].HowManyH);
        }
    }
}