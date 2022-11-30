using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// DISCLAIMER: This script is not fully finished.
// TODO: Add more path smoothing.
// TODO: Add multithreading.
// TODO: Combine PathfindingManager and Pathfinder > everything should be accessed via the MonoBehaviour. 
// TODO: Fix backtracking whenever an agent passes a node too quickly.
// TODO: Fix nodes spawning in walls when generating nodegrid from code. > Maybe remove all nodes that are not connected to the main grid.
// TODO: Add jumping down > nodeA connected to nodeB when condition is met, nodeB is not connected to nodeA. 
// TODO: Remove other astar pathfinding within jaimesutilities.
// TODO: Make ondrawgizmos more appealing.
// TODO: Heap optimization for finding path.
namespace JaimesUtilities.AStarManual
{ 
    public class Pathfinder 
    {
        private PathfindingSettings settings;
        private PathfindingNode[] nodes;

        public Pathfinder(PathfindingSettings settings) {
            this.settings = settings;
        }

        public void Setup(PathfindingNode[] nodes) {
            this.nodes = nodes;
        }

        public Path FindPath(Vector3 startPosition, Vector3 targetPosition) {
            Path path = new Path();

            PathfindingNode startNode = GetClosestNode(startPosition);
            PathfindingNode targetNode = GetClosestNode(targetPosition);

            if (startNode == null || targetNode == null) return path;

            HashSet<PathfindingNode> closetSet = new HashSet<PathfindingNode>();
            List<PathfindingNode> openSet = new List<PathfindingNode>();
            PathfindingNode current;

            openSet.Add(startNode);
            startNode.ResetValues();

            while (openSet.Count > 0) {
                current = GetLowestCostNode(openSet);

                if (current == targetNode) {
                    PathfindingNode[] pathNodes = RetraceNodes(current);

                    for (ushort i = 0; i < pathNodes.Length; i++) {
                        path.corners.Add(pathNodes[i].transform.position);
                    }

                    path.status = Path.Status.valid;
                    return path;
                }

                openSet.Remove(current);
                closetSet.Add(current);

                foreach (PathfindingNode node in current.links) {
                    if (closetSet.Contains(node) || openSet.Contains(node) || !node.enabled) continue;

                    Vector3 vectorToCurrent = (current.transform.position - node.transform.position);
                    Vector3 vectorToTarget = (node.transform.position - targetPosition);
                    vectorToCurrent.y *= 3f;
                    vectorToTarget.y *= 3f;

                    node.gCost = current.gCost + vectorToCurrent.magnitude;
                    node.hCost = vectorToTarget.magnitude;
                    node.parent = current;

                    openSet.Add(node);
                }
            }
            return path;
        }

        private PathfindingNode[] RetraceNodes(PathfindingNode finalNode) {
            List<PathfindingNode> nodes = new List<PathfindingNode>();
            PathfindingNode current = finalNode;

            while (current.parent != null) {
                nodes.Add(current);
                current = current.parent;
            }

            nodes.Reverse();
            if (settings.smoothenPath) nodes = SmoothenNodes(nodes);
            
            return nodes.ToArray();
        }

        // TODO/FIX: This function does not work on uneven ground > the previousdirection is always different when y changes slightly
        // TODO: Optimize this function!
        private List<PathfindingNode> SmoothenNodes(List<PathfindingNode> nodes) { 
            if (nodes.Count <= 2) return nodes;

            float yIncrement = settings.areaSize.y / Mathf.Ceil(settings.areaSize.y / settings.avrNodeDistance);
            System.Func<float, float> RecalculateY = (float y) => {
                return Mathf.Ceil(y / yIncrement);
            };

            Vector3 previousDirection0 = (nodes[nodes.Count - 1].transform.position - nodes[nodes.Count - 2].transform.position).normalized;
            previousDirection0.y = RecalculateY(previousDirection0.y);
            Vector3 previousDirection1 = previousDirection0;
            bool previousRemoved = false;

            for (int i = nodes.Count - 2; i > 0; i--) {
                Vector3 currentDirection = (nodes[i - 1].transform.position - nodes[i].transform.position).normalized;
                currentDirection.y = RecalculateY(currentDirection.y);

                if (!previousRemoved && currentDirection == previousDirection1) {
                    nodes.RemoveAt(i + 1);
                }
                else previousRemoved = false;

                if (currentDirection == previousDirection0) {
                    nodes.RemoveAt(i);
                    previousRemoved = true;
                }

                previousDirection1 = previousDirection0;
                previousDirection0 = currentDirection;
            }

            return nodes;
        }

        private PathfindingNode GetLowestCostNode(List<PathfindingNode> nodes) {
            PathfindingNode result = null;
            float lowestCost = float.MaxValue;

            foreach (PathfindingNode node in nodes) {
                if (!node.isEnabled) continue;

                float fCost = node.gCost + node.hCost;
                if (fCost > lowestCost) continue;

                lowestCost = fCost;
                result = node;
            }

            return result;
        }

