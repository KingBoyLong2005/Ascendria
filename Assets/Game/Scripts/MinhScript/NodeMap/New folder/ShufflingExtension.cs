using System;
using System.Collections.Generic;
using System.Linq;

namespace Map
{
    public static class ShufflingExtension
    {
        private static System.Random rng = new System.Random();

        //Shuffle list
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        //Get random element in list
        public static T Random<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        //Get last element in list
        public static T Last<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        //Get n(elementsCount) random element in list
        public static List<T> GetRandomElements<T>(this List<T> list, int elementsCount)
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(list.Count < elementsCount ? list.Count : elementsCount)
                .ToList();
        }
    }
}