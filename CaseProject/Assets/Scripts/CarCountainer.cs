using _project.Car;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public class CarCountainer : MonoBehaviour
{
    public Action PassengersFulled;

    public ReactiveProperty<int> AllPassengerValue;
    public int MaxPassengerVal;
    public Transform[] SeatPos;
    public CarPart[] AllPart;

    GameManager _gameManager;
    bool IsFull;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _gameManager.CarParents.Add(this);

        AllPassengerValue.Subscribe(PassengerIncreased);

        int seatCount = 0;
        foreach (var part in AllPart)
            seatCount += part.transform.GetChild(0).childCount;

        MaxPassengerVal = seatCount;
        SeatPos = new Transform[seatCount];

        int seatIndex = 0;
        foreach (var part in AllPart)
        {
            for (int i = 0; i < part.transform.GetChild(0).childCount; i++)
            {
                SeatPos[seatIndex] = part.transform.GetChild(0).GetChild(i);
                seatIndex++;
            }
        }
    }

    void PassengerIncreased(int val)
    {
        _gameManager.TextUpdate(1);
    }

    private void LateUpdate()
    {
        if (IsFull && !AllPart[0].IsStopped)
            return;

        if(AllPassengerValue.Value >= MaxPassengerVal)
        {
            PassengersFulled?.Invoke();
            CancelInvoke("DestroyInvoke");
            Invoke("DestroyInvoke", 1.25f);
            IsFull = true;
        }
    }

    void DestroyInvoke()
    {
        foreach (var item in AllPart.ToArray())
        {
            item.transform.DOKill(complete: false);
            Destroy(item.gameObject);
        }

        _gameManager.CarParents.Remove(this);
        Destroy(this);
    }
}