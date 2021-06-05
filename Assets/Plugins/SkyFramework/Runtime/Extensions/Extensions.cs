// This class adds functions to built-in types.
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace SkyFrameWork
{
    public static class Extensions
    {
        // string to int 
        public static int ToInt(this string value, int errVal = 0)
        {
            Int32.TryParse(value, out errVal);
            return errVal;
        }

        // string to long
        public static long ToLong(this string value, long errVal = 0)
        {
            Int64.TryParse(value, out errVal);
            return errVal;
        }

        // UI SetListener扩展，删除以前的侦听器，然后添加新的侦听器
        //（此版本用于onClick等）
        public static void SetListener(this UnityEvent uEvent, UnityAction call)
        {
            uEvent.RemoveAllListeners();
            uEvent.AddListener(call);
        }

        // UI SetListener扩展，删除以前的侦听器，然后添加新的侦听器
        //（此版本适用于onededit、onValueChanged等）
        public static void SetListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call)
        {
            uEvent.RemoveAllListeners();
            uEvent.AddListener(call);
        }

        //NavMeshAgent帮助函数，返回指定目的地。这对click&wsad移动非常有用
        //因为玩家可能会点击进入各种无法行走的路径：
        //       ________
        //      |xxxxxxx|
        //      |x|   |x|
        // P   A|B| C |x|
        //      |x|___|x|
        //      |xxxxxxx|
        //
        // 如果player在位置P并试图去：
        // A:这条路可以走，一切都很好
        // C:C在导航网上，但我们不能直接到达那里。CalulatePath将返回A作为最后一个可行走点
        // B:B不在NavMesh上，CalulatePath在这里不起作用。我们需要查找NavMesh上最近的点（可能是a或C），
        // 然后计算最接近的有效值（A）
        public static Vector3 NearestValidDestination(this NavMeshAgent agent, Vector3 destination)
        {
            // 能计算出那里的路径吗？然后返回最近的有效点
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(destination, path))
                return path.corners[path.corners.Length - 1];

            // 否则，请先找到最近的navmesh位置。我们用速度*2为半径
            // 效果很好。然后找到最近的有效点。
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, agent.speed * 2, NavMesh.AllAreas))
                if (agent.CalculatePath(hit.position, path))
                    return path.corners[path.corners.Length - 1];

            // 什么都没用，哪儿也别去。
            return agent.transform.position;
        }

        // 需要一个真正停止所有运动的方法。
        public static void ResetMovement(this NavMeshAgent agent)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // 检查列表是否有重复项
        public static bool HasDuplicates<T>(this List<T> list)
        {
            return list.Count != list.Distinct().Count();
        }

        // 查找列表中的所有重复项
        // 注意：这只在开始时调用一次，所以Linq在这里很好！
        public static List<U> FindDuplicates<T, U>(this List<T> list, Func<T, U> keySelector)
        {
            return list.GroupBy(keySelector)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key).ToList();
        }

        // string.GetHashCode 不能保证在所有机器上都是相同的，但是
        // 我们需要一个在所有机器上都一样的。这是一个简单的方法：
        public static int GetStableHashCode(this string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }
        
        public static void SetObjectActive(this Component target, bool active)
        {
            target.gameObject.SetActive(active);
        }
        
        public static void SetTransformParent(this Component target,Component parent,bool worldPositionStays = false)
        {
            target.transform.SetParent(parent.transform,worldPositionStays);
        }

        public static bool IsEmpty(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static T ToEnum<T>(this string value) where T : Enum
        {
            if (string.IsNullOrEmpty(value)) return (T) Enum.GetValues(typeof(T)).GetValue(0);

            return (T) Enum.Parse(typeof(T), value);
        }

        public static T Random<T>(this T[] source)
        {
            if (source == null || source.Length == 0) return default;

            return source[UnityEngine.Random.Range(0, source.Length)];
        }

        public static T Random<T>(this List<T> source)
        {
            if (source == null || source.Count == 0) return default;

            return source[UnityEngine.Random.Range(0, source.Count)];
        }

        public static T Random<T>(this List<T> source, int seed)
        {
            UnityEngine.Random.InitState(seed);

            return source[UnityEngine.Random.Range(0, source.Count)];
        }
        
        
        public static void KeepInScreen(this RectTransform rectTransform)
        {
            Rect rect = rectTransform.rect;
            // to 世界坐标
            Vector2 minWorld = rectTransform.TransformPoint(rect.min);
            Vector2 maxWorld = rectTransform.TransformPoint(rect.max);
            Vector2 sizeWorld = maxWorld - minWorld;

            // 保持最小位置在屏幕边界
            // 限制位置在 (0,0) - maxWorld
            maxWorld = new Vector2(Screen.width, Screen.height) - sizeWorld;
            float x = Mathf.Clamp(minWorld.x, 0, maxWorld.x);
            float y = Mathf.Clamp(minWorld.y, 0, maxWorld.y);

            // 设置tip的坐标位置
            Vector2 offset = (Vector2)rectTransform.position - minWorld;
            rectTransform.position = new Vector2(x, y) + offset;
        }
        
        /// <summary>
        /// 运算符匹配规则
        /// </summary>
        private const string RICH_PATTERN = @"<([\s\S]*?)>";

        /// <summary>
        /// 截取字符串，不限制字符串长度
        /// </summary>
        /// <param name="str">待截取的字符串</param>
        /// <param name="len">每行的长度，多于这个长度自动换行</param>
        /// <param name="curStr"></param>
        /// <returns></returns>
        public static string CutStr(this string str,int len,string curStr)
        {  
            StringBuilder result = new StringBuilder();
            for(int i=0;i<str.Length ;i++)
            {
                int r= i% len;
                int last =(str.Length/len)*len;
                if (i!=0 && i<=last)
                {
                    if( r==0)
                    {
                        result.Append(str.Substring(i - len, len));
                        result.Append(curStr);
                    }
                }
                else if (i>last)
                {
                    result.Append(str.Substring(i-1));
                    break;
                }
            }
            return result.ToString();
        }
        
        public static void ForceRebuildLayoutImmediate(this RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}