using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;

public class DialogSystemEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Database Info", "Dialog Systems" }; // UI References 탭 제거
    
    private DialogSystemDataBase targetDatabase;
    private SerializedObject serializedDatabase;
    
    private Dictionary<int, bool> systemFoldouts = new Dictionary<int, bool>();
    
    [MenuItem("Tools/Dialog System Editor")]
    public static void OpenWindow()
    {
        DialogSystemEditorWindow window = GetWindow<DialogSystemEditorWindow>("Dialog System Editor");
        window.minSize = new Vector2(500, 700);
    }
    
    private void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical();
            
            GUILayout.Space(10);
            
            // 데이터베이스 선택
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            
            targetDatabase = (DialogSystemDataBase)EditorGUILayout.ObjectField(
                "Dialog Database", targetDatabase, typeof(DialogSystemDataBase), false);
            
            if (GUILayout.Button("Create New Database"))
            {
                CreateNewDatabase();
            }
            EditorGUILayout.EndVertical();
            
            if (targetDatabase == null)
            {
                EditorGUILayout.HelpBox("Dialog Database를 선택하거나 새로 생성해주세요.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            serializedDatabase = new SerializedObject(targetDatabase);
            serializedDatabase.Update();
            
            GUILayout.Space(5);
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            GUILayout.Space(10);
            
            switch (selectedTab)
            {
                case 0:
                    DrawDatabaseInfoTab();
                    break;
                case 1:
                    DrawDialogSystemsTab();
                    break;
            }
            
            if (serializedDatabase != null)
                serializedDatabase.ApplyModifiedProperties();
            
            EditorGUILayout.EndVertical();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DialogSystemEditorWindow Error: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void DrawDatabaseInfoTab()
    {
        if (targetDatabase == null) return;
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Database Information", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Name: " + targetDatabase.name);
        EditorGUILayout.LabelField("Path: " + AssetDatabase.GetAssetPath(targetDatabase));
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Build Branch Database (Test)"))
        {
            targetDatabase.BuildBranchDatabase();
            Debug.Log($"Branch Database Built: {targetDatabase.BranchDatabase.Count} branches found.");
        }
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        
        int totalSystems = targetDatabase.systems?.Length ?? 0;
        int totalBranches = 0;
        int totalDialogs = 0;
        
        if (targetDatabase.systems != null)
        {
            foreach (var system in targetDatabase.systems)
            {
                if (system.branches != null)
                {
                    totalBranches += system.branches.Length;
                    foreach (var branch in system.branches)
                    {
                        if (branch.dialogs != null)
                            totalDialogs += branch.dialogs.Length;
                    }
                }
            }
        }
        
        EditorGUILayout.LabelField($"Systems: {totalSystems}");
        EditorGUILayout.LabelField($"Branches: {totalBranches}");
        EditorGUILayout.LabelField($"Dialogs: {totalDialogs}");
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        // UI 설정 안내
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("UI Setup Guide", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "UI 오브젝트들은 DialogSystem 컴포넌트의 인스펙터에서 직접 할당하세요:\n\n" +
            "1. DialogSystem 컴포넌트 선택\n" +
            "2. Character Parents에 PanelDialog, PanelGorillaDialog 등을 드래그\n" +
            "3. Selection Panel에 PanelSelectDialog를 드래그\n" +
            "4. Selection Button Prefab에 버튼 프리팹을 드래그", 
            MessageType.Info);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDialogSystemsTab()
    {
        if (serializedDatabase == null) return;
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        try
        {
            SerializedProperty systemsProp = serializedDatabase.FindProperty("systems");
            
            if (systemsProp == null)
            {
                EditorGUILayout.HelpBox("systems 프로퍼티를 찾을 수 없습니다.", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }
            
            // 상단 컨트롤
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dialog Systems", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Add System", GUILayout.Width(100)))
            {
                systemsProp.InsertArrayElementAtIndex(systemsProp.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 시스템들 표시
            for (int i = 0; i < systemsProp.arraySize; i++)
            {
                SerializedProperty system = systemsProp.GetArrayElementAtIndex(i);
                if (system == null) continue;
                
                SerializedProperty systemNameProp = system.FindPropertyRelative("systemName");
                SerializedProperty branchesProp = system.FindPropertyRelative("branches");
                
                if (!systemFoldouts.ContainsKey(i)) 
                    systemFoldouts[i] = false;
                
                EditorGUILayout.BeginVertical("box");
                
                // 시스템 헤더
                EditorGUILayout.BeginHorizontal();
                systemFoldouts[i] = EditorGUILayout.Foldout(systemFoldouts[i], 
                    $"System {i + 1}: {systemNameProp?.stringValue ?? "Unnamed"}", true);
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    systemsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                if (systemFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    
                    // 시스템 이름
                    if (systemNameProp != null)
                        EditorGUILayout.PropertyField(systemNameProp, new GUIContent("System Name"));
                    
                    GUILayout.Space(5);
                    
                    // 브랜치들
                    if (branchesProp != null)
                    {
                        EditorGUILayout.LabelField("Branches", EditorStyles.boldLabel);
                        
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        if (GUILayout.Button("Add Branch"))
                        {
                            branchesProp.InsertArrayElementAtIndex(branchesProp.arraySize);
                        }
                        if (GUILayout.Button("Remove Last Branch") && branchesProp.arraySize > 0)
                        {
                            branchesProp.DeleteArrayElementAtIndex(branchesProp.arraySize - 1);
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        GUILayout.Space(5);
                        
                        for (int j = 0; j < branchesProp.arraySize; j++)
                        {
                            SerializedProperty branch = branchesProp.GetArrayElementAtIndex(j);
                            if (branch != null)
                            {
                                EditorGUILayout.BeginVertical("helpbox");
                                EditorGUILayout.LabelField($"Branch {j}", EditorStyles.miniLabel);
                                
                                SerializedProperty branchNameProp = branch.FindPropertyRelative("branchName");
                                if (branchNameProp != null)
                                    EditorGUILayout.PropertyField(branchNameProp, new GUIContent("Branch Name"));
                                
                                SerializedProperty dialogsProp = branch.FindPropertyRelative("dialogs");
                                if (dialogsProp != null)
                                    EditorGUILayout.PropertyField(dialogsProp, new GUIContent("Dialogs"), true);
                                
                                SerializedProperty choicesProp = branch.FindPropertyRelative("choices");
                                if (choicesProp != null)
                                    EditorGUILayout.PropertyField(choicesProp, new GUIContent("Choices"), true);
                                
                                SerializedProperty autoNextProp = branch.FindPropertyRelative("autoNextBranchName");
                                if (autoNextProp != null)
                                    EditorGUILayout.PropertyField(autoNextProp, new GUIContent("Auto Next Branch"));
                                
                                EditorGUILayout.EndVertical();
                                GUILayout.Space(3);
                            }
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"Dialog Systems 탭 오류: {e.Message}", MessageType.Error);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateNewDatabase()
    {
        try
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Dialog Database", "New Dialog Database", "asset", "Create new dialog database asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                DialogSystemDataBase newDatabase = CreateInstance<DialogSystemDataBase>();
                
                // 기본 시스템 하나 생성
                newDatabase.systems = new DialogSystemGroup[1];
                newDatabase.systems[0] = new DialogSystemGroup
                {
                    systemName = "Default System",
                    branches = new DialogBranch[1]
                };
                newDatabase.systems[0].branches[0] = new DialogBranch
                {
                    branchName = "Default Branch",
                    dialogs = new DialogDate[1],
                    choices = new Choice[0],
                    autoNextBranchName = ""
                };
                newDatabase.systems[0].branches[0].dialogs[0] = new DialogDate
                {
                    speakerIndex = 0,
                    name = "Character Name",
                    dialogue = "Hello, this is a sample dialogue."
                };
                
                AssetDatabase.CreateAsset(newDatabase, path);
                AssetDatabase.SaveAssets();
                targetDatabase = newDatabase;
                Debug.Log($"새 데이터베이스가 생성되었습니다: {path}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터베이스 생성 오류: {e.Message}");
        }
    }
}