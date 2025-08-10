using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Spine.Unity;

public class DialogSystem : MonoBehaviour
{
    // [수정] 기존의 dialogs, branches 배열이 제거되고, 시스템 전체를 포괄하는 systems 배열만 남습니다.
    [SerializeField] private DialogSystemGroup[] systems;
    // [수정] Speaker[] -> Character[] 이름 변경 및 역할 명확화
    [SerializeField] private Character[] characters;
    // [추가] 선택지 UI(버튼 프리팹, 패널)를 관리하기 위한 구조체
    [SerializeField] private SelectionUI selectionUI;

    // [추가] Branch 이름을 Key로 하여 빠르게 접근하기 위한 데이터베이스(Dictionary)
    private Dictionary<string, DialogBranch> branchDatabase = new Dictionary<string, DialogBranch>();
    private List<DialogBranch> orderedBranches = new List<DialogBranch>();

    // [추가] 전체 대화 흐름의 활성화 상태를 관리
    private bool isConversationActive = false;
    // [수정] currentDialogIndex는 이제 전체가 아닌 '현재 Branch 내'에서의 인덱스를 의미합니다.
    private int currentDialogIndex = -1;
    private int currentCharacterIndex = 0;
    // [추가] 현재 진행 중인 Branch 데이터를 저장하는 변수
    private DialogBranch currentBranch;

    private float typingSpeed = 0.1f;
    private bool isTypingEffect = false;

    private void Awake()
    {
        Setup();
        // [추가] 게임이 시작될 때 모든 Branch 정보를 미리 데이터베이스에 저장합니다.
        BuildBranchDatabase();
    }

    private void Setup()
    {
        // [수정] speakers -> characters 배열을 사용하도록 변경
        for (int i = 0; i < characters.Length; ++i)
        {
            SetActiveObjects(characters[i], false);
            if (characters[i].spriteRenderer != null)
            {
                characters[i].spriteRenderer.gameObject.SetActive(true);
            }
            if (characters[i].spineSkeletonAnimation != null)
            {
                characters[i].spineSkeletonAnimation.gameObject.SetActive(true);
            }
        }
        // [추가] 선택지 패널이 존재하면 비활성화
        if (selectionUI.selectPanel != null)
        {
            selectionUI.selectPanel.gameObject.SetActive(false);
        }
    }

    // [추가] 에디터에 설정된 모든 Branch를 Dictionary에 저장하여 '이름'으로 빠르게 찾을 수 있도록 준비하는 함수
    private void BuildBranchDatabase()
    {
        branchDatabase.Clear();
        orderedBranches.Clear();
        if (systems == null) return;
        foreach (var system in systems)
        {
            foreach (var branch in system.branches)
            {
                if (!string.IsNullOrEmpty(branch.branchName) && !branchDatabase.ContainsKey(branch.branchName))
                {
                    branchDatabase.Add(branch.branchName, branch);
                    orderedBranches.Add(branch);
                }
                else
                {
                    Debug.LogWarning($"중복되거나 비어있는 Branch 이름이 있습니다: '{branch.branchName}'. 이 Branch는 무시됩니다.");
                }
            }
        }
    }

    // [추가] DialogSystemTrigger에서 대화를 시작하기 위해 호출하는 진입점 함수
    public void StartConversation()
    {
        // [추가] 혹시 모를 에디터 변경에 대비해 데이터베이스를 다시 빌드
        BuildBranchDatabase();

        // [수정] 첫 번째 시스템의 첫 번째 Branch를 시작으로 대화를 개시
        if (orderedBranches.Count > 0)
        {
            isConversationActive = true;
            StartBranch(orderedBranches[0].branchName);
        }
        else
        {
            Debug.LogError("DialogSystem에 정의된 System 또는 Branch가 없습니다.");
            isConversationActive = false;
        }
    }

    // [추가] 특정 이름의 Branch를 찾아 대화를 시작하는 함수
    private void StartBranch(string branchName)
    {
        // [추가] 데이터베이스에서 branchName을 Key로 하여 해당하는 Branch 정보를 찾아 currentBranch에 할당
        if (branchDatabase.TryGetValue(branchName, out currentBranch))
        {
            currentDialogIndex = -1; // 새 Branch가 시작되므로 다이얼로그 인덱스 초기화
            SetNextDialog();
        }
        else
        {
            Debug.LogError($"'{branchName}' Branch를 찾을 수 없습니다!");
            EndDialog();
        }
    }

