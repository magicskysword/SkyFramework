using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkyFrameWork
{
    public static class PathingFinding
    {
        public class Node
        {
            public Vector2Int pos;
            public float pathConsume;
            public Node parent;
        }

        /// <summary>
        /// 生成四向寻路
        /// </summary>
        /// <param name="map">地图格子与行走消耗</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns></returns>
        public static Vector2Int[] Generate4XPath(Dictionary<Vector2Int, int> map, Vector2Int start, Vector2Int end)
        {
            Dictionary<Vector2Int, Node> openDic = new Dictionary<Vector2Int, Node>();
            Dictionary<Vector2Int, Node> closeDic = new Dictionary<Vector2Int, Node>();
            Node startNode = new Node()
            {
                pos = start,
                pathConsume = 0,
                parent = null,
            };
            openDic.Add(startNode.pos, startNode);

            do
            {
                // 寻找最短消耗节点
                Node curNode = openDic.First().Value;
                foreach (var kvp in openDic)
                {
                    if (kvp.Value.pathConsume < curNode.pathConsume)
                        curNode = kvp.Value;
                }

                // 将当前节点转入closeDic
                openDic.Remove(curNode.pos);
                closeDic.Add(curNode.pos, curNode);

                // 检测当前节点是否为终点
                if (curNode.pos == end)
                    break;

                // 添加周围节点
                Vector2Int curPos = curNode.pos;
                Vector2Int tagPos;
                tagPos = new Vector2Int(curPos.x + 1, curPos.y);
                FindNode(map, start, end, tagPos, openDic, closeDic, curNode);
                tagPos = new Vector2Int(curPos.x, curPos.y + 1);
                FindNode(map, start, end, tagPos, openDic, closeDic, curNode);
                tagPos = new Vector2Int(curPos.x - 1, curPos.y);
                FindNode(map, start, end, tagPos, openDic, closeDic, curNode);
                tagPos = new Vector2Int(curPos.x, curPos.y - 1);
                FindNode(map, start, end, tagPos, openDic, closeDic, curNode);
            } while (openDic.Count > 0);

            if (closeDic.TryGetValue(end, out var endNode))
            {
                List<Vector2Int> path = new List<Vector2Int>();
                do
                {
                    path.Add(endNode.pos);
                    endNode = endNode.parent;
                } while (endNode != null);

                path.Reverse();
                return path.ToArray();
            }
            else
            {
                return new Vector2Int[0];
            }
        }

        private static void FindNode(Dictionary<Vector2Int, int> map, Vector2Int start, Vector2Int end,
            Vector2Int tagPos, Dictionary<Vector2Int, Node> openDic,
            Dictionary<Vector2Int, Node> closeDic, Node curNode)
        {
            if (map.TryGetValue(tagPos, out var consume) && !openDic.ContainsKey(tagPos) &&
                !closeDic.ContainsKey(tagPos))
            {
                openDic.Add(tagPos, new Node()
                {
                    pos = tagPos,
                    pathConsume = Mathf.Abs(tagPos.x - start.x)
                                  + Mathf.Abs(tagPos.y - start.y)
                                  + Mathf.Abs(tagPos.x - end.x)
                                  + Mathf.Abs(tagPos.x - end.x)
                                  + consume,
                    parent = curNode,
                });
            }
        }

        /// <summary>
        /// 生成四向路径范围
        /// </summary>
        /// <param name="map"></param>
        /// <param name="start"></param>
        /// <param name="actionPoint"></param>
        /// <returns></returns>
        public static Vector2Int[] Generate4XRange(Dictionary<Vector2Int, int> map, Vector2Int start, int actionPoint)
        {
            // 存放所有已经搜寻过的节点
            Dictionary<Vector2Int, Node> rangeDic = new Dictionary<Vector2Int, Node>();
            Queue<Node> findQueue = new Queue<Node>();
            List<Vector2Int> closeList = new List<Vector2Int>();

            foreach (var pos in map)
            {
                var node = new Node()
                {
                    pos = pos.Key,
                    pathConsume = 0,
                    parent = null
                };
                rangeDic.Add(pos.Key, node);
            }

            findQueue.Enqueue(rangeDic[start]);

            while (findQueue.Count > 0)
            {
                Node curNode = findQueue.Dequeue();
                Vector2Int curPos = curNode.pos;
                // 添加周围节点
                Vector2Int tagPos;
                tagPos = new Vector2Int(curPos.x + 1, curPos.y);
                FindReachNode(map, actionPoint, rangeDic, findQueue, closeList, tagPos, curNode);
                tagPos = new Vector2Int(curPos.x, curPos.y + 1);
                FindReachNode(map, actionPoint, rangeDic, findQueue, closeList, tagPos, curNode);
                tagPos = new Vector2Int(curPos.x - 1, curPos.y);
                FindReachNode(map, actionPoint, rangeDic, findQueue, closeList, tagPos, curNode);
                tagPos = new Vector2Int(curPos.x, curPos.y - 1);
                FindReachNode(map, actionPoint, rangeDic, findQueue, closeList, tagPos, curNode);

                closeList.Add(curPos);
            }

            return closeList.ToArray();
        }

        private static void FindReachNode(Dictionary<Vector2Int, int> map, int actionPoint,
            Dictionary<Vector2Int, Node> rangeDic,
            Queue<Node> findQueue,
            List<Vector2Int> closeList,
            Vector2Int tagPos, Node curNode)
        {
            if (map.TryGetValue(tagPos, out var consume))
            {
                Node tagNode = rangeDic[tagPos];
                if (!findQueue.Contains(tagNode)
                    && !closeList.Contains(tagPos)
                    && consume + curNode.pathConsume <= actionPoint)
                {
                    tagNode.parent = curNode;
                    tagNode.pathConsume = consume + curNode.pathConsume;
                    findQueue.Enqueue(tagNode);
                }
            }

        }
    }

}