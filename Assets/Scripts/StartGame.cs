using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;
using Leopotam.Ecs.UnityIntegration;

public class StartGame : MonoBehaviour
{
    public Prefabs prefabs;

    [SerializeField, HideInInspector] EcsSystems _systems;


    void Start()
    {
        var world = new EcsWorld ();

#if UNITY_EDITOR
        EcsWorldObserver.Create (world);
#endif

        _systems = new EcsSystems(world)

            .Add(new SysInitProgressBars())


            .Add(new ProgressFilling())
            .Add(new SysTestFinished())


            .OneFrame<EvFinished>()


            .Inject(prefabs)
            ;

        _systems.Init();
    }


    void Update()
    {
        _systems.Run();
    }
}
