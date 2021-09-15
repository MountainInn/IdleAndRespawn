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
    public Lift<LiftedTalent> liftTalents;

    void Start()
    {
        liftTalents  = new Lift<LiftedTalent>(ref SoftReset.onMaxStageChanged);
        AddStage(0, 005, new FullBlood(hero));
        AddStage(0, 023, new Block(hero));
        AddStage(0, 034, new Ressurection(hero));
        AddStage(0, 045, new BattleExpirience(hero));
        AddStage(0, 070, new Interruption(hero));
        AddStage(0, 100, new TitansGrowth(hero));
        AddStage(0, 101, new StaminaTraining(followers));
        AddStage(0, 150, new BlindingLight(hero));
        AddStage(0, 160, new Blitz());
        AddStage(0, 190, new Regeneration(hero));
        AddStage(0, 230, new CoordinatedActions(followers));
        AddStage(0, 270, new Diversion(followers));
        AddStage(0, 300, new EnfeeblingStrike(hero));
        AddStage(0, 320, new CounterAttack(hero));
        AddStage(0, 360, new Rebirth(hero));
        AddStage(0, 400, new Transfusion());
        AddStage(0, 450, new Infirmary(followers));
        AddStage(0, 470, new Multicrit(hero));
        AddStage(0, 530, new VeteransOfThirdWar());
        AddStage(0, 560, new Cyclone(hero));
        AddStage(0, 600, new CounterAttack(hero));
        AddStage(0, 650, new HotHand(hero));
        AddStage(0, 700, new Dejavu(hero));

        foreach (var item in TalentView.instances)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in liftTalents.floors.Values)
        {
            item.SetPhase(item.floor);
        }

        liftTalents.CheckFloors(SoftReset.maxStage);
    }

    static void AddStage(int reincarnation, int stage, LiftedTalent tal)
    {
        int id = reincarnation << 16 | stage;
        _Inst.liftTalents.Add(id, tal);
    }
}
