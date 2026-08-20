using AudioFramework;
using UnityEditor;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    SerializedProperty localCatalog;
    SerializedProperty remoteCatalog;
    SerializedProperty loadLocationEnum;
    SerializedProperty mixer;
    SerializedProperty sfxSourceCount;

    private void OnEnable()
    {
        localCatalog = serializedObject.FindProperty("_audioCatalog");
        remoteCatalog = serializedObject.FindProperty("_audioCatalogAdressables");
        loadLocationEnum = serializedObject.FindProperty("loadLocation");
        mixer = serializedObject.FindProperty("_mainMixer");
        sfxSourceCount = serializedObject.FindProperty("_sfxSourcesCount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(loadLocationEnum);

        if (loadLocationEnum.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(localCatalog);
        }
        else
        {
            EditorGUILayout.PropertyField(remoteCatalog);
        }

        EditorGUILayout.PropertyField(mixer);
        EditorGUILayout.PropertyField(sfxSourceCount);
        serializedObject.ApplyModifiedProperties();
    }
}
