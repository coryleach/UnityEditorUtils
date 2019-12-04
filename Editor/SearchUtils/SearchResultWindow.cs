using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameframe.EditorUtils
{
  public class SearchResultWindow : EditorWindow
  {
    private static string EvenBackground = "CN EntryBackEven";
    private static string OddBackground = "CN EntryBackodd";

    private List<Object> results = new List<Object>();
    private Vector2 scrollPosition = Vector2.zero;
    private Object context = null;

    #region EditorWindow

    private void OnGUI()
    {
      DrawResults();
    }

    private void OnSelectionChange()
    {
      Repaint();
    }

    #endregion

    #region Private Methods

    private string drawingScene = string.Empty;

    private static Texture GetIconForObject( Object target )
    {
      var targetGameObject = target as GameObject;
      if ( targetGameObject != null && !targetGameObject.scene.IsValid() )
      {
        //Prefab Icon
        return EditorGUIUtility.IconContent( "PrefabNormal Icon" ).image;
      }
      var defualtThumbnail = AssetPreview.GetMiniTypeThumbnail( target.GetType() );
      if ( defualtThumbnail != null )
      {
        return defualtThumbnail;
      }
      if ( target is MonoBehaviour )
      {
        return EditorGUIUtility.IconContent( "cs Script Icon" ).image;
      }
      return EditorGUIUtility.IconContent( "DefaultAsset Icon" ).image;
    }

    private void DrawResults()
    {
      Event currentEvent = Event.current;

      if ( context != null )
      {
        GUILayout.BeginHorizontal( "GroupBox" );
        GUILayout.Label( "Searched" );
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
        GUI.Label( rect, new GUIContent( context.name, GetIconForObject( context ) ), OddBackground );
        GUILayout.EndHorizontal();

        //On Left Click
        if ( currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains( currentEvent.mousePosition ) )
        {
          Selection.SetActiveObjectWithContext( context, this );
          Repaint();
        }
      }

      if ( results.Count <= 0 )
      {
        GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );
        EditorGUILayout.LabelField( "No Results" );
        GUILayout.EndVertical();
        return;
      }

      GUILayout.BeginVertical( new GUIStyle( "GroupBox" ) );
      scrollPosition = EditorGUILayout.BeginScrollView( scrollPosition );
      drawingScene = string.Empty;

      for ( int i = 0; i < results.Count; i++ )
      {
        if ( results[ i ] == null )
        {
          continue;
        }

        GameObject resultGameObject = null;
        string rowText = results[ i ].name;

        //Is our result object in a scene or is it an asset?
        if ( results[ i ] is GameObject )
        {
          resultGameObject = ( GameObject )results[ i ];
        }
        else if ( results[ i ] is Component )
        {
          resultGameObject = ( ( Component )results[ i ] ).gameObject;
          rowText = $"{results[i].name} ▶ {results[i].GetType()}";
        }

        GUIContent content = EditorGUIUtility.ObjectContent( results[ i ], results[ i ].GetType() );
        content.text = rowText;

        var locationName = "Assets";

        if ( resultGameObject != null )
        {
          //If we're in a valid scene let's use the Scene info
          if ( resultGameObject.scene.IsValid() )
          {
            locationName = resultGameObject.scene.name;
          }
          else
          {
            //If game object has no valid scene it is a prefab
            locationName = "Prefabs";
            content.image = EditorGUIUtility.IconContent( "PrefabNormal Icon" ).image;
          }
        }

        if ( drawingScene != locationName )
        {
          drawingScene = locationName;
          var style = ( GUIStyle )"toolbarbutton";
          style.alignment = TextAnchor.MiddleLeft;
          switch ( locationName )
          {
            case "Prefabs":
              GUILayout.Label( new GUIContent( locationName, EditorGUIUtility.IconContent( "PrefabNormal Icon" ).image ), style );
              break;
            case "Assets":
              GUILayout.Label( new GUIContent( locationName, AssetPreview.GetMiniTypeThumbnail( typeof( ScriptableObject ) ) ), style );
              break;
            default:
              GUILayout.Label( new GUIContent( locationName, AssetPreview.GetMiniTypeThumbnail( typeof( SceneAsset ) ) ), style );
              break;
          }
        }

        GUIStyle background = i % 2 == 0 ? OddBackground : EvenBackground;
        //Draw Game Object Row
        if ( Selection.activeObject == results[ i ] )
        {
          GUI.backgroundColor = new Color( 0.5f, 0.8f, 1f, 1f );
        }
        var rect = EditorGUILayout.GetControlRect();
        background.imagePosition = ImagePosition.ImageLeft;
        background.alignment = TextAnchor.MiddleLeft;
        background.padding.left = 0;
        EditorGUI.LabelField( rect, new GUIContent( rowText, GetIconForObject( results[ i ] ) ), background );
        GUI.backgroundColor = Color.white;

        //On Left Click
        if ( currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains( currentEvent.mousePosition ) )
        {
          Selection.SetActiveObjectWithContext( results[ i ], this );
          Repaint();
        }
        if ( currentEvent.type == EventType.MouseDown && currentEvent.button == 1 && rect.Contains( currentEvent.mousePosition ) )
        {
          var menu = new GenericMenu();

          if ( resultGameObject != null && !resultGameObject.scene.IsValid() )
          {
            //Menu Options for a Prefab
            menu.AddItem( new GUIContent( "Open" ), false, OpenInPreviewScene, resultGameObject );
            menu.AddItem( new GUIContent( "Open and Search" ), false, OpenInPreviewSceneAndSearch, resultGameObject );
          }

          menu.ShowAsContext();
          currentEvent.Use();
        }
      }

      EditorGUILayout.EndScrollView();
      GUILayout.EndVertical();

      if ( results.Count > 0 )
      {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if ( GUILayout.Button( "Clear" ) )
        {
          results.Clear();
          context = null;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space( 5 );
      }

    }

    private void OpenInPreviewScene( object prefab )
    {
      var path = AssetDatabase.GetAssetPath((Object)prefab);
      PrefabUtility.LoadPrefabContents(path);
    }

    private void OpenInPreviewSceneAndSearch( object prefab )
    {
      var path = AssetDatabase.GetAssetPath((Object)prefab);
      PrefabUtility.LoadPrefabContents(path);
      ReferenceSearchUtility.FindAssetReferenceInOpenScenes( context, results );
      scrollPosition = new Vector2( 0, float.MaxValue );
      Repaint();
    }

    #endregion

    #region Static & Menu Methods

    private static SearchResultWindow OpenWindow()
    {
      var window = GetWindow<SearchResultWindow>( "Search Result" );
      window.Show();
      return window;
    }

    public static void Find( Object asset )
    {
      var window = OpenWindow();
      window.context = asset;
      window.results.Clear();
      ReferenceSearchUtility.FindAssetReferenceInOpenScenes( asset, window.results );
    }

    public static void SetFiles( List<Object> files, Object context )
    {
      var window = OpenWindow();
      window.context = context;
      window.results = files;
      window.Repaint();
    }

    [MenuItem( "Assets/Search for References/In Open Scenes", false, 25 )]
    public static void SearchForFile()
    {
      var window = OpenWindow();
      window.context = Selection.activeObject;
      window.results.Clear();
      ReferenceSearchUtility.FindAssetReferenceInOpenScenes( Selection.activeObject, window.results );
    }

    [MenuItem( "Assets/Search for References/In Open Scenes", true, 25 )]
    public static bool CanSearchForFile()
    {
      return ( Selection.activeObject != null );
    }

    [MenuItem( "Assets/Search for References/In Build Scenes", false, 25 )]
    private static void ExecuteSearchForFileInAllScenes()
    {
      SearchForFileInAllScenes( Selection.activeObject, clearPreviousResults: true );
    }

    public static void SearchForFileInAllScenes( Object asset, bool clearPreviousResults = false )
    {
      var window = OpenWindow();
      window.context = asset;

      if ( clearPreviousResults )
      {
        window.results.Clear();
      }

      List<Object> sceneObjects = new List<Object>();
      ReferenceSearchUtility.FindAssetReferencesInBuildScenes( asset, sceneObjects );
      //Result should be a list of GameObjects
      //Sort them by scene
      sceneObjects.Sort( ( a, b ) =>
      {
        var gameObjectA = a is Component ? ( ( Component )a ).gameObject : ( GameObject )a;
        var gameObjectB = b is Component ? ( ( Component )b ).gameObject : ( GameObject )b;

        var sceneA = gameObjectA.scene;
        var sceneB = gameObjectB.scene;

        return string.Compare( sceneA.name, sceneB.name, System.StringComparison.CurrentCulture );
      } );
      window.results.AddRange( sceneObjects );
    }

    [MenuItem( "Assets/Search for References/In Build Scenes", true, 25 )]
    private static bool CanSearchForFileInAllScenes()
    {
      return ( Selection.activeObject != null );
    }

    [MenuItem( "GameObject/Search for References", false, -50 )]
    public static void SearchForGameObject()
    {
      var window = OpenWindow();
      //Result should be a list of components referencing the given object
      window.context = Selection.activeObject;
      window.results.Clear();
      ReferenceSearchUtility.FindGameObjectReferencesInOpenScene( Selection.activeObject as GameObject, window.results );
    }

    [MenuItem( "GameObject/Search for References", true, -50 )]
    private static bool CanSearchForGameObject()
    {
      return Selection.activeObject is GameObject;
    }

    [MenuItem( "CONTEXT/MonoBehaviour/Search Open Scenes for References", false, 100 )]
    public static void MonobehaviourSearchForReferencesInOpenScenes( MenuCommand command )
    {
      var window = OpenWindow();

      //Result should be a list of components referencing the given object
      window.context = command.context;
      window.results.Clear();
      ReferenceSearchUtility.FindMonoBehaviourReferencesInScene( command.context as MonoBehaviour, window.results );
    }

    [MenuItem( "CONTEXT/MonoBehaviour/Search Open Scenes for References", true, 100 )]
    private static bool CanMonobehaviourSearchForReferencesInOpenScenes( MenuCommand command )
    {
      return command.context is MonoBehaviour;
    }

    #endregion

  }
}
