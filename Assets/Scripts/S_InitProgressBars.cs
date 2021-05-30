using Leopotam.Ecs;
using UnityEngine;

public class SysInitProgressBars : IEcsInitSystem
{
    EcsWorld _world;

    Prefabs prefabs;

    public void Init()
    {
        Transform canvas = GameObject.Find("Canvas").transform;

        var bar = _world.NewEntity();


        ref var prog = ref bar.Get<Progress>();

        prog.val = 0;


        ref var view = ref bar.Get<ProgressViewRef>();

        view.progImage = Object.Instantiate( prefabs.progImage, new Vector3(200, 200, 0), Quaternion.identity, canvas );

        view.progImage.fillAmount = 0;


    }


}
