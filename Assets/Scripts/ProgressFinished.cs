using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

abstract public class ProgressFinished : IEcsRunSystem
{
    protected EcsFilter<Progress, ProgressViewRef, EvFinished> _filter;


    public void Run()
    {
        foreach (var index in _filter)
        {
           OnFinished(index);
        }
    }

    protected abstract void OnFinished(int index);
}


public class SysTestFinished : ProgressFinished
{
    protected override void OnFinished(int index)
    {
        Debug.Log( _filter.Get2(index).progImage.name +" Finished");
    }
}