    // [수정] 대화 진행 로직을 Branch 기반으로 전면 수정
    public bool UpdateDialog()
    {
        if (!isConversationActive) return true; // 대화가 비활성화 상태면 종료되었음을 Trigger에 알림 (true 반환)

        if (Input.GetMouseButtonDown(0))
        {
            // [추가] 선택지가 활성화된 상태에서는 마우스 클릭으로 대화를 넘기지 않음
            if (selectionUI.selectPanel != null && selectionUI.selectPanel.gameObject.activeSelf) return false;

            if (isTypingEffect)
            {
                isTypingEffect = false;
                StopCoroutine("OnTypingText");
                characters[currentCharacterIndex].textDialogue.text = currentBranch.dialogs[currentDialogIndex].dialogue;
                characters[currentCharacterIndex].objectArrow.SetActive(true);
                return false;
            }

            // [수정] 현재 Branch의 다이얼로그가 더 남아있는지 확인
            if (currentDialogIndex + 1 < currentBranch.dialogs.Length)
            {
                SetNextDialog();
            }
            else // [수정] 현재 Branch의 마지막 다이얼로그가 끝났을 때
            {
                // [추가] 현재 Branch에 선택지가 있는지 확인
                if (currentBranch.choices != null && currentBranch.choices.Length > 0)
                {
                    ShowChoices(); // 선택지 표시
                }
                else if(!string.IsNullOrEmpty(currentBranch.autoNextBranchName))
                {
                    StartBranch(currentBranch.autoNextBranchName);
                }
                else
                {
                    // 현재 Branch가 orderedBranches 리스트의 몇 번째에 있는지 찾습니다.
                    int currentIndex = orderedBranches.FindIndex(b => b.branchName == currentBranch.branchName);
                    
                    // 현재 Branch를 찾았고, 마지막 Branch가 아니라면
                    if (currentIndex != -1 && currentIndex < orderedBranches.Count - 1)
                    {
                        // 다음 순서의 Branch를 실행합니다.
                        StartBranch(orderedBranches[currentIndex + 1].branchName);
                    }
                    else // 마지막 Branch이거나, 어떤 이유로든 리스트에서 찾지 못했다면
                    {
                        EndDialog(); // 대화를 종료합니다.
                    }
                }
            }
        }
        return !isConversationActive; // 대화가 아직 진행 중이면 false, EndDialog로 종료됐으면 true 반환
    }

    // [수정] 'currentBranch'에서 다음 다이얼로그 정보를 가져오도록 수정
    private void SetNextDialog()
    {
        // 다음 대사로 인덱스를 넘깁니다.
        currentDialogIndex++;
        // 이번에 말할 캐릭터가 누구인지 인덱스를 가져옵니다.
        int newSpeakerIndex = currentBranch.dialogs[currentDialogIndex].speakerIndex;

        // 모든 캐릭터를 순회하면서
        for (int i = 0; i < characters.Length; i++)
        {
            // 현재 순회중인 캐릭터(i)가 이번에 말할 캐릭터(newSpeakerIndex)인지 확인합니다.
            bool isActiveSpeaker = (i == newSpeakerIndex);
            // 말할 캐릭터이면 대화창을 켜고(true), 아니라면 끕니다(false).
            // 이렇게 하면 항상 단 한 명의 대화창만 켜지는 것이 보장됩니다.
            SetActiveObjects(characters[i], isActiveSpeaker);
        }

        // 현재 대사를 말하는 캐릭터의 인덱스를 저장합니다.
        currentCharacterIndex = newSpeakerIndex;

        // 이름과 대사를 설정하고 타이핑 효과를 시작합니다.
        characters[currentCharacterIndex].textName.text = currentBranch.dialogs[currentDialogIndex].name;
        StartCoroutine("OnTypingText");
    }

