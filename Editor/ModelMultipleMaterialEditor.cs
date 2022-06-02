using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Modified by Ben Van Treese 2022
/// 
/// Copyright(c) 2021 Shawn Fox

/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// 
/// If this tool has helped you with your project, please consider donating to help me with my next Stealth Game (It's gonna be cool)
/// https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=QNBCJGGNLJ2LN&item_name=Thanks+for+supporting+me%21&currency_code=AUD
/// Thanks and godspeed to your own project
/// Designed Shawn Fox
/// </summary>

public class ModelMultipleMaterialEditor : EditorWindow {
  [MenuItem("Tools/Dropecho/Model Multiple Material Editor", false, 2)]
  public static void ShowWindow() {
    _window = GetWindow(typeof(ModelMultipleMaterialEditor));
    _window.titleContent = new GUIContent("MMME");
  }
  static EditorWindow _window;
  Vector2 _scrollPosition;

  ModelImporterMaterialImportMode _importModeSetting = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
  ModelImporterMaterialName _importNameSetting = ModelImporterMaterialName.BasedOnMaterialName;
  ModelImporterMaterialSearch _importSearchSetting = ModelImporterMaterialSearch.Everywhere;
  ModelImporterMaterialLocation _importLocationSetting = ModelImporterMaterialLocation.InPrefab;
  bool _showMaterialHelpBox;
  string _basePath;

  public void OnSelectionChange() {
    Repaint();
  }

  public void OnGUI() {
    _window.minSize = new Vector2(400, 1);
    _window.maxSize = new Vector2(1500, 1000);

    DrawTextureSection();
    GUILayout.Space(16);
    DrawMaterialSection();
  }

  void DrawTextureSection() {
    GUILayout.BeginVertical("Extract Textures", "window", GUILayout.MaxHeight(64));

    GUILayout.BeginHorizontal();
    GUILayout.BeginVertical();
    if (GUILayout.Button("Extract all textures")) {
      ExtractTextures();
    }
    if (GUILayout.Button("Extract all materials")) {
      ExtractMaterials();
    }
    GUILayout.BeginHorizontal();
    GUILayout.Label("Destination", GUILayout.Width(88));
    _basePath = GUILayout.TextField(_basePath);
    if (GUILayout.Button("...", GUILayout.Width(32))) {
      if (GetTextureDestinationPath(out string selFolder)) {
        _basePath = selFolder;
      }
    }
    GUILayout.EndHorizontal();
    GUILayout.EndVertical();
    GUILayout.EndHorizontal();
    GUILayout.EndVertical();
  }

  void ExtractTextures() {
    ModelImporter importer = null;

    try {
      if (Selection.objects != null) {
        foreach (Object obj in Selection.objects) {
          if (obj is GameObject) {
            string pathName = AssetDatabase.GetAssetPath(obj);
            try {
              importer = ModelImporter.GetAtPath(pathName) as ModelImporter;
            } catch (System.Exception ea) {
              Debug.LogError(AssetDatabase.GetAssetPath(obj) + " could not be converted to a model.");
              continue;
            }
            if (importer == null) {
              Debug.LogError("Invalid model selected");
              return;
            }
            if (!AssetDatabase.IsValidFolder(_basePath + "/" + obj.name)) {
              AssetDatabase.CreateFolder(_basePath, obj.name);
            }
            if (!importer.ExtractTextures(_basePath + "/" + obj.name)) {
              Debug.LogError("There were no embedded textures to export or the operation failed");
            } else {
              Debug.Log("Textures extracted to " + _basePath + "/" + obj.name);
            }
          } else {
            if (importer == null) {
              Debug.LogError("Selected isn't a model");
              return;
            }
          }
        }
      } else {
        if (importer == null) {
          Debug.LogError("Nothing selected");
          return;
        }
      }
    } catch (System.Exception ea) {
      Debug.LogError("There was a problem processing the operation.\n" + ea);
    }
  }

  void ExtractMaterials() {
    ModelImporter importer = null;

    try {
      if (Selection.objects != null) {
        foreach (Object obj in Selection.objects) {
          if (obj is GameObject) {
            string pathName = AssetDatabase.GetAssetPath(obj);
            try {
              importer = ModelImporter.GetAtPath(pathName) as ModelImporter;
            } catch (System.Exception ea) {
              Debug.LogError(AssetDatabase.GetAssetPath(obj) + " could not be converted to a model.");
              continue;
            }
            if (importer == null) {
              Debug.LogError("Invalid model selected");
              return;
            }
            if (!AssetDatabase.IsValidFolder(_basePath + "/Materials")) {
              AssetDatabase.CreateFolder(_basePath, "Materials");
            }
            if (ExtractMaterialsFromPrefab(pathName, _basePath + "/Materials")) {
              Debug.LogError("There were no embedded textures to export or the operation failed");
            } else {
              Debug.Log("Materials extracted to " + _basePath + "/Materials");
            }
          } else {
            if (importer == null) {
              Debug.LogError("Selected isn't a model");
              return;
            }
          }
        }
      } else {
        if (importer == null) {
          Debug.LogError("Nothing selected");
          return;
        }
      }
    } catch (System.Exception ea) {
      Debug.LogError("There was a problem processing the operation.\n" + ea);
    }
  }

