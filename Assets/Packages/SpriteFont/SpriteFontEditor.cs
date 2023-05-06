#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine.TextCore.LowLevel;

static class TM_EditorStyles {
    public static GUIStyle panelTitle;
    public static GUIStyle sectionHeader;
    public static GUIStyle textAreaBoxWindow;

    public static GUIStyle label;
    public static GUIStyle leftLabel;
    public static GUIStyle centeredLabel;
    public static GUIStyle rightLabel;

    public static Texture2D sectionHeaderStyleTexture;

    static TM_EditorStyles() {
        // Section Header
        CreateSectionHeaderStyle();

        // Labels
        panelTitle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
        label = leftLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, richText = true, wordWrap = true, stretchWidth = true };
        centeredLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, richText = true, wordWrap = true, stretchWidth = true };
        rightLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight, richText = true, wordWrap = true, stretchWidth = true };

        textAreaBoxWindow = new GUIStyle(EditorStyles.textArea) { richText = true };
    }

    internal static void CreateSectionHeaderStyle() {
        sectionHeader = new GUIStyle(EditorStyles.textArea) { fixedHeight = 22, richText = true, overflow = new RectOffset(9, 0, 0, 0), padding = new RectOffset(0, 0, 4, 0) };
        sectionHeaderStyleTexture = new Texture2D(1, 1);

        if (EditorGUIUtility.isProSkin)
            sectionHeaderStyleTexture.SetPixel(1, 1, new Color(0.4f, 0.4f, 0.4f, 0.5f));
        else
            sectionHeaderStyleTexture.SetPixel(1, 1, new Color(0.6f, 0.6f, 0.6f, 0.5f));

        sectionHeaderStyleTexture.Apply();
        sectionHeader.normal.background = sectionHeaderStyleTexture;
    }

    internal static void RefreshEditorStyles() {
        if (sectionHeader.normal.background == null) {
            Texture2D sectionHeaderStyleTexture = new Texture2D(1, 1);

            if (EditorGUIUtility.isProSkin)
                sectionHeaderStyleTexture.SetPixel(1, 1, new Color(0.4f, 0.4f, 0.4f, 0.5f));
            else
                sectionHeaderStyleTexture.SetPixel(1, 1, new Color(0.6f, 0.6f, 0.6f, 0.5f));

            sectionHeaderStyleTexture.Apply();
            sectionHeader.normal.background = sectionHeaderStyleTexture;
        }
    }
}

[CustomEditor(typeof(SpriteFontAsset))]
public class SpriteFontAssetEditor : Editor {
    private struct UI_PanelState {
        public static bool faceInfoPanel = true;
        public static bool generationSettingsPanel = true;
        public static bool fontAtlasInfoPanel = true;
        public static bool fontWeightPanel = true;
        public static bool fallbackFontAssetPanel = true;
        public static bool glyphTablePanel = false;
        public static bool characterTablePanel = false;
        public static bool fontFeatureTablePanel = false;
    }

    private struct GenerationSettings {
        public Font sourceFont;
        public int faceIndex;
        public GlyphRenderMode glyphRenderMode;
        public int pointSize;
        public int padding;
        public int atlasWidth;
        public int atlasHeight;
    }

    private static string[] s_UiStateLabel = new string[] { "<i>(Click to collapse)</i> ", "<i>(Click to expand)</i> " };

    private struct Warning {
        public bool isEnabled;
        public double expirationTime;
    }

    private int m_CurrentCharacterPage = 0;

    private bool m_DisplayDestructiveChangeWarning;

    private static readonly string[] k_InvalidFontFaces = { string.Empty };

    private string m_CharacterSearchPattern;
    private List<int> m_CharacterSearchList;

    private bool m_isSearchDirty;

    private const string k_UndoRedo = "UndoRedoPerformed";

    private SerializedProperty font_spritesheet_prop;
    private SerializedProperty font_spritespacing_prop;

    private SerializedProperty m_FontFaceIndex_prop;
    private SerializedProperty m_SamplingPointSize_prop;

