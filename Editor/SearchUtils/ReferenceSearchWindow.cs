using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gameframe.EditorUtils
{
  public class ReferenceSearchWindow : EditorWindow
  {
    private static ReferenceSearchWindow GetWindow()
    {
      return GetWindow<ReferenceSearchWindow>( false, "Prefab Search", true );
    }

    public enum SearchMode : int
    {
      StringSearch = 0, //Searching for a string in the prefab file
      AssetSearch = 1, //Searching for a reference to an asset
    }

    [Serializable]
    public class WindowSettings
    {
      public SearchMode currentMode = SearchMode.StringSearch;
      public string path = "Assets";
      public string searchString = "";
    }

    private WindowSettings settings = new WindowSettings();

    private UnityEngine.Object assetFile;

    private readonly string[] searchTypeOptions = {"String", "Asset"};

    private const string PrefsKey = "EditorPrefabSearchWindow";

    private void OnEnable()
    {
      //Load State
      var jsonData = PlayerPrefs.GetString( PrefsKey, null );

      if ( string.IsNullOrEmpty( jsonData ) )
      {
        return;
      }

      settings = JsonUtility.FromJson<WindowSettings>( jsonData );
    }

    private void OnDisable()
    {
      //Save State
      PlayerPrefs.SetString( PrefsKey, JsonUtility.ToJson( settings ) );
    }

    private void OnGUI()
    {
      //Must be using ForceText serialization since we search the text of the prefab file
      if ( EditorSettings.serializationMode != SerializationMode.ForceText )
      {
        GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );
        EditorGUILayout.LabelField( "Asset Serialization Force Text Mode is required to use this tool" );
        GUILayout.EndVertical();
        return;
      }

      EditorGUILayout.Space();

      GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );

      EditorGUILayout.LabelField( "Search Path", (GUIStyle) "IN TitleText" );

      GUILayout.BeginHorizontal();

      GUILayout.Label( settings.path, GUILayout.ExpandWidth( true ) );

      if ( GUILayout.Button( "Browse", GUILayout.Width( 60 ) ) )
      {
        var newPath = EditorUtility.OpenFolderPanel( "Select Folder", settings.path, string.Empty );

        if ( !string.IsNullOrEmpty( newPath ) )
        {
          settings.path = newPath.Replace( $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}".Replace( "\\", "/" ), string.Empty );
        }
      }

      GUILayout.EndHorizontal();

      GUILayout.EndVertical();

      GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );

      settings.currentMode = (SearchMode) GUILayout.Toolbar( (int) settings.currentMode, searchTypeOptions );

      GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );

      switch ( settings.currentMode )
      {
        case SearchMode.StringSearch:
          StringSearchGui();
          break;

        case SearchMode.AssetSearch:
          AssetSearchGui();
          break;
      
      }

      GUILayout.EndVertical();

      GUILayout.EndVertical();
    }

    private void StringSearchGui()
    {
      settings.searchString = EditorGUILayout.TextField( new GUIContent( "Search" ), settings.searchString );

      if ( GUILayout.Button( "Search" ) )
      {
        Search( SearchPrefabText );
      }
    }

    private void AssetSearchGui()
    {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField( "Asset", GUILayout.Width( 50 ) );

      assetFile = EditorGUILayout.ObjectField( assetFile, typeof(UnityEngine.Object), false );

      GUILayout.EndHorizontal();

      if ( GUILayout.Button( "Search" ) )
      {
        SearchForAsset( assetFile );
      }
    }

    private void SearchForAsset( UnityEngine.Object asset )
    {
      assetFile = asset;

      settings.currentMode = SearchMode.AssetSearch;

      if ( assetFile == null )
      {
        return;
      }

      Search( SearchPrefabAssetReferences );
    }

    private void Search( Func<FileInfo, bool> searchAction )
    {
      var foundFiles = new List<string>();

      ReferenceSearchUtility.ExecuteAssetSearchAtPath( settings.path, ".asset", ( file ) =>
      {
        if ( searchAction( file ) )
        {
          foundFiles.Add( GetProjectRelativePath( file.FullName ) );
        }
      } );

      //Get Assets for found files
      var assets = new List<UnityEngine.Object>();
      for ( int i = 0; i < foundFiles.Count; i++ )
      {
        var path = foundFiles[i];
        EditorUtility.DisplayProgressBar( "Fetching Asset References", path, i / (float) foundFiles.Count );
        assets.Add( AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) );
      }

      EditorUtility.ClearProgressBar();

      SearchResultWindow.SetFiles( assets, assetFile );
    }

    private bool SearchPrefabText( FileInfo fileInfo )
    {
      using ( var stream = fileInfo.OpenText() )
      {
        var text = stream.ReadToEnd();
        if ( !string.IsNullOrEmpty( settings.searchString ) && text.Contains( settings.searchString ) )
        {
          return true;
        }
      }

      return false;
    }

    private bool SearchPrefabAssetReferences( FileInfo fileInfo )
    {
      if ( assetFile == null )
      {
        return false;
      }

      using ( var stream = fileInfo.OpenText() )
      {
        var text = stream.ReadToEnd();

        if ( !AssetDatabase.TryGetGUIDAndLocalFileIdentifier( assetFile, out var guid, out long fileId ) )
        {
          return false;
        }

        if ( text.Contains( $"fileID: {fileId}, guid: {guid}" ) )
        {
          return true;
        }
      }

      return false;
    }

    private static string GetProjectRelativePath( string fullPath )
    {
      return fullPath.Replace($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}", string.Empty ).Replace( "\\", "/" );
    }

    #region Menu Items

    [MenuItem( "Assets/Search for References/Everywhere", false, 25 )]
    public static void SearchEverywhere()
    {
      var window = GetWindow();
      var searchTarget = Selection.activeObject;
      window.SearchForAsset( searchTarget );
      SearchResultWindow.SearchForFileInAllScenes( searchTarget );
    }

    [MenuItem( "Assets/Search for References/In Prefabs", false, 25 )]
    public static void SearchForSelectedObject()
    {
      var window = GetWindow();
      window.SearchForAsset( Selection.activeObject );
    }

    public static void Find( UnityEngine.Object asset )
    {
      var window = GetWindow();
      window.SearchForAsset( asset );
    }

    [MenuItem( "Gameframe/Search/PrefabSearch" )]
    private static void OpenWindow()
    {
      var window = GetWindow();
      window.Show();
    }

    #endregion
  }
}
