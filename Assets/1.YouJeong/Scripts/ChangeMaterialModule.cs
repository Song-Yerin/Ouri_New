using UnityEngine;
using UnityEditor;

public class MaterialChangerEditor : EditorWindow
{
    public GameObject targetPrefab; // ������ ������
    public string materialName; // ��ü�� Material�� �̸�
    public Material newMaterial; // ���� ��ü�� Material

    [MenuItem("Tools/Material Changer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialChangerEditor>("Material Changer");
    }

    private void OnGUI()
    {
        // Ÿ�� ������, ��ü�� Material �̸�, �� Material�� �����ϴ� UI
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
            Debug.LogWarning("Prefab, Material �̸� �Ǵ� ���ο� Material�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // Prefab�� ������ �� �ֵ��� Prefab�� �ν��Ͻ��� �����ɴϴ�.
        Renderer renderer = targetPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            // ���� �����տ� ����� ��� Material �迭�� ������
            Material[] materials = renderer.sharedMaterials;

            bool materialFound = false;

            // ��� Material�� Ȯ���ϸ鼭 �̸��� ��ġ�ϴ� Material�� ��ü
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name == materialName)
                {
                    materials[i] = newMaterial; // �ش� Material�� ���ο� ������ ��ü
                    materialFound = true;
                    break; // Material�� ã���� �ݺ��� ����
                }
            }

            if (materialFound)
            {
                renderer.sharedMaterials = materials; // ����� �迭�� �ݿ�

                // Prefab�� ������ ���·� ����
                PrefabUtility.ApplyPrefabInstance(targetPrefab, InteractionMode.UserAction);

                // ����� Prefab�� '������' ǥ���Ͽ� Unity �����Ϳ� �ݿ�
                EditorUtility.SetDirty(targetPrefab);
                AssetDatabase.SaveAssets();  // ����� Asset ����

                Debug.Log($"Material '{materialName}'�� ���������� '{newMaterial.name}'�� ��ü�Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning($"Material '{materialName}'�� �����տ��� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("Ÿ�� �����տ� Renderer�� �����ϴ�.");
        }
    }
}


