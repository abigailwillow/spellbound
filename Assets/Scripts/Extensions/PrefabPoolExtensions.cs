using UnityEngine;
using Photon.Pun;

public static class PrefabPoolExtensions {
    /// <summary>Register prefabs to be network instantiated without having to exist in the Resources folder</summary>
    /// <param name="prefabs">The prefab(s) to be instantiated</param>
    public static void Register(this IPunPrefabPool prefabPool, params GameObject[] prefabs) {
        // Add the first prefab to the list
        foreach (GameObject prefab in prefabs) {
            if (prefabPool is not DefaultPool) {
                Debug.LogError($"The prefab pool {prefabPool.GetType().Name} does not support registering prefabs.");
                return;
            }
            ((DefaultPool)prefabPool).Register(prefab);
        }
    }

    public static void Register(this DefaultPool prefabPool, GameObject prefab) {
        if (prefabPool.ResourceCache.ContainsKey(prefab.name)) {
            Debug.LogError($"A prefab was already registered with the key '{prefab.name}' make sure to use a unique key for each prefab.");
            return;
        }
        prefabPool.ResourceCache.Add(prefab.name, prefab);
    }
}