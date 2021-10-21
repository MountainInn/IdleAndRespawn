public class PerseveranceStatDescription : UpdatableStatDescription
{
    new PerseveranceStat stat;
    void Start()
    {
        stat = PerseveranceStat._Inst;

    }
    override protected string updatableDescription =>
        $"\n +{stat.mutation - 1f:P2}";
        }
