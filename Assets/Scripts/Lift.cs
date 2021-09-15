using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Lift<T> where T : ILifted
{
	[JsonPropertyAttribute]
	public IList<T> floorValues => floors.Values;
	public SortedList<int, T> floors = new SortedList<int, T>();
	public Action<T> onLifted;

	[JsonPropertyAttribute]
	int _currentFloor, maxFloor = 0;

	public int MaxFloor { get => maxFloor; }

	public Lift() { }
	public Lift(ref Action<int> onKeyUpdated)
	{
		onKeyUpdated += (key) => CheckFloors(key);
	}

	public Lift<T> Add(int key, T lifted)
	{
		floors.Add(key, lifted);

		lifted.floor = key;

		if (maxFloor < key) maxFloor = key;

		return this;
	}

	public void CheckFloors(int checkFloor)
	{
		if (_currentFloor <= checkFloor)
		{
			foreach (var item in AllNotLifted(checkFloor))
			{
				item.Value.isLifted = true;
				onLifted?.Invoke(item.Value);
			}

			_currentFloor = checkFloor;
		}
	}

	public IEnumerable<KeyValuePair<int, T>> AllNotLifted(int newFloor)
	{
		foreach (var item in floors)
		{
			if (!item.Value.isLifted && item.Key <= newFloor)
				yield return item;
		}
	}

	public IEnumerable<KeyValuePair<int, T>> AllLifted()
	{
		foreach (var item in floors)
		{
			if (item.Value.isLifted) yield return item;
		}
	}
}

public interface ILifted
{
	public bool isLifted { get; set; }
	public int floor { get; set; }
	void OnLifted();
	void OnDropped();
}

