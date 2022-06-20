//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using EP.U3D.EDITOR.BASE;

namespace EP.U3D.EDITOR.UI
{
    public class WinUI : EditorWindow
    {
        [MenuItem(Constants.MENU_WIN_UIEASE, false, 1)]
        public static void Invoke()
        {
            if (Instance == null)
            {
                GetWindowWithRect<WinUI>(new Rect(30, 30, 285, 450), true, "UI Ease");
            }
            Refresh();
            float invokeInterval = Time.realtimeSinceStartup - lastInvokeTime;
            if (invokeInterval < 0.5f)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
                else
                {
                    string latestEditPrefabPath = EditorPrefs.GetString(LATEST_EDIT_PREFAB);
                    HandleEdit(latestEditPrefabPath);
                }
            }
            lastInvokeTime = Time.realtimeSinceStartup;
            if (Instance != null) Instance.OnSelectChange();
        }

        private static string LATEST_EDIT_PREFAB = Path.GetFullPath("./") + "LATEST_EDIT_PREFAB";
        public static WinUI Instance;
        private Vector2 scroll = Vector2.zero;
        private static List<string> prefabPaths = new List<string>();
        private static List<GameObject> prefabs = new List<GameObject>();
        private static float lastInvokeTime;
        private static string searchStr = "";
        private static float unsearchHeigth;

        private void OnEnable()
        {
            Instance = this;
            lastInvokeTime = 0;
            Refresh();
            Selection.selectionChanged += OnSelectChange;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectChange;
        }

        private void OnSelectChange()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var index = prefabPaths.IndexOf(path);
            if (index > 0)
            {
                if (!string.IsNullOrEmpty(searchStr))
                {
                    unsearchHeigth = index * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2);
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
            prefabPaths.Clear();
            prefabs.Clear();
        }

        private void OnGUI()
        {
            if (prefabPaths.Count == 0 || prefabs.Count == 0) return;

            Helper.BeginContents();
            searchStr = Helper.SearchField(searchStr, GUILayout.Height(20));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item", GUILayout.Width(200));
            GUILayout.Label("Operate");
            GUILayout.EndHorizontal();
            Helper.EndContents();

            if (string.IsNullOrEmpty(searchStr))
            {
                scroll.y = unsearchHeigth;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            if (string.IsNullOrEmpty(searchStr))
            {
                unsearchHeigth = scroll.y;
            }
            for (int i = 0; i < prefabPaths.Count; i++)
            {
                if (prefabs[i].name.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false, GUILayout.Width(200));
                if (GUILayout.Button("Edit", GUILayout.Width(35)))
                {
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.isPlaying = false;
                    }
                    else
                    {
                        HandleEdit(prefabPaths[i]);
                    }
                    OnSelectChange();
                }
                if (GUILayout.Button("Path", GUILayout.Width(35)))
                {
                    if (File.Exists(prefabPaths[i]))
                    {
                        string fullPath = Path.GetFullPath(prefabPaths[i]);
                        Helper.ShowInExplorer(fullPath);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.Space(3);
            if (GUILayout.Button("Create New UI"))
            {
                // TODO: 可以指定模板创建UI
            }
        }

        private static void Refresh()
        {
            prefabPaths.Clear();
            prefabs.Clear();
            prefabPaths.AddRange(Constants.UI_EXTRAS);
            Helper.CollectAssets(Constants.UI_WORKSPACE, prefabPaths, ".cs", ".js", ".meta", ".DS_Store");
            for (int i = 0; i < prefabPaths.Count; i++)
            {
                string path = prefabPaths[i];
                if (File.Exists(path) && path.EndsWith(".prefab"))
                {
                    prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }
                else
                {
                    prefabPaths.RemoveAt(i);
                    i--;
                }
            }
        }

        private static void HandleEdit(string prefabPath)
        {
            if (File.Exists(prefabPath))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab)
                {
                    Type clazzProjectBrowser = Helper.GetUnityEditorClazz("UnityEditor.ProjectBrowser");
                    FocusWindowIfItsOpen(clazzProjectBrowser);
                    AssetDatabase.OpenAsset(prefab);
                    Selection.activeObject = prefab;
                    PrefabUtility.InstantiatePrefab(prefab);
                    EditorPrefs.SetString(LATEST_EDIT_PREFAB, prefabPath);
                }
            }
        }
    }
}