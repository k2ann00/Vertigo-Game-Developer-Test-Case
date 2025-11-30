using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Utils // ChatGPT sağolsun
{
    public static class ListExtensions
    {
        /// <typeparam name="T">List element tipi</typeparam>
        /// <param name="list">Karıştırılacak list</param>
        public static void Shuffle<T>(this List<T> list)
        {
            if (list == null || list.Count <= 1)
            {
                return;
            }

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);

                // Swap
                T temp = list[k];
                list[k] = list[n];
                list[n] = temp;
            }
        }

        /// <typeparam name="T">List element tipi</typeparam>
        /// <param name="list">Karıştırılacak list</param>
        /// <returns>Karıştırılmış yeni list</returns>
        public static List<T> Shuffled<T>(this List<T> list)
        {
            if (list == null)
            {
                return new List<T>();
            }

            List<T> shuffled = new List<T>(list);
            shuffled.Shuffle();
            return shuffled;
        }

        /// <typeparam name="T">List element tipi</typeparam>
        /// <param name="list">Element listesi</param>
        /// <param name="weightSelector">Her element için weight dönen fonksiyon</param>
        /// <returns>Seçilen element</returns>
        public static T SelectWeightedRandom<T>(this List<T> list, System.Func<T, float> weightSelector)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }

            // Toplam weight hesapla
            float totalWeight = 0f;
            foreach (var item in list)
            {
                totalWeight += weightSelector(item);
            }

            if (totalWeight <= 0f)
            {
                // Weight yoksa random seç
                return list[Random.Range(0, list.Count)];
            }

            // Weighted random selection
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in list)
            {
                currentWeight += weightSelector(item);
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }

            // Fallback (float precision sorunları için)
            return list[list.Count - 1];
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            return new List<T>(list);
        }

        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }

            return list[Random.Range(0, list.Count)];
        }

        public static List<T> GetRandomElements<T>(this List<T> list, int count)
        {
            if (list == null || list.Count == 0)
            {
                return new List<T>();
            }

            count = Mathf.Min(count, list.Count);
            List<T> shuffled = list.Shuffled();
            return shuffled.GetRange(0, count);
        }
    }
}
