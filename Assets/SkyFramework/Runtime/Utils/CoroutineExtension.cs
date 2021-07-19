using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFramework
{
    public static class CoroutineExtension
    {
        /// <summary>
        ///扩展mono开启协程方法
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="runner"></param>
        /// <returns></returns>
        public static CoroutineController StartExCoroutine(this MonoBehaviour mono, IEnumerator runner)
        {
            CoroutineController controller = new CoroutineController(mono, runner);
            controller.Start();
            return controller;
        }
    }


    public class CoroutineController
    {
        private static int sID;

        public static int Id
        {
            get { return sID; }
            private set { sID = value; }
        }


        protected MonoBehaviour mono;

        protected ExCoroutine exCoroutine;

        protected IEnumerator runner;
        protected Coroutine coroutine;

        public bool IsComplete => exCoroutine.state == ExCoroutine.CoroutineState.Stop;

        protected CoroutineController()
        {
            Id = sID++;
        }
        
        public CoroutineController(MonoBehaviour mono, IEnumerator runner) : this()
        {
            this.exCoroutine = new ExCoroutine();
            this.mono = mono;
            this.runner = runner;
        }


        /// <summary>
        /// 更新运行协程
        /// </summary>
        /// <param name="runner"></param>
        /// <returns></returns>
        public bool UpdateIEnumerator(IEnumerator runner)
        {
            this.runner = runner;
            if (this.exCoroutine?.state == ExCoroutine.CoroutineState.Stop)
                return true;
            else
            {
                Debug.LogWarning("请等待当前协程停止后 再替换运行的协程");
                return false;
            }
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        public void Start()
        {
            this.exCoroutine.state = ExCoroutine.CoroutineState.Running;
            this.coroutine = mono.StartCoroutine(this.exCoroutine.HelperIE(runner));
        }

        /// <summary>
        /// 暂停协程
        /// </summary>
        public void Pause()
        {
            this.exCoroutine.state = ExCoroutine.CoroutineState.Pause;

        }

        /// <summary>
        /// 恢复协程
        /// </summary>
        public void Resume()
        {
            this.exCoroutine.state = ExCoroutine.CoroutineState.Running;
        }

        /// <summary>
        ///停止协程
        /// </summary>
        public void Stop()
        {
            this.exCoroutine.state = ExCoroutine.CoroutineState.Stop;
            this.mono.StopCoroutine(this.coroutine);
        }

        /// <summary>
        /// 重启协程
        /// </summary>
        public void Restart()
        {
            if (coroutine != null) mono.StopCoroutine(this.coroutine);
            Start();
        }
    }





    public class ExCoroutine
    {

        public enum CoroutineState
        {
            Waitting,
            Running,
            Pause,
            Stop
        }

        public CoroutineState state = CoroutineState.Pause;
        public bool isComplete = false;

        /// <summary>
        /// 流程控制
        /// </summary>
        /// <returns></returns>
        public IEnumerator HelperIE(IEnumerator runnner)
        {
            while (state == CoroutineState.Waitting) 
                yield return null;
            while (state == CoroutineState.Running)
            {
                while (state == CoroutineState.Pause) 
                    yield return null;
                if (runnner != null && runnner.MoveNext())
                    yield return runnner.Current;
                else 
                    state = CoroutineState.Stop;
            }
        }

    }
}