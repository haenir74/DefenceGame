using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

namespace DungeonDefence.Editor
{
    public class ReferenceDependencyExporter
    {
        [MenuItem("Tools/Export Reference Map")]
        public static void ExportReferenceMap()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Source_Path,Source_Component,Field_Name,Target_Name,Target_Type,Target_Path_Or_Status");

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            foreach (var go in rootObjects)
            {
                ProcessGameObject(go, sb);
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string docsPath = Path.Combine(projectRoot, "docs", "architecture");
            
            if (!Directory.Exists(docsPath))
            {
                Directory.CreateDirectory(docsPath);
            }

            string filePath = Path.Combine(docsPath, "scene_reference_map.csv");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            Debug.Log($"<color=green>Scene reference map exported successfully to: {filePath}</color>");
        }

        private static void ProcessGameObject(GameObject go, StringBuilder sb)
        {
            string sourcePath = GetGameObjectPath(go);
            Component[] components = go.GetComponents<Component>();

            foreach (var comp in components)
            {
                if (comp == null) continue;

                string sourceComponent = comp.GetType().Name;
                SerializedObject so = new SerializedObject(comp);
                SerializedProperty prop = so.GetIterator();

                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = true;

                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        string fieldName = prop.name;
                        string targetName = "";
                        string targetType = "";
                        string targetStatus = "NULL";

                        Object refTarget = prop.objectReferenceValue;
                        int instanceID = prop.objectReferenceInstanceIDValue;

                        if (refTarget != null)
                        {
                            targetName = refTarget.name.Replace(",", " ");
                            targetType = refTarget.GetType().Name;
                            string assetPath = AssetDatabase.GetAssetPath(refTarget);

                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                targetStatus = assetPath;
                            }
                            else
                            {
                                targetStatus = "SCENE_OBJECT";
                            }
                        }
                        else
                        {
                            if (instanceID != 0)
                            {
                                targetStatus = "MISSING";
                            }
                            else
                            {
                                targetStatus = "NULL";
                            }
                        }

                        string safeSourcePath = EscapeCSV(sourcePath);
                        string safeComponent = EscapeCSV(sourceComponent);
                        string safeField = EscapeCSV(fieldName);
                        string safeTargetName = EscapeCSV(targetName);
                        string safeTargetType = EscapeCSV(targetType);
                        string safeTargetStatus = EscapeCSV(targetStatus);

                        sb.AppendLine($"{safeSourcePath},{safeComponent},{safeField},{safeTargetName},{safeTargetType},{safeTargetStatus}");
                    }
                }
            }

            foreach (Transform child in go.transform)
            {
                ProcessGameObject(child.gameObject, sb);
            }
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static string EscapeCSV(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\n") || str.Contains("\r"))
            {
                return "\"" + str.Replace("\"", "\"\"") + "\"";
            }
            return str;
        }
    }
}
