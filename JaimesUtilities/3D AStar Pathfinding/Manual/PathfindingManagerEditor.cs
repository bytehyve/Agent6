using UnityEngine;
using UnityEditor;

namespace JaimesUtilities.AStarManual
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PathfindingManager))]
    public class PathfindingManagerEditor : Editor 
    {
        public override void OnInspectorGUI() {
            PathfindingManager target = this.target as PathfindingManager;

            base.OnInspectorGUI();
            
            GUILayout.Space(15);
            GUILayout.Label("Initialization");
            if (GUILayout.Button("Update nodes")) {
                target.OnValueUpdated();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Bake nodes")) {
                target.OnValueUpdated();
                target.pathfinder.Bake();
            }
            GUILayout.Space(15);
            GUILayout.Label("Nodes");
            if (GUILayout.Button("Create Node")) {
                target.CreateNode();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Clear nodes")) {
                target.ClearNodes();
            }
            GUILayout.Label("Generation");
            GUILayout.Space(15);
            if (GUILayout.Button("Generate nodes in area")) {
                target.GenerateNodesInArea(target.settings.areaSize, target.settings.avrNodeDistance, target.settings.centerArea, true);
            }
        }
    }
#endif
}