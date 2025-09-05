using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class DialogSystem : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private DialogSystemDataBase database;

    [Header("UI Mode Selection")]
    [SerializeField] private UIMode uiMode = UIMode.IndividualUI;

    [Header("Individual UI Mode - 각 캐릭터별 독립 UI")]
    [SerializeField] private CharacterUIComponents[] characterUIComponents;

    [Header("Shared UI Mode - 공유 UI")]
    [SerializeField] private CharacterVisualComponents[] characterVisuals;
    [SerializeField] private Image sharedImageDialog;
    [SerializeField] private TextMeshProUGUI sharedTextName;
    [SerializeField] private TextMeshProUGUI sharedTextDialogue;
    [SerializeField] private GameObject sharedObjectArrow;

    [Header("Selection UI Components")]
    [SerializeField] private Transform selectionPanel;
    [SerializeField] private GameObject selectionButtonPrefab;

    // 런타임에서 사용할 실제 데이터들
    private Character[] runtimeCharacters;
    private SelectionUI runtimeSelectionUI;

    // 런타임 변수들
    private bool isConversationActive = false;
    private int currentDialogIndex = -1;
    private int currentCharacterIndex = 0;
    private DialogBranch currentBranch;

    private float typingSpeed = 0.1f;
    private bool isTypingEffect = false;

    public DialogSystemDataBase Database
    {
        get => database;
        set => database = value;
    }

    private void Awake()
    {
        if (database != null)
        {
            // UI 모드에 따라 다른 방식으로 런타임 데이터 생성
            if (uiMode == UIMode.IndividualUI)
            {
                BuildRuntimeDataFromIndividualUI();
            }
            else
            {
                BuildRuntimeDataFromSharedUI();
            }

            Setup();
            database.BuildBranchDatabase();
        }
        else
        {
            Debug.LogError("DialogSystemDataBase가 할당되지 않았습니다!");
        }
    }

    // 개별 UI 방식
    private void BuildRuntimeDataFromIndividualUI()
    {
        if (characterUIComponents != null && characterUIComponents.Length > 0)
        {
            runtimeCharacters = new Character[characterUIComponents.Length];

            for (int i = 0; i < characterUIComponents.Length; i++)
            {
                Character character = new Character();

                // 각 캐릭터별 독립적인 UI 컴포넌트
                character.spriteRenderer = characterUIComponents[i].spriteRenderer;
                character.spineSkeletonAnimation = characterUIComponents[i].spineSkeletonAnimation;
                character.imageDialog = characterUIComponents[i].imageDialog;
                character.textName = characterUIComponents[i].textName;
                character.textDialogue = characterUIComponents[i].textDialogue;
                character.objectArrow = characterUIComponents[i].objectArrow;

                runtimeCharacters[i] = character;

                Debug.Log($"Individual UI - Character {i} ({characterUIComponents[i].characterName}) 할당 완료: " +
                         $"ImageDialog={character.imageDialog != null}, " +
                         $"TextName={character.textName != null}, " +
                         $"TextDialogue={character.textDialogue != null}");
            }
        }
        else
        {
            Debug.LogError("Individual UI Mode: Character UI Components가 할당되지 않았습니다!");
        }
    }

    // 공유 UI 방식
    private void BuildRuntimeDataFromSharedUI()
    {
        if (characterVisuals != null && characterVisuals.Length > 0)
        {
            runtimeCharacters = new Character[characterVisuals.Length];

            for (int i = 0; i < characterVisuals.Length; i++)
            {
                Character character = new Character();

                // 각 캐릭터별 비주얼 컴포넌트
                character.spriteRenderer = characterVisuals[i].spriteRenderer;
                character.spineSkeletonAnimation = characterVisuals[i].spineSkeletonAnimation;

                // 공유 UI 컴포넌트 (모든 캐릭터가 같은 UI 사용)
                character.imageDialog = sharedImageDialog;
                character.textName = sharedTextName;
                character.textDialogue = sharedTextDialogue;
                character.objectArrow = sharedObjectArrow;

                runtimeCharacters[i] = character;
            }
        }
        else
        {
            Debug.LogError("Shared UI Mode: Character Visual Components가 할당되지 않았습니다!");
        }
    }

    // Setup 메서드 수정 (디버깅 강화)
    private void Setup()
    {
        if (runtimeCharacters == null)
        {
            Debug.LogWarning("RuntimeCharacters가 null입니다.");
            return;
        }

        if (uiMode == UIMode.IndividualUI)
        {
            // 개별 UI 모드
            for (int i = 0; i < runtimeCharacters.Length; i++)
            {
                SetActiveObjects(runtimeCharacters[i], false);

                if (runtimeCharacters[i].spriteRenderer != null)
                {
                    runtimeCharacters[i].spriteRenderer.gameObject.SetActive(true);
                    Color color = runtimeCharacters[i].spriteRenderer.color;
                    color.a = 0.5f;
                    runtimeCharacters[i].spriteRenderer.color = color;
                }

                if (runtimeCharacters[i].spineSkeletonAnimation != null)
                {
                    runtimeCharacters[i].spineSkeletonAnimation.gameObject.SetActive(true);
                    if (runtimeCharacters[i].spineSkeletonAnimation.skeleton != null)
                    {
                        Color color = runtimeCharacters[i].spineSkeletonAnimation.skeleton.GetColor();
                        color.a = 0.5f;
                        runtimeCharacters[i].spineSkeletonAnimation.skeleton.SetColor(color);
                    }
                }
            }
        }
        else
        {
            /*
            // 공유 UI 모드
            Debug.Log($"공유 UI 컴포넌트 상태 확인:");
            Debug.Log($"  sharedImageDialog: {sharedImageDialog != null} ({sharedImageDialog?.name})");
            Debug.Log($"  sharedTextName: {sharedTextName != null} ({sharedTextName?.name})");
            Debug.Log($"  sharedTextDialogue: {sharedTextDialogue != null} ({sharedTextDialogue?.name})");
            Debug.Log($"  sharedObjectArrow: {sharedObjectArrow != null} ({sharedObjectArrow?.name})");
            */
            
            // 공유 UI 초기 상태 설정 - 일단 켜둠 (테스트용)
            if (sharedImageDialog != null)
            {
                sharedImageDialog.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("sharedImageDialog가 null입니다! 인스펙터에서 할당해주세요.");
            }

            if (sharedTextName != null)
            {
                sharedTextName.gameObject.SetActive(true);
                sharedTextName.text = "테스트 이름";
            }
            else
            {
                Debug.LogError("sharedTextName이 null입니다! 인스펙터에서 할당해주세요.");
            }

            if (sharedTextDialogue != null)
            {
                sharedTextDialogue.gameObject.SetActive(true);
                sharedTextDialogue.text = "테스트 대사";
            }
            else
            {
                Debug.LogError("sharedTextDialogue가 null입니다! 인스펙터에서 할당해주세요.");
            }

            if (sharedObjectArrow != null)
            {
                sharedObjectArrow.SetActive(false); // 화살표는 처음에 숨김
            }

            // 각 캐릭터의 스프라이트 설정
            if (characterVisuals != null)
            {
                for (int i = 0; i < characterVisuals.Length; i++)
                {
                    if (characterVisuals[i].spriteRenderer != null)
                    {
                        characterVisuals[i].spriteRenderer.gameObject.SetActive(true);
                        Color color = characterVisuals[i].spriteRenderer.color;
                        color.a = 0.5f;
                        characterVisuals[i].spriteRenderer.color = color;
                    }

                    if (characterVisuals[i].spineSkeletonAnimation != null)
                    {
                        characterVisuals[i].spineSkeletonAnimation.gameObject.SetActive(true);
                        if (characterVisuals[i].spineSkeletonAnimation.skeleton != null)
                        {
                            Color color = characterVisuals[i].spineSkeletonAnimation.skeleton.GetColor();
                            color.a = 0.5f;
                            characterVisuals[i].spineSkeletonAnimation.skeleton.SetColor(color);
                        }
                    }
                }
            }
        }

        runtimeSelectionUI = new SelectionUI();
        runtimeSelectionUI.selectPanel = selectionPanel;
        runtimeSelectionUI.selectButtonPrefab = selectionButtonPrefab;

        if (runtimeSelectionUI.selectPanel != null)
        {
            runtimeSelectionUI.selectPanel.gameObject.SetActive(false);
        }
    }

    public void StartConversation()
    {
        if (database == null)
        {
            Debug.LogError("Database가 설정되지 않았습니다!");
            return;
        }

        // 런타임 데이터가 없으면 다시 생성
        if (runtimeCharacters == null)
        {
            if (uiMode == UIMode.IndividualUI)
            {
                BuildRuntimeDataFromIndividualUI();
            }
            else
            {
                BuildRuntimeDataFromSharedUI();
            }
        }

        database.BuildBranchDatabase();

        if (database.OrderedBranches.Count > 0)
        {
            isConversationActive = true;
            StartBranch(database.OrderedBranches[0].branchName);
        }
        else
        {
            Debug.LogError("DialogSystemDataBase에 정의된 System 또는 Branch가 없습니다.");
            isConversationActive = false;
        }
    }

    private void StartBranch(string branchName)
    {
        if (database.BranchDatabase.TryGetValue(branchName, out currentBranch))
        {
            currentDialogIndex = -1;

            // UI 이펙트가 있으면 이펙트 완료 후 대화 시작
            if (ShouldExecuteUIEffect(currentBranch))
            {
                HandleBranchUIEffect(currentBranch, true, () =>
                {
                    // 이펙트 완료 후 대화 시작
                    SetNextDialog();
                });
            }
            else
            {
                // 이펙트가 없으면 바로 대화 시작
                SetNextDialog();
            }
        }
        else
        {
            Debug.LogError($"'{branchName}' Branch를 찾을 수 없습니다!");
            EndDialog();
        }
    }

    public bool UpdateDialog()
    {
        if (!isConversationActive) return true;

        if (Input.GetMouseButtonDown(0))
        {
            if (runtimeSelectionUI.selectPanel != null && runtimeSelectionUI.selectPanel.gameObject.activeSelf)
                return false;

            if (isTypingEffect)
            {
                isTypingEffect = false;
                StopCoroutine("OnTypingText");
                if (runtimeCharacters != null && currentCharacterIndex < runtimeCharacters.Length &&
                    runtimeCharacters[currentCharacterIndex].textDialogue != null)
                {
                    runtimeCharacters[currentCharacterIndex].textDialogue.text = currentBranch.dialogs[currentDialogIndex].dialogue;
                    runtimeCharacters[currentCharacterIndex].objectArrow?.SetActive(true);
                }
                return false;
            }

            if (currentDialogIndex + 1 < currentBranch.dialogs.Length)
            {
                SetNextDialog();
            }
            else
            {
                if (currentBranch.choices != null && currentBranch.choices.Length > 0)
                {
                    ShowChoices();
                }
                else if (!string.IsNullOrEmpty(currentBranch.autoNextBranchName))
                {
                    StartBranch(currentBranch.autoNextBranchName);
                }
                else
                {
                    int currentIndex = database.OrderedBranches.FindIndex(b => b.branchName == currentBranch.branchName);

                    if (currentIndex != -1 && currentIndex < database.OrderedBranches.Count - 1)
                    {
                        StartBranch(database.OrderedBranches[currentIndex + 1].branchName);
                    }
                    else
                    {
                        EndDialog();
                    }
                }
            }
        }
        return !isConversationActive;
    }

    private void SetNextDialog()
    {
        currentDialogIndex++;
        int newSpeakerIndex = currentBranch.dialogs[currentDialogIndex].speakerIndex;

        if (runtimeCharacters == null || newSpeakerIndex >= runtimeCharacters.Length)
        {
            Debug.LogError($"Invalid speaker index: {newSpeakerIndex}. RuntimeCharacters length: {runtimeCharacters?.Length ?? 0}");
            return;
        }

        for (int i = 0; i < runtimeCharacters.Length; i++)
        {
            bool isActiveSpeaker = (i == newSpeakerIndex);
            SetActiveObjects(runtimeCharacters[i], isActiveSpeaker);
        }

        currentCharacterIndex = newSpeakerIndex;

        if (runtimeCharacters[currentCharacterIndex].textName != null)
        {
            runtimeCharacters[currentCharacterIndex].textName.text = currentBranch.dialogs[currentDialogIndex].name;
        }

        StartCoroutine("OnTypingText");
    }

    private void ShowChoices()
    {
        if (currentCharacterIndex >= 0 && runtimeCharacters != null &&
            runtimeCharacters.Length > currentCharacterIndex)
        {
            SetActiveObjects(runtimeCharacters[currentCharacterIndex], false);
        }

        if (runtimeSelectionUI.selectPanel == null)
        {
            Debug.LogError("SelectPanel이 null입니다. Selection Panel을 확인해주세요.");
            return;
        }

        runtimeSelectionUI.selectPanel.gameObject.SetActive(true);

        foreach (Transform child in runtimeSelectionUI.selectPanel)
        {
            Destroy(child.gameObject);
        }

        if (runtimeSelectionUI.selectButtonPrefab == null)
        {
            Debug.LogError("SelectButtonPrefab이 null입니다. Selection Button Prefab을 확인해주세요.");
            return;
        }

        foreach (var choice in currentBranch.choices)
        {
            GameObject buttonGO = Instantiate(runtimeSelectionUI.selectButtonPrefab, runtimeSelectionUI.selectPanel);
            var textComponent = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = choice.text;
            }

            var buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnChoiceSelected(choice));
            }
        }
    }

    private void OnChoiceSelected(Choice selectedChoice)
    {
        if (runtimeSelectionUI.selectPanel != null)
        {
            runtimeSelectionUI.selectPanel.gameObject.SetActive(false);
        }

        // [수정] 씬 이동이 우선 - 카메라 위치 정보와 함께 전달
        if (!string.IsNullOrEmpty(selectedChoice.nextEndingSceneName))
        {
            LoadEndingScene(selectedChoice.nextEndingSceneName, selectedChoice.endingCameraPosition);
        }
        // [기존] 씬 이름이 없을 때만 브랜치 이동
        else if (!string.IsNullOrEmpty(selectedChoice.nextBranchName))
        {
            StartBranch(selectedChoice.nextBranchName);
        }
        // [기존] 둘 다 없으면 대화 종료
        else
        {
            EndDialog();
        }
    }

    // 씬 이동 메서드 추가
    private void LoadEndingScene(string sceneName, Vector3 cameraPosition)
    {

        // [수정] 카메라 위치 정보를 PlayerPrefs에 저장 (Vector3를 3개의 float로 저장)
        PlayerPrefs.SetFloat("EndingCameraX", cameraPosition.x);
        PlayerPrefs.SetFloat("EndingCameraY", cameraPosition.y);
        PlayerPrefs.SetFloat("EndingCameraZ", cameraPosition.z);
        PlayerPrefs.Save();

        // [기존] 다이얼로그 정리
        CleanupDialog();

        // [기존] 씬 로드
        SceneManager.LoadScene(sceneName);
    }

    // [추가] 엔딩 씬에서 위치 설정을 처리할 함수
    public void SetupEndingPosition()
    {
        // [수정] PlayerPrefs에서 카메라 위치 정보 가져오기
        float x = PlayerPrefs.GetFloat("EndingCameraX", 0f);
        float y = PlayerPrefs.GetFloat("EndingCameraY", 0f);
        float z = PlayerPrefs.GetFloat("EndingCameraZ", 0f);

        Vector3 cameraPosition = new Vector3(x, y, z);

        // [수정] 카메라 위치가 (0,0,0)이 아니면 설정 (기본값이 아닌 경우)
        if (cameraPosition != Vector3.zero)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = cameraPosition;
            }
            else
            {
                Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.Log("엔딩 카메라 위치 정보가 없습니다. 기본 위치로 설정합니다.");
        }

        // [수정] 사용 후 삭제
        PlayerPrefs.DeleteKey("EndingCameraX");
        PlayerPrefs.DeleteKey("EndingCameraY");
        PlayerPrefs.DeleteKey("EndingCameraZ");
        PlayerPrefs.Save();
    }

    public void OnSceneLoaded()
    {
        // [수정] 위치 설정 실행 (매개변수 없이)
        SetupEndingPosition();
    }

    // 다이얼로그 정리 메서드 추가
    private void CleanupDialog()
    {
        // UI 정리
        if (runtimeCharacters != null)
        {
            foreach (var character in runtimeCharacters)
            {
                SetActiveObjects(character, false);
            }
        }

        if (runtimeSelectionUI.selectPanel != null)
        {
            runtimeSelectionUI.selectPanel.gameObject.SetActive(false);
        }

        // 상태 초기화
        isConversationActive = false;
    }

    private void SetActiveObjects(Character character, bool visible)
    {
        if (uiMode == UIMode.IndividualUI)
        {
            // 개별 UI 모드: 각 캐릭터의 UI를 개별적으로 제어
            if (character.imageDialog != null)
            {
                character.imageDialog.gameObject.SetActive(visible);
            }

            if (character.textName != null)
                character.textName.gameObject.SetActive(visible);

            if (character.textDialogue != null)
                character.textDialogue.gameObject.SetActive(visible);

            if (character.objectArrow != null)
                character.objectArrow.SetActive(false);

            // 스프라이트 투명도 조절
            if (character.spriteRenderer != null)
            {
                Color color = character.spriteRenderer.color;
                color.a = visible ? 1f : 0.5f;
                character.spriteRenderer.color = color;
            }

            if (character.spineSkeletonAnimation != null && character.spineSkeletonAnimation.skeleton != null)
            {
                Color color = character.spineSkeletonAnimation.skeleton.GetColor();
                color.a = visible ? 1f : 0.5f;
                character.spineSkeletonAnimation.skeleton.SetColor(color);
            }
        }
        else
        {
            // 공유 UI 모드: 오직 활성 화자일 때만 UI 제어
            if (visible) // 활성 화자일 때만 UI를 켬
            {
                if (sharedImageDialog != null)
                {
                    sharedImageDialog.gameObject.SetActive(true);
                }

                if (sharedTextName != null)
                {
                    sharedTextName.gameObject.SetActive(true);
                }

                if (sharedTextDialogue != null)
                {
                    sharedTextDialogue.gameObject.SetActive(true);
                }

                if (sharedObjectArrow != null)
                    sharedObjectArrow.SetActive(false);
            }

            // 모든 캐릭터 스프라이트 투명도 조절
            if (characterVisuals != null)
            {
                for (int i = 0; i < characterVisuals.Length; i++)
                {
                    bool isCurrentSpeaker = (i == currentCharacterIndex);

                    if (characterVisuals[i].spriteRenderer != null)
                    {
                        Color color = characterVisuals[i].spriteRenderer.color;
                        color.a = isCurrentSpeaker ? 1f : 0.5f;
                        characterVisuals[i].spriteRenderer.color = color;
                    }

                    if (characterVisuals[i].spineSkeletonAnimation != null &&
                        characterVisuals[i].spineSkeletonAnimation.skeleton != null)
                    {
                        Color color = characterVisuals[i].spineSkeletonAnimation.skeleton.GetColor();
                        color.a = isCurrentSpeaker ? 1f : 0.5f;
                        characterVisuals[i].spineSkeletonAnimation.skeleton.SetColor(color);
                    }
                }
            }
        }
    }

    private void EndDialog()
    {
        if (currentBranch.branchName != null)
        {
            HandleBranchUIEffect(currentBranch, false); // false = 브랜치 종료
        }
        if (runtimeCharacters != null)
        {
            foreach (var character in runtimeCharacters)
            {
                SetActiveObjects(character, false);
                if (character.spriteRenderer != null)
                {
                    Color color = character.spriteRenderer.color;
                    color.a = 1f;
                    character.spriteRenderer.color = color;
                }
                if (character.spineSkeletonAnimation != null && character.spineSkeletonAnimation.skeleton != null)
                {
                    Color color = character.spineSkeletonAnimation.skeleton.GetColor();
                    color.a = 1f;
                    character.spineSkeletonAnimation.skeleton.SetColor(color);
                }
            }
        }

        // 공유 UI 모드일 때 추가로 캐릭터 비주얼 초기화
        if (uiMode == UIMode.SharedUI && characterVisuals != null)
        {
            for (int i = 0; i < characterVisuals.Length; i++)
            {
                if (characterVisuals[i].spriteRenderer != null)
                {
                    Color color = characterVisuals[i].spriteRenderer.color;
                    color.a = 1f;
                    characterVisuals[i].spriteRenderer.color = color;
                }

                if (characterVisuals[i].spineSkeletonAnimation != null &&
                    characterVisuals[i].spineSkeletonAnimation.skeleton != null)
                {
                    Color color = characterVisuals[i].spineSkeletonAnimation.skeleton.GetColor();
                    color.a = 1f;
                    characterVisuals[i].spineSkeletonAnimation.skeleton.SetColor(color);
                }
            }
        }

        isConversationActive = false;
    }

    private IEnumerator OnTypingText()
    {
        int index = 0;
        isTypingEffect = true;

        string dialogueText = currentBranch.dialogs[currentDialogIndex].dialogue;

        if (runtimeCharacters != null && currentCharacterIndex < runtimeCharacters.Length &&
            runtimeCharacters[currentCharacterIndex].textDialogue != null)
        {
            runtimeCharacters[currentCharacterIndex].textDialogue.text = "";

            while (index < dialogueText.Length)
            {
                if (!isTypingEffect)
                {
                    yield break;
                }
                runtimeCharacters[currentCharacterIndex].textDialogue.text += dialogueText[index];
                index++;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTypingEffect = false;
            runtimeCharacters[currentCharacterIndex].objectArrow?.SetActive(true);
        }
    }

    /*
    // 디버깅용 메서드
    [ContextMenu("Debug Runtime Data")]
    public void DebugRuntimeData()
    {
        Debug.Log($"=== Runtime Data Debug ===");
        Debug.Log($"Database: {database != null}");
        Debug.Log($"UI Mode: {uiMode}");

        if (uiMode == UIMode.IndividualUI)
        {
            Debug.Log($"Individual UI Components: {characterUIComponents?.Length ?? 0}");
            if (characterUIComponents != null)
            {
                for (int i = 0; i < characterUIComponents.Length; i++)
                {
                    var comp = characterUIComponents[i];
                    Debug.Log($"  Character {i} ({comp.characterName}): " +
                             $"ImageDialog={comp.imageDialog != null}, " +
                             $"TextName={comp.textName != null}, " +
                             $"TextDialogue={comp.textDialogue != null}");
                }
            }
        }
        else
        {
            Debug.Log($"Character Visuals: {characterVisuals?.Length ?? 0}");
            Debug.Log($"Shared UI: ImageDialog={sharedImageDialog != null}, " +
                     $"TextName={sharedTextName != null}, " +
                     $"TextDialogue={sharedTextDialogue != null}");
        }

        Debug.Log($"RuntimeCharacters: {runtimeCharacters?.Length ?? 0}");
        Debug.Log($"Selection Panel: {selectionPanel != null}");
        Debug.Log($"Selection Button Prefab: {selectionButtonPrefab != null}");
    }
    */

    private bool ShouldExecuteUIEffect(DialogBranch branch)
    {
        var effectSettings = branch.uiEffectSettings;
        return effectSettings.enableUIEffect && effectSettings.triggerOnBranchStart;
    }

    private void HandleBranchUIEffect(DialogBranch branch, bool isStart, System.Action onComplete = null)
    {
        var effectSettings = branch.uiEffectSettings;

        // UI 이펙트가 비활성화되어 있으면 리턴
        if (!effectSettings.enableUIEffect)
        {
            onComplete?.Invoke();
            return;
        }
        // 시작/종료 조건 확인
        if ((isStart && !effectSettings.triggerOnBranchStart) ||
            (!isStart && !effectSettings.triggerOnBranchEnd))
        {
            onComplete?.Invoke();
            return;
        }

        // 딜레이가 있으면 코루틴으로 실행
        if (effectSettings.delayBeforeEffect > 0)
        {
            StartCoroutine(ExecuteUIEffectWithDelay(effectSettings));
        }
        else
        {
            ExecuteUIEffect(effectSettings, onComplete);
        }
    }

    private IEnumerator ExecuteUIEffectWithDelay(UIEffectSettings settings, System.Action onComplete = null)
    {
        yield return new WaitForSeconds(settings.delayBeforeEffect);
        ExecuteUIEffect(settings, onComplete);
    }

    private void ExecuteUIEffect(UIEffectSettings settings, System.Action onComplete = null)
    {
        switch (settings.effectType)
        {
            case UIEffectType.Blink:
                var beautifyController1 = FindObjectOfType<BeautifyUIController>();
                if (beautifyController1 != null)
                {
                    beautifyController1.SetCustomBlinkSettings(settings.customBlinkCount, settings.customBlinkSpeed);
                    StartCoroutine(WaitForBlinkComplete(beautifyController1, onComplete));
                    beautifyController1.StartBlinking();
                }
                else
                {
                    onComplete?.Invoke();
                }
                break;

            case UIEffectType.Blur:
                UIManager.RequestDialogEffect(currentBranch.branchName, "blur_end");
                onComplete?.Invoke();
                break;

            case UIEffectType.BlinkWithBlur:
                var beautifyController2 = FindObjectOfType<BeautifyUIController>();
                if (beautifyController2 != null)
                {
                    beautifyController2.SetCustomBlinkSettings(settings.customBlinkCount, settings.customBlinkSpeed);
                    beautifyController2.ExecuteBlinkWithBlur(onComplete);
                }
                else
                {
                    onComplete?.Invoke();
                }
                break;

            case UIEffectType.CustomBlur:
                var beautifyController3 = FindObjectOfType<BeautifyUIController>();
                if (beautifyController3 != null)
                {
                    beautifyController3.SetCustomBlurValues(settings.customBlurStart, settings.customBlurEnd);
                    beautifyController3.ApplyEndBlur();
                }
                onComplete?.Invoke(); // 커스텀 블러는 즉시 완료
                break;

            default:
                onComplete?.Invoke();
                break;
        }
    }

    private IEnumerator WaitForBlinkComplete(BeautifyUIController controller, System.Action onComplete)
    {
        // 깜빡임이 시작될 때까지 대기
        yield return new WaitForSeconds(0.1f);

        // 깜빡임이 완료될 때까지 대기
        while (controller.IsBlinking())
        {
            yield return null;
        }

        onComplete?.Invoke();
    }
}