        private PathfindingNode GetClosestNode(Vector3 position) {
            PathfindingNode result = null;
            float closestSqrDist = float.MaxValue;

            foreach (PathfindingNode node in nodes) {
                if (!node.isEnabled) continue;

                float sqrDist = (node.transform.position - position).sqrMagnitude;
                if (sqrDist > closestSqrDist) continue;

                closestSqrDist = sqrDist;
                result = node;
            }

            return result;
        }

        public void Bake() {
            List<PathfindingNode> nodesList = nodes.ToList();
            nodesList.ForEach(x => x.links.Clear());
            nodesList.ForEach(x => x.isEnabled = false);

            // Link Nodes
            HashSet<PathfindingNode> closedSet = new HashSet<PathfindingNode>();
            float maxDist = settings.maxNodeDistance;
            float maxAngl = settings.maxNodeAngle;
            float agentWidth = settings.agentWidth;
            LayerMask collideMask = settings.collideMask;

            foreach (PathfindingNode current in nodes) {
                closedSet.Add(current);

                Vector3 currentPos = current.transform.position;
                PathfindingNode[] nodesInRange = nodes.Where(x => (x.transform.position - currentPos).magnitude <= maxDist && !closedSet.Contains(x)).ToArray();

                foreach (PathfindingNode next in nodesInRange) {
                    Vector3 nextPos = next.transform.position;
                    
                    float angl = Mathf.Abs(90f - Vector3.Angle(nextPos - currentPos, Vector3.up));
                    if (angl > maxAngl) continue;

                    RaycastHit hitInfo;
                    if (Physics.Raycast(currentPos, (nextPos - currentPos).Horizontal(), out hitInfo, (nextPos - currentPos).Horizontal().magnitude, collideMask)) {
                        if (Vector3.Angle(hitInfo.normal, Vector3.up) > maxAngl) continue;
                    }
                    if (Physics.Raycast(nextPos, (currentPos - nextPos).Horizontal(), out hitInfo, (currentPos - nextPos).Horizontal().magnitude, collideMask)) {
                        if (Vector3.Angle(hitInfo.normal, Vector3.up) > maxAngl) continue;
                    }
                    if (Physics.CheckCapsule(currentPos, nextPos, agentWidth, settings.collideMask)) continue;

                    current.links.Add(next);
                    next.links.Add(current);
                }
            }

            // Enable largest link
            List<PathfindingNode[]> grids = new List<PathfindingNode[]>();
            Queue<PathfindingNode> openSet = new Queue<PathfindingNode>();

            while (nodesList.Count > 0) {
                PathfindingNode node = nodesList[0];

                closedSet.Clear();
                openSet.Clear();
                openSet.Enqueue(node);

                while (openSet.Count > 0) {
                    PathfindingNode current = openSet.Dequeue();
                    closedSet.Add(current);
                    nodesList.Remove(current);

                    foreach (PathfindingNode next in current.links) {
                        if (closedSet.Contains(next) || openSet.Contains(next)) continue;

                        openSet.Enqueue(next);
                    }
                }

                grids.Add(closedSet.ToArray());
            }

            int largestGridSize = 0;
            int largestGridIndex = 0;
            for (int i = 0; i < grids.Count; i++) {
                if (grids[i].Length < largestGridSize) continue;

                largestGridSize = grids[i].Length;
                largestGridIndex = i;
            }

            foreach (PathfindingNode node in grids[largestGridIndex]) node.isEnabled = true;
        }
    }

    // TODO: Add more functionality
    //   > NextCorner(); -or- Next(); 
    //   > CurrentCorner(); -or- (Vector3)Current { get; };
    //   > TargetPosition();
    //   > GetDirection(vector3 position); > automatically remove corner if too close!
    // TODO: Remove pathstatus > make FindPath a boolean with out arg Path. When invalid: FindPath returns false with out arg path null;
    public class Path
    {
        public enum Status { Invalid, valid }

        public List<Vector3> corners = new List<Vector3>();
        public Status status = Status.Invalid;

        public float Length {
            get {
                float result = 0;
                if (corners.Count == 0) return result;

                Vector3 prevV3 = corners[0];
                for (int i = 1; i < corners.Count; i++) {
                    result += (prevV3 - corners[i]).magnitude;
                    prevV3 = corners[i];
                }
                return result;
            }
        }
    }

    // TODO: Needs an onvalidate > some variables cause errors when given a certain value. (like nodeheight)
    // TODO: Clean up!
    [System.Serializable]
    public class PathfindingSettings
    {
        public enum ShowType { Full, OnlyNodes, Nothing }

        [Header("Base")]
        public LayerMask collideMask;
        [Range(1, 10)]
        public int maxNodeDistance = 3;
        [Range(0.1f, 5f)]
        public float nodeHeight = 1f;
        [Range(10, 70)]
        public float maxNodeAngle = 50;
        public float agentWidth = 0.5f;
        public ShowType showType = ShowType.OnlyNodes;
        public bool realtimeUpdate = false;
        public bool smoothenPath = false;

        [Header("AreaGeneration")]
        public bool viewAreaBounds;
        public Vector3 areaSize = new Vector3(10, 2, 10);
        public float avrNodeDistance = 1f;
        public bool centerArea = true;
    }
}
