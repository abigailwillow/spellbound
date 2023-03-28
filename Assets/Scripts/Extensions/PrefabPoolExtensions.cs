using UnityEngine;
using Photon.Pun;

public static class PrefabPoolExtensions {
    /// <summary>
    /// Register prefabs to be network instantiated that are not located in a Resources folder.
    /// </summary>
    /// <param name="prefab">The prefab to be instantiated.</param>
    public static void Register(this DefaultPool prefabPool, GameObject prefab) {
        if (!prefabPool.ResourceCache.ContainsKey(prefab.name)) {
            prefabPool.ResourceCache.Add(prefab.name, prefab);
        } else {
            Debug.LogError($"A prefab was already registered with the key '{prefab.name}' make sure you use a unique Key for each prefab or not registered multiple times.");
        }
    }

    /// <summary>
    /// Register prefabs to be network instantiated that are not located in a Resources folder.
    /// </summary>
    /// <param name="prefab">The prefab to be instantiated.</param>
    public static void Register(this IPunPrefabPool prefabPool, GameObject prefab) {
        if (prefabPool is DefaultPool) {
            ((DefaultPool) prefabPool).Register(prefab);
        } else {
            Debug.LogError($"The prefab pool {prefabPool.GetType().Name} does not support registering prefabs.", prefabPool as Object);
        }
    }
}