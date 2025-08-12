using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Spine.Unity;

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

                Debug.Log($"Shared UI - Character {i} ({characterVisuals[i].characterName}) 할당 완료");
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

        Debug.Log($"Setup 시작: UI Mode = {uiMode}");

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
            // 공유 UI 모드
            Debug.Log($"공유 UI 컴포넌트 상태 확인:");
            Debug.Log($"  sharedImageDialog: {sharedImageDialog != null} ({sharedImageDialog?.name})");
            Debug.Log($"  sharedTextName: {sharedTextName != null} ({sharedTextName?.name})");
            Debug.Log($"  sharedTextDialogue: {sharedTextDialogue != null} ({sharedTextDialogue?.name})");
            Debug.Log($"  sharedObjectArrow: {sharedObjectArrow != null} ({sharedObjectArrow?.name})");

            // 공유 UI 초기 상태 설정 - 일단 켜둠 (테스트용)
            if (sharedImageDialog != null)
            {
                sharedImageDialog.gameObject.SetActive(true);
                Debug.Log($"sharedImageDialog 활성화: {sharedImageDialog.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogError("sharedImageDialog가 null입니다! 인스펙터에서 할당해주세요.");
            }

            if (sharedTextName != null)
            {
                sharedTextName.gameObject.SetActive(true);
                sharedTextName.text = "테스트 이름";
                Debug.Log($"sharedTextName 활성화: {sharedTextName.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogError("sharedTextName이 null입니다! 인스펙터에서 할당해주세요.");
            }

            if (sharedTextDialogue != null)
            {
                sharedTextDialogue.gameObject.SetActive(true);
                sharedTextDialogue.text = "테스트 대사";
                Debug.Log($"sharedTextDialogue 활성화: {sharedTextDialogue.gameObject.activeSelf}");
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
                        Debug.Log($"Character {i} 스프라이트 설정 완료");
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
                        Debug.Log($"Character {i} Spine 애니메이션 설정 완료");
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
            Debug.Log("Selection Panel 숨김 처리 완료");
        }

        Debug.Log($"Setup 완료: {uiMode} 모드로 초기화");
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
            SetNextDialog();
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
                buttonComponent.onClick.AddListener(() => OnChoiceSelected(choice.nextBranchName));
            }
        }
    }

    private void OnChoiceSelected(string nextBranchName)
    {
        if (runtimeSelectionUI.selectPanel != null)
        {
            runtimeSelectionUI.selectPanel.gameObject.SetActive(false);
        }

        if (string.IsNullOrEmpty(nextBranchName))
        {
            EndDialog();
        }
        else
        {
            StartBranch(nextBranchName);
        }
    }

    private void SetActiveObjects(Character character, bool visible)
{
    if (uiMode == UIMode.IndividualUI)
    {
        // 개별 UI 모드: 각 캐릭터의 UI를 개별적으로 제어
        if (character.imageDialog != null)
        {
            character.imageDialog.gameObject.SetActive(visible);
            Debug.Log($"Individual UI - ImageDialog 활성화: {visible}");
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
            Debug.Log($"Shared UI - 활성 화자 설정: Character Index {currentCharacterIndex}");
            
            if (sharedImageDialog != null)
            {
                sharedImageDialog.gameObject.SetActive(true);
                Debug.Log($"Shared UI - ImageDialog 활성화: True");
            }

            if (sharedTextName != null)
            {
                sharedTextName.gameObject.SetActive(true);
                Debug.Log($"Shared UI - TextName 활성화: True");
            }

            if (sharedTextDialogue != null)
            {
                sharedTextDialogue.gameObject.SetActive(true);
                Debug.Log($"Shared UI - TextDialogue 활성화: True");
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
        Debug.Log("대화 흐름 종료.");
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
}

// UI 모드 열거형
public enum UIMode
{
    IndividualUI,  // 각 캐릭터별 독립 UI (2개 대화창)
    SharedUI       // 공유 UI (1개 대화창)
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
}

[System.Serializable]
public struct DialogBranch
{
    public string branchName;
    public DialogDate[] dialogs;
    public Choice[] choices;
    public string autoNextBranchName;
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