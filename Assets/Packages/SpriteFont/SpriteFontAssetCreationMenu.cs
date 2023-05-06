#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

internal static class SpriteFontAsset_CreationMenu {
    [MenuItem("Assets/Create/Text/Sprite Font Asset", false, 115)]
    static void CreateSpriteFontAsset() {
        // Initialize FontEngine
        FontEngine.InitializeFontEngine();

        // Create new Font Asset
        SpriteFontAsset spriteFontAsset = ScriptableObject.CreateInstance<SpriteFontAsset>();
        ProjectWindowUtil.CreateAsset(spriteFontAsset, "New Sprite Font Asset.asset");

        spriteFontAsset.faceInfo = FontEngine.GetFaceInfo();
    }
}

#endif