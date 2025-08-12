using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Character))]
public class CharacterReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // 폴드아웃 스타일로 표시
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
            property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            float yOffset = EditorGUIUtility.singleLineHeight + 2;
            float currentY = position.y + yOffset;
            
            // Character Name
            SerializedProperty characterName = property.FindPropertyRelative("characterName");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                characterName, new GUIContent("Character Name"));
            currentY += yOffset;
            
            // Character ID
            SerializedProperty characterID = property.FindPropertyRelative("characterID");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                characterID, new GUIContent("Character ID"));
            currentY += yOffset;
            
            // Visual Components
            EditorGUI.LabelField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                "Visual Components", EditorStyles.boldLabel);
            currentY += yOffset;
            
            SerializedProperty spriteRenderer = property.FindPropertyRelative("spriteRenderer");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                spriteRenderer, new GUIContent("Sprite Renderer"));
            currentY += yOffset;
            
            SerializedProperty spineSkeletonAnimation = property.FindPropertyRelative("spineSkeletonAnimation");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                spineSkeletonAnimation, new GUIContent("Spine Skeleton Animation"));
            currentY += yOffset;
            
            // UI Components
            EditorGUI.LabelField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                "UI Components", EditorStyles.boldLabel);
            currentY += yOffset;
            
            SerializedProperty imageDialog = property.FindPropertyRelative("imageDialog");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                imageDialog, new GUIContent("Image Dialog"));
            currentY += yOffset;
            
            SerializedProperty textName = property.FindPropertyRelative("textName");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                textName, new GUIContent("Text Name"));
            currentY += yOffset;
            
            SerializedProperty textDialogue = property.FindPropertyRelative("textDialogue");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                textDialogue, new GUIContent("Text Dialogue"));
            currentY += yOffset;
            
            SerializedProperty objectArrow = property.FindPropertyRelative("objectArrow");
            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
                objectArrow, new GUIContent("Object Arrow"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight * 11 + 20; // 11개 필드 + 여백
        }
        return EditorGUIUtility.singleLineHeight;
    }
}
