using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

public class ProgressFilling : IEcsRunSystem
{
    EcsFilter<Progress, ProgressViewRef> _filter;


    public void Run()
    {
        foreach (var index in _filter)
        {
            ref var prog = ref _filter.Get1(index);


            if ( (prog.val += Time.deltaTime) > 1f )
            {
                prog.val -= 1f;

                _filter.GetEntity(index).Get<EvFinished>();
            }


            ref var view = ref _filter.Get2(index);

            view.progImage.fillAmount = prog.val;
        }
    }

}
