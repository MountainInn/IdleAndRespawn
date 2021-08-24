using System.Collections.Generic;
using System;

public class Lift<T> where T : ILifted
{
    public SortedList<int, T> floors = new SortedList<int, T>();
    public Action<T> onLifted;

    int currentFloor, maxFloor = 0;

    public int MaxFloor {get=>maxFloor;}

    public Lift() {}

    public void Add(int key, T lifted)
    {
        floors.Add(key, lifted);

        if (maxFloor < key) maxFloor = key;
    }

    public void CheckFloors(int checkFloor)
    {
        if (currentFloor < checkFloor)
        {
            for (int i = 0; i < floors.Count; i++)
            {
                int key = floors.Keys[i];

                if (key > currentFloor)
                {
                    if (key > checkFloor) break;

                    floors[key].OnLifted();

                    onLifted?.Invoke(floors[key]);
                }
            }

            currentFloor = checkFloor;
        }
    }
}

public interface ILifted
{
    void OnLifted();
    void OnDropped();
}