  private void HandleApplyToSelectionButton() {
    ModelImporter importer = null;
    try {
      if (Selection.objects != null) {
        foreach (Object obj in Selection.objects) {
          if (obj is GameObject) {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            try {
              importer = ModelImporter.GetAtPath(assetPath) as ModelImporter;
            } catch (System.Exception ea) {
              Debug.LogError(AssetDatabase.GetAssetPath(obj) + " could not be converted to a model.");
              continue;
            }
            var map = importer.GetExternalObjectMap();
            foreach (var pair in map) {
              importer.RemoveRemap(pair.Key);
            }
            importer.materialImportMode = _importModeSetting;
            importer.materialLocation = _importLocationSetting;
            if (_importModeSetting != ModelImporterMaterialImportMode.None) {
              importer?.SearchAndRemapMaterials(_importNameSetting, _importSearchSetting);
            }
            importer?.SaveAndReimport();
          } else {
            Debug.LogError(AssetDatabase.GetAssetPath(obj) + " is not a model.");
          }
        }
        Debug.Log("Applied settings to " + Selection.objects.Length + " models.");
      }
    } catch (System.Exception ea) {
      Debug.LogError("There was a problem processing the operation.\n" + ea);
    }
  }

  private void DrawMaterialSection() {
    GUILayout.BeginVertical("Automatically search and remap materials on a model", "window", GUILayout.MaxHeight(1000));//T

    if (_showMaterialHelpBox) {
      if (GUILayout.Button("Close")) {
        _showMaterialHelpBox = !_showMaterialHelpBox;
      }
      EditorGUILayout.HelpBox("If you are using Unity FBX Exporter or a similar tool, ensure you uncheck the option 'Compatible Naming'. " +
          "It will replace all of the spaces in your material names with underscores and thus this tool will not find " +
          "the correct materials. If you are using Autodesk Maya in your toolset, you should keep this option on and use safe characters in your file names.", MessageType.Info);
    } else {
      if (GUILayout.Button("Can't find my model's materials?")) {
        _showMaterialHelpBox = !_showMaterialHelpBox;
      }
    }

    EditorGUILayout.Space();
    GUILayout.BeginHorizontal();
    GUILayout.Label("Material Creation Mode", GUILayout.Width(150));
    _importModeSetting = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup(_importModeSetting);
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    GUILayout.Label("Material Location", GUILayout.Width(150));
    _importLocationSetting = (ModelImporterMaterialLocation)EditorGUILayout.EnumPopup(_importLocationSetting);
    GUILayout.EndHorizontal();
    GUILayout.Space(8);


    if (_importModeSetting == ModelImporterMaterialImportMode.None) {
      EditorGUILayout.HelpBox("Materials will not be imported. Use Unity's default material instead.", MessageType.Info);
    } else {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Naming", GUILayout.Width(150));
      _importNameSetting = (ModelImporterMaterialName)EditorGUILayout.EnumPopup(_importNameSetting);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Search", GUILayout.Width(150));
      _importSearchSetting = (ModelImporterMaterialSearch)EditorGUILayout.EnumPopup(_importSearchSetting);
      GUILayout.EndHorizontal();
    }
    GUILayout.Space(8);
    if (GUILayout.Button("Apply to selection")) {
      HandleApplyToSelectionButton();
    }

    _scrollPosition = DrawCurrentSelection(_scrollPosition);

    GUILayout.EndVertical();
  }

  private static string GetSelectionPath() {
    string guid = Selection.assetGUIDs[0];
    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), assetPath);

    System.IO.FileAttributes attributes = System.IO.File.GetAttributes(fullPath);
    return attributes.HasFlag(System.IO.FileAttributes.Directory) ? fullPath : System.IO.Path.GetDirectoryName(fullPath);
  }

  public static bool GetTextureDestinationPath(out string path) {
    path = "";
    try {
      var selectionPath = GetSelectionPath();
      if (selectionPath != "") {
        path = selectionPath.Substring(selectionPath.LastIndexOf("Assets")).Replace("/", "\\");
        return true;
      } else {
        Debug.LogError("Select a folder in the project view.");
        return false;
      }
    } catch (System.Exception ea) {
      if (ea is System.IndexOutOfRangeException) {
        Debug.LogError("Select a folder from the project view.\n" + ea);
      } else {
        Debug.LogError("The folder you have selected is not accessible.\n" + ea);
      }

      return false;
    }
  }

  public static Vector2 DrawCurrentSelection(Vector2 scrollPosition) {
    if (Selection.objects != null) {
      GUILayout.Label("Current selection: (" + Selection.objects.Length + ")");
      scrollPosition = GUILayout.BeginScrollView(scrollPosition);
      foreach (Object sbsbsbs in Selection.objects) {
        GUILayout.Label(string.Format("{0}: {1}", sbsbsbs.GetType().Name, sbsbsbs.name));
      }
      GUILayout.EndScrollView();
    }
    return scrollPosition;
  }

  public static bool ExtractMaterialsFromPrefab(string assetPath, string destinationPath) {
    var materialPaths = new HashSet<string>();
    var modelPaths = new HashSet<string>();
    var enumerable = AssetDatabase
        .LoadAllAssetsAtPath(assetPath)
        .Where(x => x.GetType() == typeof(Material));

    foreach (var item in enumerable) {
      string path = destinationPath + "/" + item.name + ".mat";
      if (materialPaths.Contains(path)) {
        modelPaths.Add(assetPath);
        continue;
      } else {
        string value = AssetDatabase.ExtractAsset(item, path);
        if (string.IsNullOrEmpty(value)) {
          modelPaths.Add(assetPath);
        }
      }
      materialPaths.Add(path);
    }

    foreach (string item2 in modelPaths) {
      AssetDatabase.WriteImportSettingsIfDirty(item2);
      AssetDatabase.ImportAsset(item2, ImportAssetOptions.ForceUpdate);
    }

    return false;
  }
}