// UI 모드 열거형
public enum UIMode
{
    IndividualUI,  // 각 캐릭터별 독립 UI (2개 대화창)
    SharedUI       // 공유 UI (1개 대화창)
}

[System.Serializable]
public enum UIEffectType
{
    None,
    Blink,
    Blur,
    BlinkWithBlur,
    CustomBlur
}

// 개별 UI용 구조체
[System.Serializable]
public struct CharacterUIComponents
{
    [Header("Character Info")]
    public string characterName;

    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;
    public SkeletonMecanim spineSkeletonAnimation;

    [Header("UI Components")]
    public Image imageDialog;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDialogue;
    public GameObject objectArrow;
}

// 공유 UI용 구조체
[System.Serializable]
public struct CharacterVisualComponents
{
    [Header("Character Info")]
    public string characterName;

    [Header("Visual Components Only")]
    public SpriteRenderer spriteRenderer;
    public SkeletonMecanim spineSkeletonAnimation;
}

// 기존 구조체들은 그대로 유지
[System.Serializable]
public struct Character
{
    public SpriteRenderer spriteRenderer;
    public SkeletonMecanim spineSkeletonAnimation;
    public Image imageDialog;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDialogue;
    public GameObject objectArrow;
}

[System.Serializable]
public struct DialogDate
{
    public int speakerIndex;
    public string name;
    [TextArea(3, 5)]
    public string dialogue;
}

