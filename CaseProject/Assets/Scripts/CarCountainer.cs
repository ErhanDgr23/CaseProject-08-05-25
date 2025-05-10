using _project.Car;
using UnityEngine;

public class CarCountainer : MonoBehaviour
{
    public int AllPassengerValue;
    public Transform[] SeatPos;
    public CarPart[] AllPart;

    private void Start()
    {
        int seatCount = 0;
        foreach (var part in AllPart)
            seatCount += part.transform.childCount;

        SeatPos = new Transform[seatCount];

        int seatIndex = 0;
        foreach (var part in AllPart)
        {
            for (int i = 0; i < part.transform.childCount; i++)
            {
                SeatPos[seatIndex] = part.transform.GetChild(i);
                seatIndex++;
            }
        }
    }
}