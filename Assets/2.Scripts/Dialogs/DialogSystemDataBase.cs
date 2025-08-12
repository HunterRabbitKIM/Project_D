using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Spine.Unity;

[CreateAssetMenu(fileName = "New Dialog Database", menuName = "Dialog System/Dialog Database")]
public class DialogSystemDataBase : ScriptableObject
{
    [Header("Dialog Systems")]
    public DialogSystemGroup[] systems;
    
    // 런타임에서 사용할 Branch 데이터베이스
    [System.NonSerialized]
    private Dictionary<string, DialogBranch> branchDatabase = new Dictionary<string, DialogBranch>();
    [System.NonSerialized]
    private List<DialogBranch> orderedBranches = new List<DialogBranch>();
    
    public Dictionary<string, DialogBranch> BranchDatabase => branchDatabase;
    public List<DialogBranch> OrderedBranches => orderedBranches;
    
    public void BuildBranchDatabase()
    {
        branchDatabase.Clear();
        orderedBranches.Clear();
        
        if (systems == null) return;
        
        foreach (var system in systems)
        {
            if (system.branches == null) continue;
            
            foreach (var branch in system.branches)
            {
                if (!string.IsNullOrEmpty(branch.branchName) && !branchDatabase.ContainsKey(branch.branchName))
                {
                    branchDatabase.Add(branch.branchName, branch);
                    orderedBranches.Add(branch);
                }
                else if (!string.IsNullOrEmpty(branch.branchName))
                {
                    Debug.LogWarning($"중복되거나 빈 Branch 이름이 있습니다: '{branch.branchName}'. 이 Branch는 무시됩니다.");
                }
            }
        }
    }
}

[System.Serializable]
public struct CharacterUIReference
{
    [Header("Character Info")]
    public string characterName;
    public string characterID;
    
    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;
    public SkeletonMecanim spineSkeletonAnimation;
    
    [Header("UI Components")]
    public Image imageDialog;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDialogue;
    public GameObject objectArrow;
    
    public Character ToRuntimeCharacter()
    {
        Character character = new Character();
        character.spriteRenderer = spriteRenderer;
        character.spineSkeletonAnimation = spineSkeletonAnimation;
        character.imageDialog = imageDialog;
        character.textName = textName;
        character.textDialogue = textDialogue;
        character.objectArrow = objectArrow;
        return character;
    }
}

[System.Serializable]
public struct SelectionUIReference
{
    [Header("Selection UI Components")]
    public GameObject selectButtonPrefab;
    public Transform selectPanel;
    
    public SelectionUI ToRuntimeSelectionUI()
    {
        SelectionUI selectionUI = new SelectionUI();
        selectionUI.selectButtonPrefab = selectButtonPrefab;
        selectionUI.selectPanel = selectPanel;
        return selectionUI;
    }
}