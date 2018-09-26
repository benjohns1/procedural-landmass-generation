using NoiseGenerator;
using System.Collections.Generic;
using UnityEngine;

namespace NoiseGenerator
{
    public class RegionGenerator
    {
        private static Dictionary<int, NoiseMap> cache = new Dictionary<int, NoiseMap>();

        public static Region GenerateRegion(int width, int height, NoiseSettings settings, Vector2 startPoint, bool reinitialize = false)
        {
            int hash = settings.GetHashCode();
            NoiseMap map;
            if (!cache.TryGetValue(hash, out map))
            {
                map = new NoiseMap(settings);
                lock (cache)
                {
                    if (cache.ContainsKey(hash))
                    {
                        map = cache[hash];
                    }
                    else
                    {
                        cache.Add(hash, map);
                    }
                }
            }
            else if (reinitialize)
            {
                map.Initialize();
            }
            return map.GenerateRegion(width, height, startPoint);
        }
    }
}