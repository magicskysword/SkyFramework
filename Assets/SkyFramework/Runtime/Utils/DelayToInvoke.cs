using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace SkyFramework
{
    public class DelayToInvoke
    {
        //内部类
        private class TaskBehaviour : MonoBehaviour
        {
            private event UnityAction updateEvent;

            private void Update()
            {
                updateEvent?.Invoke();
            }

            public void AddUpdateListener(UnityAction func)
            {
                updateEvent += func;
            }
            
            public void RemoveUpdateListener(UnityAction func)
            {
                updateEvent -= func;
            }

            public void RemoveAllUpdateListener()
            {
                while (updateEvent != null)
                {
                    updateEvent -= updateEvent;
                }
            }
        }

        private static TaskBehaviour taskBehaviour;

        public static MonoBehaviour CurTaskBehaviour => taskBehaviour;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DelayToInvoke()
        {
            GameObject gameObj = new GameObject("DelayToInvoke");
            UnityEngine.Object.DontDestroyOnLoad(gameObj);
            taskBehaviour = gameObj.AddComponent<TaskBehaviour>();
        }

        /// <summary>
        /// 开始协程，非Mono类可调用
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                return null;
            return taskBehaviour.StartCoroutine(routine);
        }

        public static CoroutineController StartExCoroutine(IEnumerator routine)
        {
            if (routine == null)
                return null;
            return taskBehaviour.StartExCoroutine(routine);
        }
        
        /// <summary>
        /// 终止协程
        /// </summary>
        /// <param name="routine"></param>
        public static void StopCoroutine(ref Coroutine routine)
        {
            if (routine != null)
            {
                taskBehaviour.StopCoroutine(routine);
                routine = null;
            }
        }

        /// <summary>
        /// 延迟调用
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        public static Coroutine DelayToInvokeBySecond(Action action, float delaySeconds)
        {
            if (action == null)
                return null;
            return taskBehaviour.StartCoroutine(StartDelayToInvokeBySecond(action, delaySeconds));
        }
        
        /// <summary>
        /// 延迟调用
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayFrames"></param>
        /// <returns></returns>
        public static Coroutine DelayToInvokeByFrame(Action action, int delayFrames)
        {
            if (action == null)
                return null;
            return taskBehaviour.StartCoroutine(StartDelayToInvokeByFrame(action, delayFrames));
        }

        /// <summary>
        /// 循环调用
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Coroutine ActionLoopByTime(float duration, float interval, Action action)
        {
            if (action == null)
                return null;
            if (duration <= 0 || interval <= 0 || duration < interval)
                return null;
            return taskBehaviour.StartCoroutine(StartActionLoopByTime(duration, interval, action));
        }

        /// <summary>
        /// 循环调用
        /// </summary>
        /// <param name="loopCount"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Coroutine ActionLoopByCount(int loopCount, float interval, Action action)
        {
            if (action == null)
                return null;
            if (loopCount <= 0 || interval <= 0)
                return null;
            return taskBehaviour.StartCoroutine(StartActionLoopByCount(loopCount, interval, action));
        }

        public static void AddUpdateListener(UnityAction func)
        {
            if (func == null)
                return;
            taskBehaviour.AddUpdateListener(func);
        }
        
        public static void RemoveUpdateListener(UnityAction func)
        {
            if (func == null)
                return;
            taskBehaviour.RemoveUpdateListener(func);
        }
        
        public static void RemoveAllUpdateListener()
        {
            taskBehaviour.RemoveAllUpdateListener();
        }
        
        //----------

        private static IEnumerator StartDelayToInvokeBySecond(Action action, float delaySeconds)
        {
            if (delaySeconds > 0)
                yield return new WaitForSeconds(delaySeconds);
            else
                yield return null;
            action?.Invoke();
        }

        private static IEnumerator StartDelayToInvokeByFrame(Action action, int delayFrames)
        {
            if (delayFrames > 1)
            {
                for (int i = 0; i < delayFrames; i++)
                {
                    yield return null;
                }
            }
            else
                yield return null;
            action?.Invoke();
        }

        private static IEnumerator StartActionLoopByTime(float duration, float interval, Action action)
        {
            yield return new CustomActionLoopByTime(duration, interval, action);
        }

        private static IEnumerator StartActionLoopByCount(int loopCount, float interval, Action action)
        {
            yield return new CustomActionLoopByCount(loopCount, interval, action);
        }

        private class CustomActionLoopByTime : CustomYieldInstruction
        {
            private Action callback;
            private float startTime;
            private float lastTime;
            private float interval;
            private float duration;

            public CustomActionLoopByTime(float _duration, float _interval, Action _callback)
            {
                //记录开始时间
                startTime = Time.time;
                //记录上一次间隔时间
                lastTime = Time.time;
                //记录间隔调用时间
                interval = _interval;
                //记录总时间
                duration = _duration;
                //间隔回调
                callback = _callback;
            }

            //保持协程暂停返回true。让coroutine继续执行返回 false。
            //在MonoBehaviour.Update之后、MonoBehaviour.LateUpdate之前，每帧都会查询keepWaiting属性。
            public override bool keepWaiting
            {
                get
                {
                    //此方法返回false表示协程结束
                    if (Time.time - startTime >= duration)
                    {
                        return false;
                    }
                    else if (Time.time - lastTime >= interval)
                    {
                        //更新上一次间隔时间
                        lastTime = Time.time;
                        callback?.Invoke();
                    }
                    return true;
                }
            }
        }
        private class CustomActionLoopByCount : CustomYieldInstruction
        {
            private Action callback;
            private float lastTime;
            private float interval;
            private int curCount;
            private int loopCount;

            public CustomActionLoopByCount(int _loopCount, float _interval, Action _callback)
            {
                lastTime = Time.time;
                interval = _interval;
                curCount = 0;
                loopCount = _loopCount;
                callback = _callback;
            }

            public override bool keepWaiting
            {
                get
                {
                    if (curCount > loopCount)
                    {
                        return false;
                    }
                    else if (Time.time - lastTime >= interval)
                    {
                        //更新上一次间隔时间
                        lastTime = Time.time;
                        curCount++;
                        callback?.Invoke();
                    }
                    return true;
                }
            }
        }
    }
}