using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Spine.Unity;

public class DialogSystem : MonoBehaviour
{
    // [����] ������ dialogs, branches �迭�� ���ŵǰ�, �ý��� ��ü�� �����ϴ� systems �迭�� �����ϴ�.
    [SerializeField] private DialogSystemGroup[] systems;
    // [����] Speaker[] -> Character[] �̸� ���� �� ���� ��Ȯȭ
    [SerializeField] private Character[] characters;
    // [�߰�] ������ UI(��ư ������, �г�)�� �����ϱ� ���� ����ü
    [SerializeField] private SelectionUI selectionUI;

    // [�߰�] Branch �̸��� Key�� �Ͽ� ������ �����ϱ� ���� �����ͺ��̽�(Dictionary)
    private Dictionary<string, DialogBranch> branchDatabase = new Dictionary<string, DialogBranch>();
    private List<DialogBranch> orderedBranches = new List<DialogBranch>();

    // [�߰�] ��ü ��ȭ �帧�� Ȱ��ȭ ���¸� ����
    private bool isConversationActive = false;
    // [����] currentDialogIndex�� ���� ��ü�� �ƴ� '���� Branch ��'������ �ε����� �ǹ��մϴ�.
    private int currentDialogIndex = -1;
    private int currentCharacterIndex = 0;
    // [�߰�] ���� ���� ���� Branch �����͸� �����ϴ� ����
    private DialogBranch currentBranch;

    private float typingSpeed = 0.1f;
    private bool isTypingEffect = false;

    private void Awake()
    {
        Setup();
        // [�߰�] ������ ���۵� �� ��� Branch ������ �̸� �����ͺ��̽��� �����մϴ�.
        BuildBranchDatabase();
    }

    private void Setup()
    {
        // [����] speakers -> characters �迭�� ����ϵ��� ����
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
        // [�߰�] ������ �г��� �����ϸ� ��Ȱ��ȭ
        if (selectionUI.selectPanel != null)
        {
            selectionUI.selectPanel.gameObject.SetActive(false);
        }
    }

