using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.Text;

public class SpriteFontAsset : FontAsset {
    #if UNITY_EDITOR
    [SerializeField]
    float m_SpriteSpacing = 0;

    [SerializeField]
    Texture2D m_SpriteSheet;


    private void Awake() => Init();
    private void OnValidate() => Init();
	private void Reset() => Init();

    private void OnDestroy() {
        EditorApplication.update -= DelayedInit;
    }

    private void Init() {
        if (material) return;

        if (AssetDatabase.Contains(this)) {
            DelayedInit();
        } else {
            EditorApplication.update -= DelayedInit;
            EditorApplication.update += DelayedInit;
        }
    }

    void SetInternal(string parameter, object value) {
        typeof(SpriteFontAsset)
            .GetField(parameter, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(this, value);
    }

    private void DelayedInit() {
        if (!AssetDatabase.Contains(this)) return;

        EditorApplication.update -= DelayedInit;

		UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
        material = assets.FirstOrDefault(a => a.GetType() == typeof(Material)) as Material;
        
        if (!material) {
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            if (shader == null) shader = Shader.Find("Text/Sprite");
            if (shader == null) shader = Shader.Find("Hidden/TextCore/Sprite");

            material = new Material(shader);
            material.name = "Hidden/TextCore/Sprite";

            material.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(material, this);
        }

        SetInternal("m_Material", material);
        SetInternal("m_Version", "1.1.0");

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public void UpdateData() {
        this.glyphTable.Clear();
        this.characterTable.Clear();

        if (this.m_SpriteSheet != null) {
            SetInternal("m_AtlasWidth", this.m_SpriteSheet.width);
            SetInternal("m_AtlasHeight", this.m_SpriteSheet.height);

            this.material.mainTexture = this.m_SpriteSheet;
            this.atlasTextures = new Texture2D[1];
            this.atlasTextures[0] = this.m_SpriteSheet;

            List<Sprite> sprites = new List<Sprite>();
            UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this.m_SpriteSheet));

            if (data != null) {
                foreach (UnityEngine.Object obj in data) {
                    if (obj.GetType() == typeof(Sprite)) {
                        sprites.Add(obj as Sprite);
                    }
                }
            }

            uint glyphIndex = 0;
            foreach (Sprite sprite in sprites) {
                SpriteGlyph glyph = new SpriteGlyph();
                glyph.sprite = sprite;
                glyph.index = glyphIndex;
                glyph.atlasIndex = 0;
                glyph.glyphRect = new GlyphRect() {
                    height = (int) sprite.rect.height - 10,
                    width = (int) sprite.rect.width - 10,
                    x = (int) sprite.rect.x + 5,
                    y = (int) sprite.rect.y + 5
                };

                glyph.metrics = new GlyphMetrics() {
                    height = sprite.rect.height - 10,
                    width = sprite.rect.width - 10,
                    horizontalBearingX = 5,
                    horizontalBearingY = sprite.rect.height - 5,
                    horizontalAdvance = sprite.rect.width + m_SpriteSpacing
                };

                this.glyphTable.Add(glyph);

                uint unicode = 0;
                bool unicodeRetrieved = false;

                string name = sprite.name;
                if (name.Length > 2) {
                    string prefix = name.ToLower().Substring(0, 2);
                    string value = name.ToLower().Substring(2, name.Length - 2);

                    int numberBase = 0; 

                    switch (prefix) {
                        case "0x":
                            numberBase = 16;
                            break;
                        case "0d":
                            numberBase = 10;
                            break;
                        case "0o":
                            numberBase = 8;
                            break;
                        case "0b":
                            numberBase = 2;
                            break;
                    }

                    if (numberBase > 0) {
                        unicode = Convert.ToUInt32(value, numberBase);
                        unicodeRetrieved = true;
                    }
                }
                            
                if (!unicodeRetrieved) {
                    unicode = (uint) name[0];
                    unicodeRetrieved = true;
                }

                Character character = new Character(unicode, glyph);
                character.scale = 1;

                this.characterTable.Add(character);

                glyphIndex += 1;
            }
        }

        SetInternal("m_Material", material);
        ReadFontAssetDefinition();
        EditorUtility.SetDirty(this);
    }

    #endif
}