[System.Serializable]
public struct Choice
{
    public string text;
    public string nextBranchName;
    public string nextEndingSceneName;
    public Vector3 endingCameraPosition;
}

[System.Serializable]
public struct DialogBranch
{
    public string branchName;
    public DialogDate[] dialogs;
    public Choice[] choices;
    public string autoNextBranchName;
    public UIEffectSettings uiEffectSettings;
}

[System.Serializable]
public struct DialogSystemGroup
{
    public string systemName;
    public DialogBranch[] branches;
}

[System.Serializable]
public struct SelectionUI
{
    public GameObject selectButtonPrefab;
    public Transform selectPanel;
}

[System.Serializable]
public struct UIEffectSettings
{
    [Header("Effect Settings")]
    public bool enableUIEffect;        // UI 이펙트 활성화 여부
    public UIEffectType effectType;    // 이펙트 타입

    [Header("Timing Settings")]
    public bool triggerOnBranchStart;  // 브랜치 시작 시 트리거
    public bool triggerOnBranchEnd;    // 브랜치 종료 시 트리거
    public float delayBeforeEffect;    // 이펙트 실행 전 딜레이

    [Header("Custom Blur Settings")]
    [Range(0f, 5f)]
    public float customBlurStart;      // 커스텀 시작 블러 값
    [Range(0f, 5f)]
    public float customBlurEnd;        // 커스텀 종료 블러 값

    [Header("Blink Settings")]
    [Range(1, 10)]
    public int customBlinkCount;       // 커스텀 깜빡임 횟수
    [Range(0.1f, 1f)]
    public float customBlinkSpeed;     // 커스텀 깜빡임 속도
}