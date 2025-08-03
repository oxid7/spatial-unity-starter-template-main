using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UniqueMaterialFinder : EditorWindow
{
    private Vector2 scrollPos;
    private List<Material> uniqueMaterials = new List<Material>();

    [MenuItem("Tools/Find Unique Materials In Scene")]
    public static void ShowWindow()
    {
        GetWindow<UniqueMaterialFinder>("Unique Materials Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Scene for Unique Materials"))
        {
            ScanSceneForMaterials();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Unique Materials in Scene:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var mat in uniqueMaterials)
        {
            EditorGUILayout.ObjectField(mat, typeof(Material), false);
        }
        EditorGUILayout.EndScrollView();
    }

    private void ScanSceneForMaterials()
    {
        uniqueMaterials.Clear();
        HashSet<Material> foundMaterials = new HashSet<Material>();

        // Find all renderers in the scene
        var renderers = GameObject.FindObjectsOfType<Renderer>();

        foreach (var rend in renderers)
        {
            foreach (var mat in rend.sharedMaterials)
            {
                if (mat != null)
                    foundMaterials.Add(mat);
            }
        }

        uniqueMaterials = foundMaterials.ToList();
        Debug.Log($"Found {uniqueMaterials.Count} unique material(s) in the scene.");
    }
}
