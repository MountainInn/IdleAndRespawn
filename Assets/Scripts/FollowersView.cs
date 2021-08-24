using UnityEngine;
using UnityEngine.UI;

public class FollowersView : UnitView<Followers>
{
    static FollowersView inst;
    static public FollowersView _Inst => inst ??= GameObject.FindObjectOfType<FollowersView>();

}
