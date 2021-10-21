using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Phases : MonoBehaviour
{
    public static Phases _Inst;

    static Hero hero;
    static Boss boss;
    static Followers followers;

    void Awake()
    {
        _Inst = this;

        hero = GameObject.FindObjectOfType<Hero>();
        boss = GameObject.FindObjectOfType<Boss>();
        followers = GameObject.FindObjectOfType<Followers>();
    }

    [JsonPropertyAttribute]
    public Lift<Lifted> lifts;
    [JsonPropertyAttribute]
    public List<Talent> allTalents;

    void Start()
    {
        allTalents = new List<Talent>();

        lifts  = new Lift<Lifted>();
        AddStage(0, 004, new BloodHunger(hero));
        AddStage(0, 010, new Block(hero));
        AddStage(0, 020, new BattleExpirience(hero));
        AddStage(0, 030, new Cyclone(hero));
        AddStage(0, 080, new Ressurection(hero));
        AddStage(0, 110, new Blitz());
        AddStage(0, 150, new EnfeeblingStrike(hero));
        AddStage(0, 200, new Interruption(hero));
        AddStage(0, 240, new Rebirth(hero));
        AddStage(0, 280, new Salvation());
        AddStage(0, 320, new TitansGrowth(hero));
        AddStage(0, 420, new BlindingLight(hero));
        AddStage(0, 460, new VeteransOfThirdWar());
        AddStage(0, 500, new Transfusion());
        AddStage(0, 550, new Dejavu(hero));
        AddStage(0, 780, new CounterAttack(hero));

        // AddStage(0, 170, new CoordinatedActions(followers));
        // AddStage(0, 330, new Multicrit(hero));
        // AddStage(0, 650, new HotHand(hero));

        foreach (var item in TalentView.instances)
        {
            item.gameObject.SetActive(false);
        }

        SaveSystem.onAfterLoad += OnAfterLoaded;
        void OnAfterLoaded()
        {
            SoftReset.onMaxStageChanged += (stage)=> CheckFloors();
            Hero.onFragsUpdated += (frags) => CheckFloors();
            SaveSystem.onAfterLoad -= OnAfterLoaded;
        }
    }

    static void AddStage(int reincarnation, int stage, Talent tal)
    {
        tal.SetPhase(stage);

        _Inst.allTalents.Add(tal);

        int floor = MakeFloor(reincarnation,  stage);

        _Inst.lifts.Add(floor, tal.lifted);
    }

    static int MakeFloor(int reincarnation, int stage)
        => reincarnation << 16 | stage;

    void CheckFloors()
    {
        lifts.CheckFloors(
            MakeFloor(Hero._Inst.frags, SoftReset.maxStage));
    }
}
