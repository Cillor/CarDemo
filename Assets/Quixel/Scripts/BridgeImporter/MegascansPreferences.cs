#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Quixel
{

    public class MegascansPreferences : MonoBehaviour
    {
        
        public static void ClearMegascansEditorPreferences()
        {
            EditorPrefs.DeleteKey("QuixelDefaultPath");
            EditorPrefs.DeleteKey("QuixelDefaultDisplacement");
            EditorPrefs.DeleteKey("QuixelDefaultTexPacking");
            EditorPrefs.DeleteKey("QuixelDefaultShader");
            EditorPrefs.DeleteKey("QuixelDefaultImportResolution");
            EditorPrefs.DeleteKey("QuixelDefaultLodFadeMode");
            EditorPrefs.DeleteKey("QuixelDefaultConnection");
            EditorPrefs.DeleteKey("QuixelDefaultSetupCollision");
            EditorPrefs.DeleteKey("QuixelDefaultApplyToSelection");
            EditorPrefs.DeleteKey("QuixelDefaultAddAssetToScene");
            EditorPrefs.DeleteKey("QuixelDefaultTerrainNormal");
            EditorPrefs.DeleteKey("QuixelDefaultDecalBlend");
            EditorPrefs.DeleteKey("QuixelDefaultDecalSize");
            EditorPrefs.DeleteKey("QuixelDefaultMaterialName");
            EditorPrefs.DeleteKey("QuixelDefaultMaterialPath");
            EditorPrefs.DeleteKey("QuixelDefaultTiling");
            MegascansImporterWindow.GetDefaults();
        }
    }
}

#endif
