using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class UIEffectRequest
{
    public string branchName;
    public string effectType;
    public float timestamp;
}

public class UIManager : MonoBehaviour
{
    [Header("UI 그룹 설정")]
    [SerializeField] private List<UIGroup> uiGroups = new List<UIGroup>();

    [Header("Beautify 컨트롤러")]
    [SerializeField] private BeautifyUIController beautifyController; // 추가: Beautify 컨트롤러 참조

    // 싱글톤 패턴
    public static UIManager Instance { get; private set; }

    private Dictionary<string, List<GameObject>> uiGroupsDict = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, bool> previousStates = new Dictionary<string, bool>();

    // 추가: 이펙트 요청 큐 시스템
    private Queue<UIEffectRequest> effectQueue = new Queue<UIEffectRequest>();
    private bool isProcessingEffect = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 추가: 씬 전환 시에도 유지
            InitializeUIGroups();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeBeautifyController(); // 추가: Beautify 컨트롤러 초기화
        SubscribeToEffectEvents();
    }

    private void InitializeUIGroups()
    {
        foreach (var group in uiGroups)
        {
            if (!uiGroupsDict.ContainsKey(group.groupName))
            {
                uiGroupsDict[group.groupName] = new List<GameObject>();
            }

            uiGroupsDict[group.groupName].AddRange(group.uiObjects);
            Debug.Log($"UI 그룹 '{group.groupName}' 초기화 완료: {group.uiObjects.Count}개 요소");
        }
    }

    private void InitializeBeautifyController()
    {
        if (beautifyController == null)
        {
            beautifyController = FindObjectOfType<BeautifyUIController>();
            if (beautifyController == null)
            {
                Debug.LogWarning("BeautifyUIController를 찾을 수 없습니다!");
                return;
            }
        }

        Debug.Log("BeautifyUIController 초기화 완료");
    }

    private void SubscribeToEffectEvents()
    {
        BeautifyUIController.OnEffectStarted += OnEffectStarted;
        BeautifyUIController.OnEffectCompleted += OnEffectCompleted;
    }

    private void OnEffectStarted()
    {
        Debug.Log("UI 이펙트 시작 - 모든 UI 그룹 숨김 처리");
        HideAllUIGroups();
    }

    private void OnEffectCompleted()
    {
        Debug.Log("UI 이펙트 완료 - 모든 UI 그룹 복원 처리");
        RestoreAllUIGroups();
    }

    private void HideAllUIGroups()
    {
        previousStates.Clear();

        foreach (var kvp in uiGroupsDict)
        {
            string groupName = kvp.Key;
            List<GameObject> uiObjects = kvp.Value;

            // 각 그룹의 현재 상태 저장
            bool wasAnyActive = false;
            foreach (var ui in uiObjects)
            {
                if (ui != null && ui.activeInHierarchy)
                {
                    wasAnyActive = true;
                    break;
                }
            }
            previousStates[groupName] = wasAnyActive;

            // 모든 UI 숨김
            foreach (var ui in uiObjects)
            {
                if (ui != null)
                {
                    ui.SetActive(false);
                }
            }

            Debug.Log($"UI 그룹 '{groupName}' 숨김 처리 완료");
        }
    }

    private void RestoreAllUIGroups()
    {
        foreach (var kvp in previousStates)
        {
            string groupName = kvp.Key;
            bool shouldRestore = kvp.Value;

            if (shouldRestore && uiGroupsDict.ContainsKey(groupName))
            {
                foreach (var ui in uiGroupsDict[groupName])
                {
                    if (ui != null)
                    {
                        ui.SetActive(true);
                    }
                }
                Debug.Log($"UI 그룹 '{groupName}' 복원 완료");
            }
        }

        // 다이얼로그 시스템에 UI 복원 알림
        NotifyDialogSystemRestore();
    }

    private void NotifyDialogSystemRestore()
    {
        var dialogSystem = FindObjectOfType<DialogSystem>();
        if (dialogSystem != null)
        {
            dialogSystem.RestoreUIAfterEffect();
            Debug.Log("DialogSystem에 UI 복원 알림 전송 완료");
        }
    }

    public static void RequestDialogEffect(string branchName, string effectType)
    {
        if (Instance != null)
        {
            Instance.ProcessEffectRequest(branchName, effectType);
        }
        else
        {
            Debug.LogError("UIManager Instance가 없습니다!");
        }
    }

    private void ProcessEffectRequest(string branchName, string effectType)
    {
        var request = new UIEffectRequest
        {
            branchName = branchName,
            effectType = effectType,
            timestamp = Time.time
        };

        effectQueue.Enqueue(request);

        if (!isProcessingEffect)
        {
            StartCoroutine(ProcessEffectQueue());
        }
    }
    private IEnumerator ProcessEffectQueue()
    {
        isProcessingEffect = true;

        while (effectQueue.Count > 0)
        {
            var request = effectQueue.Dequeue();
            yield return StartCoroutine(ExecuteEffectRequest(request));
        }

        isProcessingEffect = false;
    }

    private IEnumerator ExecuteEffectRequest(UIEffectRequest request)
    {
        Debug.Log($"이펙트 요청 실행: {request.branchName} - {request.effectType}");

        if (beautifyController == null)
        {
            Debug.LogError("BeautifyUIController가 설정되지 않았습니다!");
            yield break;
        }

        switch (request.effectType.ToLower())
        {
            case "blink":
                beautifyController.StartBlinking();
                yield return new WaitUntil(() => !beautifyController.IsBlinking());
                break;

            case "blur_start":
                beautifyController.ApplyStartBlur();
                break;

            case "blur_end":
                beautifyController.ApplyEndBlur();
                break;

            case "blink_with_blur":
                yield return StartCoroutine(ExecuteBlinkWithBlur());
                break;

            case "reset":
                beautifyController.ResetAllEffects();
                break;

            default:
                Debug.LogWarning($"알 수 없는 이펙트 타입: {request.effectType}");
                break;
        }
    }

    private IEnumerator ExecuteBlinkWithBlur()
    {
        bool effectCompleted = false;
        beautifyController.ExecuteBlinkWithBlur(() => effectCompleted = true);

        yield return new WaitUntil(() => effectCompleted);
    }

    public void AddUIGroup(string groupName, List<GameObject> uiObjects)
    {
        if (!uiGroupsDict.ContainsKey(groupName))
        {
            uiGroupsDict[groupName] = new List<GameObject>();
        }

        uiGroupsDict[groupName].AddRange(uiObjects);
        Debug.Log($"UI 그룹 '{groupName}' 추가됨: {uiObjects.Count}개 요소");
    }

    public void SetUIGroupActive(string groupName, bool active)
    {
        if (uiGroupsDict.ContainsKey(groupName))
        {
            foreach (var ui in uiGroupsDict[groupName])
            {
                if (ui != null)
                {
                    ui.SetActive(active);
                }
            }
            Debug.Log($"UI 그룹 '{groupName}' 활성화 상태 변경: {active}");
        }
    }

    public void SetCustomBlinkSettings(int count, float speed)
    {
        if (beautifyController != null)
        {
            beautifyController.SetCustomBlinkSettings(count, speed);
        }
    }

    public void SetCustomBlurValues(float startBlur, float endBlur)
    {
        if (beautifyController != null)
        {
            beautifyController.SetCustomBlurValues(startBlur, endBlur);
        }
    }

    private void OnDestroy()
    {
        BeautifyUIController.OnEffectStarted -= OnEffectStarted;
        BeautifyUIController.OnEffectCompleted -= OnEffectCompleted;
    }
}

[System.Serializable]
public struct UIGroup
{
    public string groupName;
    public List<GameObject> uiObjects;
}


