using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

public class SysProgressFilling : IEcsRunSystem
{
    EcsFilter<Progress, ProgBarRef> _filter;


    public void Run()
    {
        foreach (var index in _filter)
        {
            ref var prog = ref _filter.Get1(index);


            if ( (prog.current += Time.deltaTime) >= prog.max )
            {
                prog.current -= prog.max;

                _filter.GetEntity(index).Get<EvFinished>();
            }


            ref var view = ref _filter.Get2(index);

            view.progImage.fillAmount = prog.current / prog.max;
        }
    }

}
