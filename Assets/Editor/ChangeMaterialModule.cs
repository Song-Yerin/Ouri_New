using UnityEngine;
using UnityEditor;

public class MaterialChangerEditor : EditorWindow
{
    public GameObject targetPrefab; // 변경할 프리팹
    public string materialName; // 교체할 Material의 이름
    public Material newMaterial; // 새로 교체할 Material

    [MenuItem("Tools/Material Changer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialChangerEditor>("Material Changer");
    }

    private void OnGUI()
    {
        // 타겟 프리팹, 교체할 Material 이름, 새 Material을 지정하는 UI
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Prefab", targetPrefab, typeof(GameObject), false);
        materialName = EditorGUILayout.TextField("Material Name", materialName);
        newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), false);

        if (GUILayout.Button("Change Material"))
        {
            ChangeMaterialInPrefab();
        }
    }

    private void ChangeMaterialInPrefab()
    {
        if (targetPrefab == null || string.IsNullOrEmpty(materialName) || newMaterial == null)
        {
            Debug.LogWarning("Prefab, Material 이름 또는 새로운 Material이 지정되지 않았습니다.");
            return;
        }

        // Prefab을 수정할 수 있도록 Prefab의 인스턴스를 가져옵니다.
        Renderer renderer = targetPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 현재 프리팹에 적용된 모든 Material 배열을 가져옴
            Material[] materials = renderer.sharedMaterials;

            bool materialFound = false;

            // 모든 Material을 확인하면서 이름이 일치하는 Material을 교체
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name == materialName)
                {
                    materials[i] = newMaterial; // 해당 Material을 새로운 것으로 교체
                    materialFound = true;
                    break; // Material을 찾으면 반복문 종료
                }
            }

            if (materialFound)
            {
                renderer.sharedMaterials = materials; // 변경된 배열을 반영

                // Prefab을 수정된 상태로 저장
                PrefabUtility.ApplyPrefabInstance(targetPrefab, InteractionMode.UserAction);

                // 변경된 Prefab을 '더럽게' 표시하여 Unity 에디터에 반영
                EditorUtility.SetDirty(targetPrefab);
                AssetDatabase.SaveAssets();  // 변경된 Asset 저장

                Debug.Log($"Material '{materialName}'이 성공적으로 '{newMaterial.name}'로 교체되었습니다.");
            }
            else
            {
                Debug.LogWarning($"Material '{materialName}'을 프리팹에서 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("타겟 프리팹에 Renderer가 없습니다.");
        }
    }
}


