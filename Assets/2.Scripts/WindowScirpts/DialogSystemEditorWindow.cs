using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spine.Unity;

public class DialogSystemEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;

    private int selectedTab = 0;
    private string[] tabNames = { "UI_Dialog", "Branch", "Trigger" };

    private DialogSystem targetDialogSystem;
    private DialogSystemTrigger targetDialogTrigger;

    private Dictionary<int, bool> systemFoldouts = new Dictionary<int, bool>();
    private Dictionary<(int, int), bool> branchFoldouts = new Dictionary<(int, int), bool>();
    private SerializedObject serializedObject;

    [MenuItem("Tools/Dialog System Editor")]
    public static void OpenWindow()
    {
        GetWindow<DialogSystemEditorWindow>("Dialog System Editor");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        targetDialogSystem = (DialogSystem)EditorGUILayout.ObjectField("Target Dialog System", targetDialogSystem, typeof(DialogSystem), true);
        targetDialogTrigger = (DialogSystemTrigger)EditorGUILayout.ObjectField("Target Dialog Trigger", targetDialogTrigger, typeof(DialogSystemTrigger), true);

        if (targetDialogSystem == null)
        {
            EditorGUILayout.HelpBox("Dialog System?ùÑ Î®ºÏ?? Ïß??†ï?ï¥Ï£ºÏÑ∏?öî.", MessageType.Info);
            return;
        }

        serializedObject = new SerializedObject(targetDialogSystem);
        serializedObject.Update();

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        GUILayout.Space(10);

        switch (selectedTab)
        {
            case 0:
                DrawUIDialogTab();
                break;
            case 1:
                DrawSystemBranchDialogTab();
                break;
            case 2:
                DrawTriggerTab();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUIDialogTab()
    {
        EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
        SerializedProperty charactersProperty = serializedObject.FindProperty("characters");
        
        // Show array size
        int arraySize = charactersProperty.arraySize;
        int newSize = EditorGUILayout.IntField("Size", arraySize);
        if (newSize != arraySize)
        {
            charactersProperty.arraySize = newSize;
        }

        // Show each character element with expanded fields
        for (int i = 0; i < charactersProperty.arraySize; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);
            
            SerializedProperty characterElement = charactersProperty.GetArrayElementAtIndex(i);
            EditorGUI.indentLevel++;
            
            // Show individual fields
            SerializedProperty spriteRendererProp = characterElement.FindPropertyRelative("spriteRenderer");
            SerializedProperty spineSkeletonAnimationProp = characterElement.FindPropertyRelative("spineSkeletonAnimation");
            SerializedProperty imageDialogProp = characterElement.FindPropertyRelative("imageDialog");
            SerializedProperty textNameProp = characterElement.FindPropertyRelative("textName");
            SerializedProperty textDialogueProp = characterElement.FindPropertyRelative("textDialogue");
            SerializedProperty objectArrowProp = characterElement.FindPropertyRelative("objectArrow");
            
            EditorGUILayout.PropertyField(spriteRendererProp, new GUIContent("Sprite Renderer"));
            EditorGUILayout.PropertyField(spineSkeletonAnimationProp, new GUIContent("Spine Skeleton Animation"));
            EditorGUILayout.PropertyField(imageDialogProp, new GUIContent("Image Dialog"));
            EditorGUILayout.PropertyField(textNameProp, new GUIContent("Text Name"));
            EditorGUILayout.PropertyField(textDialogueProp, new GUIContent("Text Dialog"));
            EditorGUILayout.PropertyField(objectArrowProp, new GUIContent("Object Arrow"));
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(20);

        EditorGUILayout.LabelField("Select", EditorStyles.boldLabel);
        SerializedProperty selectionUIProperty = serializedObject.FindProperty("selectionUI");
        EditorGUILayout.PropertyField(selectionUIProperty, new GUIContent("Selection UI"), true);
    }

    private void DrawSystemBranchDialogTab()
    {
        EditorGUILayout.LabelField("Systems", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        SerializedProperty systemsProp = serializedObject.FindProperty("systems");
        for (int i = 0; i < systemsProp.arraySize; i++)
        {
            SerializedProperty system = systemsProp.GetArrayElementAtIndex(i);
            SerializedProperty systemNameProp = system.FindPropertyRelative("systemName");
            SerializedProperty branchesProp = system.FindPropertyRelative("branches");

            if (!systemFoldouts.ContainsKey(i)) systemFoldouts[i] = true;
            systemFoldouts[i] = EditorGUILayout.Foldout(systemFoldouts[i], $"System {i + 1} : {systemNameProp.stringValue}", true);
            
            if(systemFoldouts[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(systemNameProp, new GUIContent("System Name"));

                for(int j = 0; j < branchesProp.arraySize; j++)
                {
                    SerializedProperty branch = branchesProp.GetArrayElementAtIndex(j);
                    SerializedProperty branchNameProp = branch.FindPropertyRelative("branchName");
                    
                    var branchKey = (i, j);
                    if (!branchFoldouts.ContainsKey(branchKey)) branchFoldouts[branchKey] = true;

                    branchFoldouts[branchKey] = EditorGUILayout.Foldout(branchFoldouts[branchKey], $"Branch {j + 1} : {branchNameProp.stringValue}", true);
                
                    if(branchFoldouts[branchKey])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(branchNameProp, new GUIContent("Branch Name"));
                        
                        SerializedProperty branchDialogsProp = branch.FindPropertyRelative("dialogs");
                        EditorGUILayout.PropertyField(branchDialogsProp, new GUIContent("Dialogs"), true);
                        
                        EditorGUILayout.Space(5);

                        EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
                        SerializedProperty choicesProp = branch.FindPropertyRelative("choices");
                        EditorGUILayout.PropertyField(choicesProp, new GUIContent("Choices"), true);

                        
                        EditorGUI.indentLevel--;
                    }
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Add Branch"))
                {
                    branchesProp.InsertArrayElementAtIndex(branchesProp.arraySize);
                }
                if (GUILayout.Button("Remove Last Branch"))
                {
                    if (branchesProp.arraySize > 0)
                        branchesProp.DeleteArrayElementAtIndex(branchesProp.arraySize - 1);
                }
                EditorGUI.indentLevel--;
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Add System"))
        {
            systemsProp.InsertArrayElementAtIndex(systemsProp.arraySize);
        }
        if (GUILayout.Button("Remove Last System"))
        {
            if (systemsProp.arraySize > 0)
                systemsProp.DeleteArrayElementAtIndex(systemsProp.arraySize - 1);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTriggerTab()
    {
        if (targetDialogTrigger == null)
        {
            EditorGUILayout.HelpBox("Dialog System TriggerÎ•? Î®ºÏ?? Ïß??†ï?ï¥Ï£ºÏÑ∏?öî.", MessageType.Info);
            return;
        }
        SerializedObject triggerSerializedObject = new SerializedObject(targetDialogTrigger);
        triggerSerializedObject.Update();

        EditorGUILayout.LabelField("Dialog Systems", EditorStyles.boldLabel);
        
        SerializedProperty dialogSystemsProp = triggerSerializedObject.FindProperty("dialogSystems");
        EditorGUILayout.PropertyField(dialogSystemsProp, new GUIContent("Systems Queue"), true);
        
        triggerSerializedObject.ApplyModifiedProperties();
    }
}