    private SerializedProperty fontWeights_prop;

    private ReorderableList m_list;

    private SerializedProperty font_normalStyle_prop;
    private SerializedProperty font_normalSpacing_prop;

    private SerializedProperty font_boldStyle_prop;
    private SerializedProperty font_boldSpacing_prop;

    private SerializedProperty font_italicStyle_prop;
    private SerializedProperty font_tabSize_prop;

    private SerializedProperty m_FaceInfo_prop;
    private SerializedProperty m_CharacterTable_prop;

    private SpriteFontAsset m_spriteFontAsset;

    private bool isAssetDirty = false;

    private int errorCode;

    private System.DateTime timeStamp;

    public void OnEnable() {
        m_FaceInfo_prop = serializedObject.FindProperty("m_FaceInfo");

        font_spritesheet_prop = serializedObject.FindProperty("m_SpriteSheet");
        font_spritespacing_prop = serializedObject.FindProperty("m_SpriteSpacing");

        m_FontFaceIndex_prop = m_FaceInfo_prop.FindPropertyRelative("m_FaceIndex");
        m_SamplingPointSize_prop = m_FaceInfo_prop.FindPropertyRelative("m_PointSize");

        fontWeights_prop = serializedObject.FindProperty("m_FontWeightTable");

        m_list = new ReorderableList(serializedObject, serializedObject.FindProperty("m_FallbackFontAssetTable"), true, true, true, true);

        m_list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = m_list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };

