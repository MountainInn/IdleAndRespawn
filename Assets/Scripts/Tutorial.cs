using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Tutorial : MonoBehaviour
{
    static Tutorial inst;
    static public Tutorial _Inst => inst??=GameObject.FindObjectOfType<Tutorial>();


    [SerializeField, SpaceAttribute] Image blackPanel;
    [SerializeField] Text hintText;
    [SerializeField] RectTransform prefUnmask, hintPanel;
    [SerializeField] Button btnNext;
    [SerializeField] Transform unmaskParent;
    [SerializeField] Transform prefTutorialView, tutorialsList;

    [SerializeField] List<CanvasGroupFadeInOut> menus;

    [SerializeField, JsonPropertyAttribute]
    TutorialInstance
        firstTutorial,
        reincarnationTutorial;

    Pool<RectTransform> unmasks;




    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext context)
    {
        if (firstTutorial.alreadySeen) SpawnTutorialView(firstTutorial);
        if (reincarnationTutorial.alreadySeen) SpawnTutorialView(reincarnationTutorial);
    }


    void Awake()
    {
        unmasks = new Pool<RectTransform>(unmaskParent, prefUnmask, 3);

        btnNext.onClick.AddListener(()=>{ isWaitingForNextHintButton = false; });

        SetChildrenActive(false);

        Boss.onBossRespawned += ()=>
        {
            if (!reincarnationTutorial.alreadySeen)
                StartShowTutorial(reincarnationTutorial);
        };

        SaveSystem.onAfterLoad += ()=>
        {
            if (!firstTutorial.alreadySeen)
            {
                StartShowTutorial(firstTutorial);
            }

        };
    }


    void SpawnTutorialView(TutorialInstance tutorial)
    {
        var instTutorialView = Instantiate(prefTutorialView, Vector3.zero, Quaternion.identity, tutorialsList.transform);

        instTutorialView.GetComponentInChildren<Text>().text = tutorial.name;

        instTutorialView.GetComponentInChildren<Button>().onClick.AddListener(()=>
        {
            StartShowTutorial(tutorial);
        });

    }

    void StartShowTutorial(TutorialInstance tutorial)
    {
        StartCoroutine(ShowTutorial(tutorial));
    }

    IEnumerator ShowTutorial(TutorialInstance tutorial)
    {
        Pause();

        foreach (var item in menus) item.OutImmediate();

        SetChildrenActive(true);


        foreach (var hint in tutorial.hints)
        {
            unmasks.FitActiveToNumber(hint.targets.Count);

            hint.Show(unmasks.actives, hintText, hintPanel);

            yield return WaitForNextHintButton();
        }


        if (!tutorial.alreadySeen)
        {
            SpawnTutorialView(tutorial);

            tutorial.alreadySeen = true;
        }

        SetChildrenActive(false);

        Unpause();

        yield return null;
    }

    bool isWaitingForNextHintButton;

    IEnumerator WaitForNextHintButton()
    {
        var wait = new WaitForEndOfFrame();

        isWaitingForNextHintButton = true;

        while( isWaitingForNextHintButton ) { yield return wait; }
    }
    

    float backupTimeScale = 1;
    void Pause()
    {
        backupTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }
    void Unpause()
    {
        Time.timeScale = backupTimeScale;
    }



    void SetChildrenActive(bool isActive)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    [SerializableAttribute, JsonObjectAttribute(MemberSerialization.OptIn)]
    class TutorialInstance
    {
        [SerializeField] public string name;
        [SerializeField] public List<Hint> hints;
        [HideInInspector, JsonPropertyAttribute]
        public bool alreadySeen;
    }

    [SerializableAttribute]
    class Hint
    {
        [SerializeField]
        public List<RectTransform> targets;

        [SerializeField, MultilineAttribute(5)]
        public string hintStr;

        [SerializeField]
        public HintPivot hintPivot = HintPivot.center;

        [SerializeField]
        public float y;

        public void Show(List<RectTransform> masks, Text hintText, RectTransform hintPanel)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var mask = masks[i];
                var target = targets[i];

                mask.anchorMin = target.anchorMin;
                mask.anchorMax = target.anchorMax;
                mask.pivot = target.pivot;
                mask.position = target.position;
                mask.sizeDelta = target.sizeDelta;
                mask.localScale = target.localScale;
            }


            hintText.text = hintStr;

            Vector3 anchors = Vector3.one;

            switch (hintPivot)
            {
                case HintPivot.top: anchors = new Vector3(.5f, 1); break;
                case HintPivot.center: anchors = new Vector3(.5f, .5f); break;
                case HintPivot.bottom: anchors = new Vector3(.5f, 0); break;
            }


            hintPanel.anchorMin = anchors;
            hintPanel.anchorMax = anchors;

            hintPanel.anchoredPosition = new Vector3(0, y, 0);
        }

        public enum HintPivot
        {
            top, center, bottom
        }
    }
}

