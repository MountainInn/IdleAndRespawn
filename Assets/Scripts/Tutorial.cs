using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Tutorial : MonoBehaviour
{
    static Tutorial inst;
    static public Tutorial _Inst => inst??=GameObject.FindObjectOfType<Tutorial>();


    [SerializeField] Image blackPanel;
    [SerializeField] Text hintText;
    [SerializeField] RectTransform prefUnmask, hintPanel;
    [SerializeField] Button btnNext;
    [SerializeField] Transform unmaskParent;
    [SerializeField] VerticalLayoutGroup tutorialsList;
    [SerializeField] RectTransform prefTutorialView;

    [SerializeField,
    JsonPropertyAttribute]
    List<TutorialInstance> tutorials;

    Pool<RectTransform> unmasks;

    [JsonPropertyAttribute]
    public bool
        isFirstTutorialSeen,
        isReincarnationTutorialSeen;
    
    void Awake()
    {
        unmasks = new Pool<RectTransform>(unmaskParent, prefUnmask, 3);

        btnNext.onClick.AddListener(()=>{ isWaitingForNextHintButton = false; });

        SetChildrenActive(false);

        Boss.onBossRespawned += ()=>
        {
            if (!isReincarnationTutorialSeen) StartShowTutorial("Reincarnation");
            isReincarnationTutorialSeen = true;
        };
    }

    void Start()
    {
        PopulateTutorialList();

        
        // if (!isFirstTutorialSeen)
        // {
        //     StartShowTutorial("Stats", "Boss", "Hero", "Followers", "Healing", "Scrying");
        //     isFirstTutorialSeen = true;
        // }
        
        
    }

    public void StartShowTutorial(params string[] tutorialNames)
    {
        StartCoroutine(ShowTutorial(tutorialNames));
    }

    IEnumerator ShowTutorial(params string[] tutorialNames)
    {
        Pause();

        SetChildrenActive(true);
        
        foreach (var name in tutorialNames)
        {
            TutorialInstance currentTutorial = tutorials.FirstOrDefault(t =>t.name == name);

            if (currentTutorial == default) { Debug.Log("No tutorial named "+ name); yield return null; }


            foreach (var hint in currentTutorial.hints)
            {
                unmasks.FitActiveToNumber(hint.targets.Count);

                hint.Show(unmasks.actives, hintText, hintPanel);

                yield return WaitForNextHintButton();
            }


            currentTutorial.alreadySeen = true;
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
    

    void Pause() => Time.timeScale = 0;

    void Unpause() => Time.timeScale = 1f;


    void PopulateTutorialList()
    {
        foreach ( var item in tutorials )
        {
            var instTutorialView = Instantiate(prefTutorialView, Vector3.zero, Quaternion.identity, tutorialsList.transform);

            instTutorialView.GetComponentInChildren<Text>().text = item.name;

            instTutorialView.GetComponentInChildren<Button>().onClick.AddListener(()=>
            {
                StartShowTutorial(item.name);
            });
        }
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

            Vector3 positionMask = Vector3.one;

            switch (hintPivot)
            {
                case HintPivot.center: positionMask = new Vector3(0, 0); break;
                case HintPivot.leftTop: positionMask = new Vector3(-1, 1); break;
                case HintPivot.leftBottom: positionMask = new Vector3(-1, -1); break;
                case HintPivot.rightTop: positionMask = new Vector3(1, 1); break;
                case HintPivot.rightBottom: positionMask = new Vector3(1, -1); break;
            }

            float offset = Screen.width / 6;

            hintPanel.localPosition = positionMask * offset;
        }

        public enum HintPivot
        {
            center, leftTop, leftBottom, rightTop, rightBottom
        }
    }
}

