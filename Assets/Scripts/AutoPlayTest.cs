using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

public class AutoPlayTest : MonoBehaviour
{
    FileWriter logger;

    Timer autobuyTimer = new Timer(10);
    Timer statbuyTimer = new Timer(60 * 5);

    List<TalentStripView> talentBuys ;
    List<StatView> statBuys = new List<StatView>();

    void Start()
    {
        // Tutorial._Inst.isReincarnationTutorialSeen = true;
        // Tutorial._Inst.isFirstTutorialSeen = true;

        ShoppingCart._BuyLevelQuantity = 1;

        autobuyTimer.T = autobuyTimer.endTime * .9f;

        talentBuys = TalentStripView.instances.Where(tv=>tv.thisTalent != null).ToList();
        talentBuys.Sort(
        delegate (TalentStripView a, TalentStripView b)
        {
                    if (a.thisTalent.vendible.price < b.thisTalent.vendible.price) return -1;
                    else if (a.thisTalent.vendible.price == b.thisTalent.vendible.price) return 0;
                    else return 1;
            });
        Talent.onStripViewInitialized += (strip)=>{
            talentBuys.Add(strip);

            talentBuys.Sort(
            delegate (TalentStripView a, TalentStripView b)
            {
                    if (a.thisTalent.vendible.price < b.thisTalent.vendible.price) return -1;
                    else if (a.thisTalent.vendible.price == b.thisTalent.vendible.price) return 0;
                    else return 1;
            });
        };

        statBuys = StatView.instances;

        InitializeLogger();
    }


    void InitializeLogger()
    {
        logger = new LogPhases();
    }


    void Update()
    {
        if (autobuyTimer.Tick())
        {
            if (talentBuys != null)
            foreach (var item in talentBuys)
            {
                if (item.currentState.GetType() == typeof(TalentStripView.DiscoveredState))
                {
                    item.buyButton.onClick.Invoke();;
                }
            }


            if (AdCharges.IsGreaterThanZero)
            {
                RewardedAdsButton.MyOnUnityAdsShowComplete();
            }
        }

        if (statbuyTimer.Tick())
        {
            int safeCounter = 0;

            statBuys
            .Sort(delegate (StatView a, StatView b)
            {
                            if (a.stat.cost < b.stat.cost) return -1;
                    else if (a.stat.cost == b.stat.cost) return 0;
                        else return 1;
                    });

            List<Button> buttons;
            while (
                (buttons = (
                    statBuys
                    .Select(view => {
                        if (view.stat.CalculateMaxAffordableLevel2(ShoppingCart._BuyLevelQuantity, out bool canAfford) > 0 && canAfford)
                            return view.button;

                        return null;
                    })
                    .Where(button => button != null)
                    .ToList()
                ))
                .Count() > 0
            )
            {
                foreach (var item in buttons)
                    item.onClick.Invoke();

                if (++safeCounter > 3000)
                {
                    break;
                }
            }
        }
    }


    void OnDisable()
    {
        logger.Dispose();
    }
}
public class LogPhases : FileWriter
{
    List<UniversalLogPayload> logs = new List<UniversalLogPayload>();

    public LogPhases() : base("Autoplay Logs")
    {
        OpenNewLog("Phase 0");

        Phases._Inst.lifts.onLifted += (liftedTalent) => { OnPhaseActivated(liftedTalent.floor); };
        Hero.onFragsUpdated += (frags) =>
        {
            OnPhaseActivated(SoftReset.maxStage);
            Application.Quit();
        };
    }

    override public void Dispose()
    {
        foreach (var item in logs)
        {
            item.CloseLog();
            Log(item.ToString());
        }

        base.Dispose();
    }

    void OnPhaseActivated(int stage)
    {
        WriteLastLog();
       
        OpenNewLog($"Phase {stage}");
    }

    void WriteLastLog()
    {
        var lastLog = logs.Last();

        lastLog.CloseLog();

        string logString = lastLog.ToString();

        Log(logString);

        Debug.Log(logString);

        logs.Remove(lastLog);
    }

