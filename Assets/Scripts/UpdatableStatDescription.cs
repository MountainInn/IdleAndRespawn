abstract public class UpdatableStatDescription : StatDescription
{
    protected StatMultChain stat;

    override public void ConnectToView(StatInfoView view, StatMultChain stat)
    {
        base.ConnectToView(view, stat);

        this.stat = stat;

        stat.onRecalculate += () => { view.description.text = description; };
        stat.onLoaded += () => { view.description.text = description; };
    }

    override public string description
        => _description + updatableDescription;

    abstract protected string updatableDescription {get;}
}
