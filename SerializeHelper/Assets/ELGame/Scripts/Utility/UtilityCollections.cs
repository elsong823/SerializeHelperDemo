
using System.Collections.Generic;

namespace ELGame
{
    public interface IRecyclePool
    {
        void PoolUpdate(float timeElapsed);
    }

    public interface IRecyclable
    {
        void OnRecycle();
        void Return();
    }

    /// <summary>
    /// 池管理器，用于同一个更新创建出来的回收池
    /// 定期更新capacity
    /// </summary>
    public static class RecyclePoolsMgr
    {
        private static List<IRecyclePool> singletonRecyclePools = new List<IRecyclePool>();

        //更新间隔
        private const float UPDATE_INTERVAL = 1f;

        private static float updateTimer = 0f;

        /// <summary>
        /// 注册一个回收池
        /// </summary>
        /// <param name="pool"></param>
        public static void Register(IRecyclePool pool)
        {
            if(pool != null)
            {
                singletonRecyclePools.Add(pool);
            }
        }

        /// <summary>
        /// 更新管理器，不用太高精度
        /// </summary>
        /// <param name="timeElapsed"></param>
        public static void Update(float timeElapsed)
        {
            updateTimer += timeElapsed;
            if(updateTimer >= UPDATE_INTERVAL)
            {
                updateTimer -= UPDATE_INTERVAL;

                //更新
                int poolCount = singletonRecyclePools.Count;
                for (int i = 0; i < poolCount; i++)
                {
                    singletonRecyclePools[i].PoolUpdate(UPDATE_INTERVAL);
                }
            }
        }
    }

    /// <summary>
    /// 单例回收池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonRecyclePool<T>
        : IRecyclePool
        where T : IRecyclable, new()
    {
        //检查capacity时间间隔
        //不活跃以后开始
        private const float REFRESH_CAPACITY_INTERVAL = 10f;

        private static SingletonRecyclePool<T> poolInstance;
        private static SingletonRecyclePool<T> Instance
        {
            get
            {
                if (poolInstance == null)
                {
                    poolInstance = LiteSingleton<SingletonRecyclePool<T>>.Instance;
                    //默认为10
                    poolInstance.capacity = 10;
                    //注册到池管理器中，接受更新
                    RecyclePoolsMgr.Register(poolInstance);
                }
                return poolInstance;
            }
        }

        private int totalGenerated = 0; //总共产生过多少对象
        private int totalReturn = 0;    //总共归还过多少次
        private int capacity = 10;
        private float refreshTimer = 0f;  
        private Stack<T> stack = new Stack<T>();

        /// <summary>
        /// 拿取一个新的
        /// </summary>
        /// <returns></returns>
        public static T Get()
        {
            if (Instance.stack.Count == 0)
            {
                ++poolInstance.totalGenerated;
                T instance = new T();
                
                return instance;
            }

            return poolInstance.stack.Pop();
        }

        /// <summary>
        /// 归还到回收池
        /// </summary>
        /// <param name="t"></param>
        public static void Return(T t)
        {
            if (t == null)
                return;
            
            t.OnRecycle();

            if(poolInstance != null)
            {
                ++poolInstance.totalReturn;

                //回收
                poolInstance.stack.Push(t);

                //触发检查
                poolInstance.refreshTimer = REFRESH_CAPACITY_INTERVAL - 0.1f;
            }
        }

        /// <summary>
        /// 获取当前池中的数量
        /// </summary>
        public static int Size
        {
            get
            {
                return Instance.stack.Count;
            }
        }

        /// <summary>
        /// 重新设置大小
        /// </summary>
        public static int Capacity
        {
            get { return Instance.capacity; }
            set
            {
                Instance.capacity = value;
                poolInstance.RefreshCapacity();
            }
        }

        /// <summary>
        /// 刷新池，超过大小的将会被移除
        /// </summary>
        private void RefreshCapacity()
        {
            if (stack.Count <= capacity)
                return;

            int gap = stack.Count - capacity;

            for (int i = 0; i < gap; i++)
                stack.Pop();

            //增加一点，不用update
            refreshTimer = REFRESH_CAPACITY_INTERVAL + 1f;
        }

        /// <summary>
        /// 获取当前池的情况
        /// </summary>
        /// <returns></returns>
        public string DebugDescription()
        {
            return string.Format("[{0}], gen:{1}, ret:{2}, cap:{3}, size:{4}",
                typeof(T).Name,
                totalGenerated,
                totalReturn,
                capacity,
                stack.Count);
        }

        /// <summary>
        /// 池更新，用于重置Capacity
        /// </summary>
        /// <param name="timeElapsed"></param>
        public void PoolUpdate(float timeElapsed)
        {
            //无需更新
            if (refreshTimer > REFRESH_CAPACITY_INTERVAL)
                return;

            refreshTimer -= timeElapsed;
            if(refreshTimer <= 0f)
                RefreshCapacity();
        }
    }
}