    // [추가] 선택지를 화면에 동적으로 생성하고 표시하는 함수
    private void ShowChoices()
    {
        if (currentCharacterIndex >= 0 && characters.Length > currentCharacterIndex)
        {
            SetActiveObjects(characters[currentCharacterIndex], false); // 마지막 대화창 숨기기
        }

        selectionUI.selectPanel.gameObject.SetActive(true); // 선택지 패널 활성화

        // 이전에 생성된 버튼이 있다면 모두 삭제
        foreach (Transform child in selectionUI.selectPanel)
        {
            Destroy(child.gameObject);
        }

        // 현재 Branch의 모든 선택지에 대해 버튼 생성
        foreach (var choice in currentBranch.choices)
        {
            GameObject buttonGO = Instantiate(selectionUI.selectButtonPrefab, selectionUI.selectPanel);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            // 각 버튼에 클릭 이벤트를 연결. 클릭 시 OnChoiceSelected 함수가 'nextBranchName'과 함께 호출됨
            buttonGO.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice.nextBranchName));
        }
    }

    // [추가] 선택지 버튼을 클릭했을 때 호출되는 함수
    private void OnChoiceSelected(string nextBranchName)
    {
        selectionUI.selectPanel.gameObject.SetActive(false); // 선택지 패널 비활성화
        // [추가] 연결될 Branch 이름이 비어있으면 대화 종료, 있으면 해당 Branch 시작
        if (string.IsNullOrEmpty(nextBranchName))
        {
            EndDialog();
        }
        else
        {
            StartBranch(nextBranchName);
        }
    }
    
    // [수정] 매개변수 타입을 Speaker에서 Character로 변경
    private void SetActiveObjects(Character character, bool visible)
    {
        if (character.imageDialog != null)
            character.imageDialog.gameObject.SetActive(visible);
    
        if (character.textName != null)
            character.textName.gameObject.SetActive(visible);
    
        if (character.textDialogue != null)
            character.textDialogue.gameObject.SetActive(visible);
    
        if (character.objectArrow != null)
            character.objectArrow.SetActive(false);

        if (character.spriteRenderer != null)
        {
            Color color = character.spriteRenderer.color;
            color.a = visible ? 1f : 0.5f; // 비활성 캐릭터는 약간 투명하게 처리
            character.spriteRenderer.color = color;
        }

        if(character.spineSkeletonAnimation != null && character.spineSkeletonAnimation.skeleton != null)
        {
            Color color = character.spineSkeletonAnimation.skeleton.GetColor();
            color.a = visible ? 1f : 0.5f;
            character.spineSkeletonAnimation.skeleton.SetColor(color);
        }
    }

    // [수정] 대화 종료 시 모든 캐릭터 UI를 비활성화하고 상태 플래그를 변경
    private void EndDialog()
    {
        foreach (var character in characters)
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
        isConversationActive = false;
        Debug.Log("대화 흐름 종료.");
    }
    
    // [수정] 'dialogs' 배열 대신 'currentBranch.dialogs'에서 정보를 가져오도록 수정
    private IEnumerator OnTypingText()
    {
        int index = 0;
        isTypingEffect = true;
        
        string dialogueText = currentBranch.dialogs[currentDialogIndex].dialogue;
        characters[currentCharacterIndex].textDialogue.text = "";

        while (index < dialogueText.Length)
        {
            if (!isTypingEffect) 
            {
                yield break;
            }
            characters[currentCharacterIndex].textDialogue.text += dialogueText[index];
            index++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingEffect = false;
        characters[currentCharacterIndex].objectArrow.SetActive(true);
    }
}

// [수정] 명확성을 위해 Speaker 구조체 이름을 Character로 변경
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

// [추가] 선택지 데이터를 담을 구조체. 선택지 텍스트와, 이 선택지를 골랐을 때 넘어갈 다음 Branch의 '이름'을 가짐
[System.Serializable]
public struct Choice
{
    public string text;
    public string nextBranchName;
}

// [수정] DialogBranch 구조체에 선택지(Choice) 배열을 추가
[System.Serializable]
public struct DialogBranch
{
    public string branchName;
    public DialogDate[] dialogs;
    public Choice[] choices; // 이 Branch의 다이얼로그가 모두 끝난 후 표시될 선택지들
    public string autoNextBranchName;
}

[System.Serializable]
public struct DialogSystemGroup
{
    public string systemName;
    public DialogBranch[] branches;
}

// [추가] 선택지 UI 요소들을 관리하기 위한 구조체
[System.Serializable]
public struct SelectionUI
{
    public GameObject selectButtonPrefab;
    public Transform selectPanel;
}