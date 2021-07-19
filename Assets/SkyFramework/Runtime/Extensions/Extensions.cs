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

namespace SkyFramework
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
        
        /// <summary>
        /// 检查列表是否有重复项
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasDuplicates<T>(this List<T> list)
        {
            return list.Count != list.Distinct().Count();
        }

        /// <summary>
        /// 查找列表中的所有重复项
        /// </summary>
        /// <param name="list"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public static List<U> FindDuplicates<T, U>(this List<T> list, Func<T, U> keySelector)
        {
            return list.GroupBy(keySelector)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key).ToList();
        }

        /// <summary>
        /// 获取稳定的HashCode
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        public static T Random<T>(this IList<T> source)
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

        public static void ForceRebuildLayoutImmediate(this RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}