    void OpenNewLog(string message)
    {
        logs.Add(new UniversalLogPayload(message));
    }
}
public class LogStages : FileWriter
{
    List<StageLogPayload> stageLogs = new List<StageLogPayload>();

    public LogStages() : base("Autoplay Logs")
    {
        stageLogs.Add(new StageLogPayload());
    }

    public void OnTalentActivated(Talent talent)
    {
        stageLogs.Last().LogNewTalent(talent);
    }

    public void OnMaxStageChanged(int maxStage)
    {
        var lastStageLog = stageLogs.Last();

        lastStageLog.CloseLog();

        stageLogs.Add(new StageLogPayload());

        WriteStageLogToFile(lastStageLog);
    }

    public void WriteStageLogToFile(StageLogPayload lastStageLog)
    {
        Log(lastStageLog.ToString());
    }
}

abstract public class LogPayload
{
    protected int startingRespawns, totalRespawns;
    protected double startingTime, endTime;
    int startingStage, endStage;


    protected LogPayload()
    {
        OpenLog();
    }

    public void OpenLog()
    {
        startingTime = Time.time;
        startingRespawns = SoftReset.respawnCount;
        ConcreteOpenLog();;
    }
    abstract protected void ConcreteOpenLog();

    public void CloseLog()
    {
        totalRespawns = SoftReset.respawnCount - startingRespawns;
        endTime = Time.time;
        ConcreteCloseLog();
    }
    abstract protected void ConcreteCloseLog();


    public override string ToString()
    {
        var startingTimespan = TimeSpan.FromSeconds((startingTime)).ToStringFormatted();
        var durationTimespan = TimeSpan.FromSeconds(( endTime - startingTime)).ToStringFormatted();

        return $"[{startingTimespan}] ({durationTimespan}) ;{totalRespawns:000} " + ConcreteToString() +"\n";
    }

    abstract protected string ConcreteToString();
}

public class UniversalLogPayload : LogPayload
{
    string message;

    public UniversalLogPayload(string message)
    {
        this.message = message;
    }

    protected override void ConcreteCloseLog() {}

    protected override void ConcreteOpenLog() {}

    protected override string ConcreteToString()
    {
        return message;
    }
}

public class StageLogPayload : LogPayload
{
    string message;
    int startingStage, endStage;
    List<TalentLogPayload> talentLogs = new List<TalentLogPayload>();

    protected override void ConcreteOpenLog()
    {
        startingStage = SoftReset.maxStage;
        talentLogs.Add(new TalentLogPayload());
    }

    public void LogNewTalent(Talent talent)
    {
        talentLogs.Last().CloseLog();

        talentLogs.Add(new TalentLogPayload(talent.name));
    }

    protected override void ConcreteCloseLog()
    {
        endStage = SoftReset.maxStage;

        if (talentLogs.Count == 1) talentLogs.Clear();
        else talentLogs.Last().CloseLog();
    }

    override public string ToString()
    {
        double duration = startingTime + endTime;

        string endTimespanReal = TimespanExtension.ToStringFormatted(TimeSpan.FromSeconds(duration));

        string output = $"[{endTimespanReal}] "+  $" Stage {startingStage} -> {endStage}\n";

        foreach (var item in talentLogs) output += item.ToString();

        return output;
    }

    protected override string ConcreteToString()
    {
        throw new NotImplementedException();
    }
}


public class TalentLogPayload: LogPayload
{
    string talentName;

    public TalentLogPayload(string talentName = "") : base()
    {
        this.talentName = talentName;
    }

    protected override void ConcreteOpenLog() {}

    protected override void ConcreteCloseLog() {}

    override public string ToString()
    {
        return new string('-', 10)+" " +  $" {talentName}\n";
    }

    protected override string ConcreteToString()
    {
        throw new NotImplementedException();
    }
}