        m_list.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Fallback List");
        };

        // Clean up fallback list in the event if contains null elements.
        CleanFallbackFontAssetTable();

        font_normalStyle_prop = serializedObject.FindProperty("m_RegularStyleWeight");
        font_normalSpacing_prop = serializedObject.FindProperty("m_RegularStyleSpacing");

        font_boldStyle_prop = serializedObject.FindProperty("m_BoldStyleWeight");
        font_boldSpacing_prop = serializedObject.FindProperty("m_BoldStyleSpacing");

        font_italicStyle_prop = serializedObject.FindProperty("m_ItalicStyleSlant");
        font_tabSize_prop = serializedObject.FindProperty("m_TabMultiple");

        m_CharacterTable_prop = serializedObject.FindProperty("m_CharacterTable");

        m_spriteFontAsset = target as SpriteFontAsset;
    }

    public void OnDisable() {
        // Revert changes if user closes or changes selection without having made a choice.
        if (m_DisplayDestructiveChangeWarning) {
            m_DisplayDestructiveChangeWarning = false;
            //RestoreGenerationSettings();
            GUIUtility.keyboardControl = 0;

            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI() {
        //Debug.Log("OnInspectorGUI Called.");

        Event currentEvent = Event.current;

        serializedObject.Update();

        Rect rect = EditorGUILayout.GetControlRect(false, 24);
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = EditorGUIUtility.fieldWidth;

		// FACE INFO PANEL
		#region Face info
		GUI.Label(rect, new GUIContent("<b>Face Info</b> - v" + m_spriteFontAsset.version), TM_EditorStyles.sectionHeader);

        EditorGUI.indentLevel = 1;

        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_FamilyName"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_StyleName"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_PointSize"));

        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_Scale"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_LineHeight"));

        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_AscentLine"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_CapLine"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_MeanLine"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_Baseline"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_DescentLine"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_UnderlineOffset"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_UnderlineThickness"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_StrikethroughOffset"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_StrikethroughThickness"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_SuperscriptOffset"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_SuperscriptSize"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_SubscriptOffset"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_SubscriptSize"));
        EditorGUILayout.PropertyField(m_FaceInfo_prop.FindPropertyRelative("m_TabWidth"));

        EditorGUILayout.Space();
        #endregion

        // ATLAS & MATERIAL PANEL
        #region Atlas & Material
        rect = EditorGUILayout.GetControlRect(false, 24);

		GUI.Label(rect, new GUIContent("<b>Spritesheet</b>"), TM_EditorStyles.sectionHeader);

        rect.x += rect.width - 132f;
        rect.y += 2;
        rect.width = 130f;
        rect.height = 18f;
        if (GUI.Button(rect, new GUIContent("Update Spritesheet"))) {
            m_spriteFontAsset.UpdateData();
        }

        EditorGUI.indentLevel = 1;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(font_spritesheet_prop, new GUIContent("Font Spritesheet"));
        EditorGUILayout.PropertyField(font_spritespacing_prop);

        if (EditorGUI.EndChangeCheck()) {
            m_spriteFontAsset.UpdateData();
        }

        EditorGUILayout.Space();
        #endregion

        string evt_cmd = Event.current.commandName; // Get Current Event CommandName to check for Undo Events

        // FONT WEIGHT PANEL
        #region Font Weights
        rect = EditorGUILayout.GetControlRect(false, 24);

		if (GUI.Button(rect, new GUIContent("<b>Font Weights</b>", "The Font Assets that will be used for different font weights and the settings used to simulate a typeface when no asset is available."), TM_EditorStyles.sectionHeader))
			UI_PanelState.fontWeightPanel = !UI_PanelState.fontWeightPanel;

		GUI.Label(rect, (UI_PanelState.fontWeightPanel ? s_UiStateLabel[0] : s_UiStateLabel[1]), TM_EditorStyles.rightLabel);

        if (UI_PanelState.fontWeightPanel) {
            EditorGUIUtility.labelWidth *= 0.75f;
            EditorGUIUtility.fieldWidth *= 0.25f;

            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            rect = EditorGUILayout.GetControlRect(true);
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2f;
            GUI.Label(rect, "Regular Typeface", EditorStyles.label);
            rect.x += rect.width;
            GUI.Label(rect, "Italic Typeface", EditorStyles.label);

            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(1), new GUIContent("100 - Thin"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(2), new GUIContent("200 - Extra-Light"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(3), new GUIContent("300 - Light"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(4), new GUIContent("400 - Regular"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(5), new GUIContent("500 - Medium"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(6), new GUIContent("600 - Semi-Bold"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(7), new GUIContent("700 - Bold"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(8), new GUIContent("800 - Heavy"));
            EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(9), new GUIContent("900 - Black"));

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(font_normalStyle_prop, new GUIContent("Regular Weight"));
            font_normalStyle_prop.floatValue = Mathf.Clamp(font_normalStyle_prop.floatValue, -3.0f, 3.0f);

            EditorGUILayout.PropertyField(font_boldStyle_prop, new GUIContent("Bold Weight"));
            font_boldStyle_prop.floatValue = Mathf.Clamp(font_boldStyle_prop.floatValue, -3.0f, 3.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(font_normalSpacing_prop, new GUIContent("Regular Spacing"));
            font_normalSpacing_prop.floatValue = Mathf.Clamp(font_normalSpacing_prop.floatValue, -100, 100);
            if (GUI.changed || evt_cmd == k_UndoRedo) {
                GUI.changed = false;
            }

            EditorGUILayout.PropertyField(font_boldSpacing_prop, new GUIContent("Bold Spacing"));
            font_boldSpacing_prop.floatValue = Mathf.Clamp(font_boldSpacing_prop.floatValue, 0, 100);
            if (GUI.changed || evt_cmd == k_UndoRedo) {
                GUI.changed = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(font_italicStyle_prop, new GUIContent("Italic Slant"));
            font_italicStyle_prop.intValue = Mathf.Clamp(font_italicStyle_prop.intValue, 15, 60);

            EditorGUILayout.PropertyField(font_tabSize_prop, new GUIContent("Tab Multiple"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;
        #endregion

        // FALLBACK FONT ASSETS
        #region Fallback Font Asset
        rect = EditorGUILayout.GetControlRect(false, 24);
        EditorGUI.indentLevel = 0;
        if (GUI.Button(rect, new GUIContent("<b>Fallback Font Assets</b>", "Select the Font Assets that will be searched and used as fallback when characters are missing from this font asset."), TM_EditorStyles.sectionHeader))
            UI_PanelState.fallbackFontAssetPanel = !UI_PanelState.fallbackFontAssetPanel;

        GUI.Label(rect, (UI_PanelState.fallbackFontAssetPanel ? s_UiStateLabel[0] : s_UiStateLabel[1]), TM_EditorStyles.rightLabel);

        if (UI_PanelState.fallbackFontAssetPanel) {
            EditorGUIUtility.labelWidth = 120;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            m_list.DoLayoutList();
            EditorGUILayout.Space();
        }
        #endregion

        // CHARACTER TABLE TABLE
        #region Character Table
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUIUtility.fieldWidth = fieldWidth;
        EditorGUI.indentLevel = 0;
        rect = EditorGUILayout.GetControlRect(false, 24);

        int characterCount = m_spriteFontAsset.characterTable.Count;

		if (GUI.Button(rect, new GUIContent("<b>Character Table</b>   [" + characterCount + "]" + (rect.width > 320 ? " Characters" : ""), "List of characters contained in this font asset."), TM_EditorStyles.sectionHeader))
			UI_PanelState.characterTablePanel = !UI_PanelState.characterTablePanel;

		GUI.Label(rect, (UI_PanelState.characterTablePanel ? s_UiStateLabel[0] : s_UiStateLabel[1]), TM_EditorStyles.rightLabel);

		if (UI_PanelState.characterTablePanel) {
            int arraySize = m_CharacterTable_prop.arraySize;
            int itemsPerPage = 15;

            // Display Glyph Management Tools
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Search Bar implementation
                #region DISPLAY SEARCH BAR
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 130f;
                    EditorGUI.BeginChangeCheck();
                    string searchPattern = EditorGUILayout.TextField("Character Search", m_CharacterSearchPattern, "SearchTextField");
                    if (EditorGUI.EndChangeCheck() || m_isSearchDirty) {
                        if (string.IsNullOrEmpty(searchPattern) == false) {
                            m_CharacterSearchPattern = searchPattern;

                            // Search Character Table for potential matches
                            SearchCharacterTable(m_CharacterSearchPattern, ref m_CharacterSearchList);
                        } else
                            m_CharacterSearchPattern = null;

                        m_isSearchDirty = false;
                    }

                    string styleName = string.IsNullOrEmpty(m_CharacterSearchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
                    if (GUILayout.Button(GUIContent.none, styleName)) {
                        GUIUtility.keyboardControl = 0;
                        m_CharacterSearchPattern = string.Empty;
                    }
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                // Display Page Navigation
                if (!string.IsNullOrEmpty(m_CharacterSearchPattern))
                    arraySize = m_CharacterSearchList.Count;

                DisplayPageNavigation(ref m_CurrentCharacterPage, arraySize, itemsPerPage);
            }
            EditorGUILayout.EndVertical();

            // Display Character Table Elements
            if (arraySize > 0) {
                // Display each character entry using the CharacterPropertyDrawer.
                for (int i = itemsPerPage * m_CurrentCharacterPage; i < arraySize && i < itemsPerPage * (m_CurrentCharacterPage + 1); i++) {
                    // Define the start of the selection region of the element.
                    Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                    int elementIndex = i;
                    if (!string.IsNullOrEmpty(m_CharacterSearchPattern))
                        elementIndex = m_CharacterSearchList[i];

                    SerializedProperty characterProperty = m_CharacterTable_prop.GetArrayElementAtIndex(elementIndex);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.PropertyField(characterProperty);

                    EditorGUILayout.EndVertical();
                }
            }

            DisplayPageNavigation(ref m_CurrentCharacterPage, arraySize, itemsPerPage);

            EditorGUILayout.Space();
        }
		#endregion

		if (serializedObject.ApplyModifiedProperties() || evt_cmd == k_UndoRedo || isAssetDirty) {
			// Delay callback until user has decided to Apply or Revert the changes.
			if (m_DisplayDestructiveChangeWarning == false) {
				//TextResourceManager.RebuildFontAssetCache();
				TextEventManager.ON_FONT_PROPERTY_CHANGED(true, m_spriteFontAsset);
			}

			isAssetDirty = false;
			EditorUtility.SetDirty(target);
		}
    }

    string[] GetFontFaces() {
        return GetFontFaces(m_FontFaceIndex_prop.intValue);
    }

    string[] GetFontFaces(int faceIndex) {
        if (LoadFontFace(m_SamplingPointSize_prop.intValue, faceIndex) == FontEngineError.Success)
            return FontEngine.GetFontFaces();

        return k_InvalidFontFaces;
    }

    FontEngineError LoadFontFace(int pointSize, int faceIndex) {
        //if (m_fontAsset.SourceFont_EditorRef != null) {
        //    if (FontEngine.LoadFontFace(m_fontAsset.SourceFont_EditorRef, pointSize, faceIndex) == FontEngineError.Success)
        //        return FontEngineError.Success;
        //}

        return FontEngine.LoadFontFace(m_spriteFontAsset.faceInfo.familyName, m_spriteFontAsset.faceInfo.styleName, pointSize);
    }

    void CleanFallbackFontAssetTable() {
        SerializedProperty m_FallbackFontAsseTable = serializedObject.FindProperty("m_FallbackFontAssetTable");

        bool isListDirty = false;

        int elementCount = m_FallbackFontAsseTable.arraySize;

        for (int i = 0; i < elementCount; i++) {
            SerializedProperty element = m_FallbackFontAsseTable.GetArrayElementAtIndex(i);
            if (element.objectReferenceValue == null) {
                m_FallbackFontAsseTable.DeleteArrayElementAtIndex(i);
                elementCount -= 1;
                i -= 1;

                isListDirty = true;
            }
        }

        if (isListDirty) {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }

    void DisplayPageNavigation(ref int currentPage, int arraySize, int itemsPerPage) {
        Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
        pagePos.width /= 3;

        int shiftMultiplier = Event.current.shift ? 10 : 1; // Page + Shift goes 10 page forward

        // Previous Page
        GUI.enabled = currentPage > 0;

        if (GUI.Button(pagePos, "Previous Page"))
            currentPage -= 1 * shiftMultiplier;


        // Page Counter
        GUI.enabled = true;
        pagePos.x += pagePos.width;
        int totalPages = (int) (arraySize / (float) itemsPerPage + 0.999f);
        //GUI.Label(pagePos, "Page " + (currentPage + 1) + " / " + totalPages, TM_EditorStyles.centeredLabel);

        // Next Page
        pagePos.x += pagePos.width;
        GUI.enabled = itemsPerPage * (currentPage + 1) < arraySize;

        if (GUI.Button(pagePos, "Next Page"))
            currentPage += 1 * shiftMultiplier;

        // Clamp page range
        currentPage = Mathf.Clamp(currentPage, 0, arraySize / itemsPerPage);

        GUI.enabled = true;
    }

    void SearchCharacterTable(string searchPattern, ref List<int> searchResults) {
        if (searchResults == null) searchResults = new List<int>();

        searchResults.Clear();

        int arraySize = m_CharacterTable_prop.arraySize;

        for (int i = 0; i < arraySize; i++) {
            SerializedProperty sourceCharacter = m_CharacterTable_prop.GetArrayElementAtIndex(i);

            int id = sourceCharacter.FindPropertyRelative("m_Unicode").intValue;

            // Check for potential match against a character.
            if (searchPattern.Length == 1 && id == searchPattern[0])
                searchResults.Add(i);
            else if (id.ToString("x").Contains(searchPattern))
                searchResults.Add(i);
            else if (id.ToString("X").Contains(searchPattern))
                searchResults.Add(i);

            // Check for potential match against decimal id
            //if (id.ToString().Contains(searchPattern))
            //    searchResults.Add(i);
        }
    }
}

#endif