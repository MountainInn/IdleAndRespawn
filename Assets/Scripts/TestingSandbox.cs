using UnityEngine;

public class TestingSandbox : MonoBehaviour
{
    Hero hero;
    Followers followers;
    Boss boss;

    void Start()
    {
        hero = Hero._Inst;
        followers = Followers._Inst;
        boss = Boss._Inst;

    }

    private void TestInterruptionAndLastwishes()
    {
        AdProgression.lastWishes.OnLifted();

        new Interruption(hero).PublicConnect();

        hero.healthRange.UpgradeMaxWithValRestoration(2000);

        followers.healthRange.UpgradeMaxWithValRestoration(1000);

        boss.damage = new StatMultChain(4000, 0, 0);
    }
}
