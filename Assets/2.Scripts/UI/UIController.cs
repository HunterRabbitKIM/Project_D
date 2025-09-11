using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("UI 그룹 설정")]
    [SerializeField] private List<UIGroup> uiGroups = new List<UIGroup>();

    public static UIController instance {get; private set;}

    private Dictionary<string, List<GameObject>> uiGroupsDict = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, bool> previousStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            InitializeUIGroups();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void InitializeUIGroups()
    {
        foreach(var group in uiGroups)
        {
            if(!uiGroupsDict.ContainsKey(group.groupName))
            {
                uiGroupsDict[group.groupName] = new List<GameObject>();
            }
            uiGroupsDict[group.groupName].AddRange(group.uiObjects);
        }
    }

    private void SubscribeToEvents()
    {
        BeautifyUIController.OnEffectStarted += OnEffectStarted;
        BeautifyUIController.OnEffectCompleted += OnEffectCompleted;
    }

    private void OnEffectStarted()
    {
        HideAllUIGroups();
    }

    private void OnEffectCompleted()
    {
        RestoreAllUIGroups();
    }

    private void HideAllUIGroups()
    {
        previousStates.Clear();
        foreach(var kvp in uiGroupsDict)
        {
            string groupName = kvp.Key;
            List<GameObject> uiElements = kvp.Value;

            bool wasAnyActive = false;
            foreach(var ui in uiElements)
            {
                if(ui != null && ui.activeInHierarchy)
                {
                    wasAnyActive = true;
                    break;
                }
            }
            previousStates[groupName] = wasAnyActive;

            foreach(var ui in uiElements)
            {
                if(ui != null)
                {
                    ui.SetActive(false);
                }
            }
        }
    }

    private void RestoreAllUIGroups()
    {
        foreach(var kvp in previousStates)
        {
            string groupName = kvp.Key;
            bool shouldRestore = kvp.Value;

            if (shouldRestore && uiGroupsDict.ContainsKey(groupName))
            {
                foreach(var ui in uiGroupsDict[groupName])
                {
                    if(ui != null)
                    {
                        ui.SetActive(true);
                    }
                }
            }
        }

        var dialogSystem = FindObjectOfType<DialogSystem>();
        if(dialogSystem != null)
        {
            dialogSystem.RestoreUIAfterEffect();
        }
    }

    public void AddUIGroup(string groupName, List<GameObject> uiElements)
    {
        if(!uiGroupsDict.ContainsKey(groupName))
        {
            uiGroupsDict[groupName] = new List<GameObject>();
        }
        uiGroupsDict[groupName].AddRange(uiElements);
    }

    public void SetUIGroupActive(string groupName, bool active)
    {
        if(uiGroupsDict.ContainsKey(groupName))
        {
            foreach(var ui in uiGroupsDict[groupName])
            {
                if(ui != null)
                {
                    ui.SetActive(active);
                }
            }
        }
    }

    private void OnDestroy()
    {
        BeautifyUIController.OnEffectStarted -= OnEffectStarted;
        BeautifyUIController.OnEffectCompleted -= OnEffectCompleted;
    }

}

