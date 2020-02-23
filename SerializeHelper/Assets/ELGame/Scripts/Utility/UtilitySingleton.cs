using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ELGame
{
    /// <summary>
    /// 超简单泛型单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LiteSingleton<T>
        where T : new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }

                return instance;
            }
        }
    }
}