    // [�߰�] �����Ϳ� ������ ��� Branch�� Dictionary�� �����Ͽ� '�̸�'���� ������ ã�� �� �ֵ��� �غ��ϴ� �Լ�
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
                    Debug.LogWarning($"�ߺ��ǰų� ����ִ� Branch �̸��� �ֽ��ϴ�: '{branch.branchName}'. �� Branch�� ���õ˴ϴ�.");
                }
            }
        }
    }

    // [�߰�] DialogSystemTrigger���� ��ȭ�� �����ϱ� ���� ȣ���ϴ� ������ �Լ�
    public void StartConversation()
    {
        // [�߰�] Ȥ�� �� ������ ���濡 ����� �����ͺ��̽��� �ٽ� ����
        BuildBranchDatabase();

        // [����] ù ��° �ý����� ù ��° Branch�� �������� ��ȭ�� ����
        if (orderedBranches.Count > 0)
        {
            isConversationActive = true;
            StartBranch(orderedBranches[0].branchName);
        }
        else
        {
            Debug.LogError("DialogSystem�� ���ǵ� System �Ǵ� Branch�� �����ϴ�.");
            isConversationActive = false;
        }
    }

    // [�߰�] Ư�� �̸��� Branch�� ã�� ��ȭ�� �����ϴ� �Լ�
    private void StartBranch(string branchName)
    {
        // [�߰�] �����ͺ��̽����� branchName�� Key�� �Ͽ� �ش��ϴ� Branch ������ ã�� currentBranch�� �Ҵ�
        if (branchDatabase.TryGetValue(branchName, out currentBranch))
        {
            currentDialogIndex = -1; // �� Branch�� ���۵ǹǷ� ���̾�α� �ε��� �ʱ�ȭ
            SetNextDialog();
        }
        else
        {
            Debug.LogError($"'{branchName}' Branch�� ã�� �� �����ϴ�!");
            EndDialog();
        }
    }

    // [����] ��ȭ ���� ������ Branch ������� ���� ����
    public bool UpdateDialog()
    {
        if (!isConversationActive) return true; // ��ȭ�� ��Ȱ��ȭ ���¸� ����Ǿ����� Trigger�� �˸� (true ��ȯ)

        if (Input.GetMouseButtonDown(0))
        {
            // [�߰�] �������� Ȱ��ȭ�� ���¿����� ���콺 Ŭ������ ��ȭ�� �ѱ��� ����
            if (selectionUI.selectPanel != null && selectionUI.selectPanel.gameObject.activeSelf) return false;

            if (isTypingEffect)
            {
                isTypingEffect = false;
                StopCoroutine("OnTypingText");
                characters[currentCharacterIndex].textDialogue.text = currentBranch.dialogs[currentDialogIndex].dialogue;
                characters[currentCharacterIndex].objectArrow.SetActive(true);
                return false;
            }

            // [����] ���� Branch�� ���̾�αװ� �� �����ִ��� Ȯ��
            if (currentDialogIndex + 1 < currentBranch.dialogs.Length)
            {
                SetNextDialog();
            }
            else // [����] ���� Branch�� ������ ���̾�αװ� ������ ��
            {
                // [�߰�] ���� Branch�� �������� �ִ��� Ȯ��
                if (currentBranch.choices != null && currentBranch.choices.Length > 0)
                {
                    ShowChoices(); // ������ ǥ��
                }
                else if(!string.IsNullOrEmpty(currentBranch.autoNextBranchName))
                {
                    StartBranch(currentBranch.autoNextBranchName);
                }
                else
                {
                    // ���� Branch�� orderedBranches ����Ʈ�� �� ��°�� �ִ��� ã���ϴ�.
                    int currentIndex = orderedBranches.FindIndex(b => b.branchName == currentBranch.branchName);
                    
                    // ���� Branch�� ã�Ұ�, ������ Branch�� �ƴ϶��
                    if (currentIndex != -1 && currentIndex < orderedBranches.Count - 1)
                    {
                        // ���� ������ Branch�� �����մϴ�.
                        StartBranch(orderedBranches[currentIndex + 1].branchName);
                    }
                    else // ������ Branch�̰ų�, � �����ε� ����Ʈ���� ã�� ���ߴٸ�
                    {
                        EndDialog(); // ��ȭ�� �����մϴ�.
                    }
                }
            }
        }
        return !isConversationActive; // ��ȭ�� ���� ���� ���̸� false, EndDialog�� ��������� true ��ȯ
    }

    // [����] 'currentBranch'���� ���� ���̾�α� ������ ���������� ����
    private void SetNextDialog()
    {
        // ���� ���� �ε����� �ѱ�ϴ�.
        currentDialogIndex++;
        // �̹��� ���� ĳ���Ͱ� �������� �ε����� �����ɴϴ�.
        int newSpeakerIndex = currentBranch.dialogs[currentDialogIndex].speakerIndex;

        // ��� ĳ���͸� ��ȸ�ϸ鼭
        for (int i = 0; i < characters.Length; i++)
        {
            // ���� ��ȸ���� ĳ����(i)�� �̹��� ���� ĳ����(newSpeakerIndex)���� Ȯ���մϴ�.
            bool isActiveSpeaker = (i == newSpeakerIndex);
            // ���� ĳ�����̸� ��ȭâ�� �Ѱ�(true), �ƴ϶�� ���ϴ�(false).
            // �̷��� �ϸ� �׻� �� �� ���� ��ȭâ�� ������ ���� ����˴ϴ�.
            SetActiveObjects(characters[i], isActiveSpeaker);
        }

        // ���� ��縦 ���ϴ� ĳ������ �ε����� �����մϴ�.
        currentCharacterIndex = newSpeakerIndex;

        // �̸��� ��縦 �����ϰ� Ÿ���� ȿ���� �����մϴ�.
        characters[currentCharacterIndex].textName.text = currentBranch.dialogs[currentDialogIndex].name;
        StartCoroutine("OnTypingText");
    }

    // [�߰�] �������� ȭ�鿡 �������� �����ϰ� ǥ���ϴ� �Լ�
    private void ShowChoices()
    {
        if (currentCharacterIndex >= 0 && characters.Length > currentCharacterIndex)
        {
            SetActiveObjects(characters[currentCharacterIndex], false); // ������ ��ȭâ �����
        }

        selectionUI.selectPanel.gameObject.SetActive(true); // ������ �г� Ȱ��ȭ

        // ������ ������ ��ư�� �ִٸ� ��� ����
        foreach (Transform child in selectionUI.selectPanel)
        {
            Destroy(child.gameObject);
        }

        // ���� Branch�� ��� �������� ���� ��ư ����
        foreach (var choice in currentBranch.choices)
        {
            GameObject buttonGO = Instantiate(selectionUI.selectButtonPrefab, selectionUI.selectPanel);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            // �� ��ư�� Ŭ�� �̺�Ʈ�� ����. Ŭ�� �� OnChoiceSelected �Լ��� 'nextBranchName'�� �Բ� ȣ���
            buttonGO.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice.nextBranchName));
        }
    }

    // [�߰�] ������ ��ư�� Ŭ������ �� ȣ��Ǵ� �Լ�
    private void OnChoiceSelected(string nextBranchName)
    {
        selectionUI.selectPanel.gameObject.SetActive(false); // ������ �г� ��Ȱ��ȭ
        // [�߰�] ����� Branch �̸��� ��������� ��ȭ ����, ������ �ش� Branch ����
        if (string.IsNullOrEmpty(nextBranchName))
        {
            EndDialog();
        }
        else
        {
            StartBranch(nextBranchName);
        }
    }
    
    // [����] �Ű����� Ÿ���� Speaker���� Character�� ����
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
            color.a = visible ? 1f : 0.5f; // ��Ȱ�� ĳ���ʹ� �ణ �����ϰ� ó��
            character.spriteRenderer.color = color;
        }

        if(character.spineSkeletonAnimation != null && character.spineSkeletonAnimation.skeleton != null)
        {
            Color color = character.spineSkeletonAnimation.skeleton.GetColor();
            color.a = visible ? 1f : 0.5f;
            character.spineSkeletonAnimation.skeleton.SetColor(color);
        }
    }

    // [����] ��ȭ ���� �� ��� ĳ���� UI�� ��Ȱ��ȭ�ϰ� ���� �÷��׸� ����
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
        Debug.Log("��ȭ �帧 ����.");
    }
    
    // [����] 'dialogs' �迭 ��� 'currentBranch.dialogs'���� ������ ���������� ����
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

// [����] ��Ȯ���� ���� Speaker ����ü �̸��� Character�� ����
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

// [�߰�] ������ �����͸� ���� ����ü. ������ �ؽ�Ʈ��, �� �������� ����� �� �Ѿ ���� Branch�� '�̸�'�� ����
[System.Serializable]
public struct Choice
{
    public string text;
    public string nextBranchName;
}

// [����] DialogBranch ����ü�� ������(Choice) �迭�� �߰�
[System.Serializable]
public struct DialogBranch
{
    public string branchName;
    public DialogDate[] dialogs;
    public Choice[] choices; // �� Branch�� ���̾�αװ� ��� ���� �� ǥ�õ� ��������
    public string autoNextBranchName;
}

[System.Serializable]
public struct DialogSystemGroup
{
    public string systemName;
    public DialogBranch[] branches;
}

// [�߰�] ������ UI ��ҵ��� �����ϱ� ���� ����ü
[System.Serializable]
public struct SelectionUI
{
    public GameObject selectButtonPrefab;
    public Transform selectPanel;
}