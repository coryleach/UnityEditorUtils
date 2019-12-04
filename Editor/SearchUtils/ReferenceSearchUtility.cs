using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Gameframe.EditorUtils
{
  public static class ReferenceSearchUtility
  {
    private const string SearchingMessage = "Searching...";

    public static void FindMonoBehaviourReferencesInScene( MonoBehaviour behaviour, List<Object> results )
    {
      if ( !behaviour.gameObject.scene.IsValid() )
      {
        return;
      }

      var searchObjects = Object.FindObjectsOfType<MonoBehaviour>();

      //Using a HashSet so we wont add the same game object to our list multiple times
      var foundReferences = new HashSet<Object>();

      //JsonUtility will output references with the format "instanceID":value
      var searchText = $"\"instanceID\":{behaviour.GetInstanceID()}";

      for ( var i = 0; i < searchObjects.Length; i++ )
      {
        EditorUtility.DisplayProgressBar( "Searching...", searchObjects[i].name, i / (float) searchObjects.Length );
        //Serializing the entire component to json so we can search the text for a reference
        //When checking for Scene object to Scene Object references we need to use the regular Json utility. EditorJsonUtility does not output instance ids.
        var json = JsonUtility.ToJson( searchObjects[i] );
        if ( json.Contains( searchText ) )
        {
          foundReferences.Add( searchObjects[i] );
        }
      }

      EditorUtility.ClearProgressBar();

      results.AddRange( new List<Object>( foundReferences ) );

      Selection.objects = results.ToArray();
    }

    public static void FindGameObjectReferencesInOpenScene( GameObject target, List<Object> results )
    {
      if ( !target.scene.IsValid() )
      {
        //target is a prefab asset, do normal asset search
        FindAssetReferenceInOpenScenes( target, results );
        return;
      }

      var searchObjects = Object.FindObjectsOfType<MonoBehaviour>();

      //Using a HashSet so we wont add the same game object to our list multiple times
      var foundReferences = new HashSet<Object>();

      //JsonUtility will output references with the format "instanceID":value
      var searchText = $"\"instanceID\":{target.GetInstanceID()}";

      for ( var i = 0; i < searchObjects.Length; i++ )
      {
        EditorUtility.DisplayProgressBar( "Searching...", searchObjects[i].name, i / (float) searchObjects.Length );
        //Serializing the entire component to json so we can search the text for a reference
        //When checking for Scene object to Scene Object references we need to use the regular Json utility. EditorJsonUtility does not output instance ids.
        var json = JsonUtility.ToJson( searchObjects[i] );
        if ( json.Contains( searchText ) )
        {
          foundReferences.Add( searchObjects[i] );
        }
      }

      EditorUtility.ClearProgressBar();

      results.AddRange( new List<Object>( foundReferences ) );

      Selection.objects = results.ToArray();
    }

    public static void FindAssetReferenceInOpenScenes( Object asset, List<Object> results )
    {
      var searchObjects = new List<Component>();
      var searchType = typeof(MonoBehaviour);

      if ( asset is MonoScript scriptAsset )
      {
        //Searching for a MonoScript means searching for instances of the type
        searchType = scriptAsset.GetClass();

        if ( !searchType.IsSubclassOf( typeof(Component) ) )
        {
          EditorUtility.DisplayDialog( "I am error", "Scene script search only works for MonoBehaviour types.", "Ok" );
          return;
        }
      }

      //Get all components in all open scenes
      var sceneSetups = EditorSceneManager.GetSceneManagerSetup();
      foreach ( var sceneSetup in sceneSetups )
      {
        if (!sceneSetup.isLoaded)
        {
          continue;
        }
        
        var scene = SceneManager.GetSceneByPath( sceneSetup.path );
        var gameObjects = scene.GetRootGameObjects();
        foreach ( var gameObject in gameObjects )
        {
          searchObjects.AddRange( gameObject.GetComponentsInChildren( searchType, true ) );
        }
      }

      //Using a hashset so we wont add the same game object to our list multiple times
      var foundReferences = new HashSet<Object>();

      if ( !AssetDatabase.TryGetGUIDAndLocalFileIdentifier( asset, out var guid, out long fileId ) )
      {
        EditorUtility.DisplayDialog( "I am Error", "Object not found in Asset Database.", "Ok" );
        return;
      }

      //EditorJsonUtility will output references with the format "fileID":value,"guid":"value"
      var searchText = $"\"fileID\":{fileId},\"guid\":\"{guid}\"";

      for ( var i = 0; i < searchObjects.Count; i++ )
      {
        EditorUtility.DisplayProgressBar( "Searching...", searchObjects[i].name, i / (float) searchObjects.Count );
        if ( asset is MonoScript )
        {
          foundReferences.Add( searchObjects[i].gameObject );
        }
        else
        {
          //Serializing the entire component to json so we can search the text for a reference
          //We must use EditorJsonUtility and not JsonUtility because the editor version outputs the file and guid values
          var json = EditorJsonUtility.ToJson( searchObjects[i] );
          if ( json.Contains( searchText ) )
          {
            foundReferences.Add( searchObjects[i] );
          }
        }
      }

      EditorUtility.ClearProgressBar();

      results.AddRange( new List<Object>( foundReferences ) );

      Selection.objects = results.ToArray();
    }

    private static void FindAssetReferenceInScene( Scene scene, Object asset, List<Object> results )
    {
      var searchObjects = new List<Component>();
      var searchType = typeof(MonoBehaviour);

      if ( asset is MonoScript scriptAsset )
      {
        //Searching for a MonoScript means searching for instances of the type
        searchType = scriptAsset.GetClass();

        if ( !searchType.IsSubclassOf( typeof(Component) ) )
        {
          EditorUtility.DisplayDialog( "I am error", "Scene script search only works for MonoBehaviour types.", "Ok" );
          return;
        }
      }

      //Get all components in all open scenes
      var gameObjects = scene.GetRootGameObjects();
      foreach ( var gameObject in gameObjects )
      {
        searchObjects.AddRange( gameObject.GetComponentsInChildren( searchType, true ) );
      }

      //Using a hashset so we wont add the same game object to our list multiple times
      var foundReferences = new HashSet<Object>();

      if ( !AssetDatabase.TryGetGUIDAndLocalFileIdentifier( asset, out var guid, out long fileId ) )
      {
        EditorUtility.DisplayDialog( "I am Error", "Object not found in Asset Database.", "Ok" );
        return;
      }

      //EditorJsonUtility will output references with the format "fileID":value,"guid":"value"
      string searchText = $"\"fileID\":{fileId},\"guid\":\"{guid}\"";

      for ( int i = 0; i < searchObjects.Count; i++ )
      {
        EditorUtility.DisplayProgressBar($"Searching {scene.name}", searchObjects[i].name,
          i / (float) searchObjects.Count );
        if ( asset is MonoScript )
        {
          foundReferences.Add( searchObjects[i].gameObject );
        }
        else
        {
          //Serializing the entire component to json so we can search the text for a reference
          //We must use EditorJsonUtility and not JsonUtility because the editor version outputs the file and guid values
          var json = EditorJsonUtility.ToJson( searchObjects[i] );
          if ( json.Contains( searchText ) )
          {
            foundReferences.Add( searchObjects[i] );
          }
        }
      }

      EditorUtility.ClearProgressBar();

      results.AddRange( new List<Object>( foundReferences ) );

      Selection.objects = results.ToArray();
    }

    public static void FindAssetReferencesInBuildScenes( Object asset, List<Object> results )
    {
      // Give user option to save/cancel
      if ( !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() )
      {
        return;
      }

      var activeScene = EditorSceneManager.GetActiveScene();

      var sceneObjects = new List<Object>();
      //Open All Scenes
      for ( int i = 0; i < EditorBuildSettings.scenes.Length; i++ )
      {
        EditorUtility.DisplayProgressBar( "Opening Scenes", EditorBuildSettings.scenes[i].path,
          i / (float) EditorBuildSettings.scenes.Length );
        var scene = EditorSceneManager.OpenScene( EditorBuildSettings.scenes[i].path, OpenSceneMode.Additive );
        sceneObjects.Clear();
        FindAssetReferenceInScene( scene, asset, sceneObjects );
        results.AddRange( sceneObjects );
        //Close any scene that had zero results (except the active scene)
        if ( scene != activeScene && sceneObjects.Count <= 0 )
        {
          EditorSceneManager.CloseScene( scene, true );
        }
      }
    }

    public static void ExecuteAssetSearchAtPath( string path, string assetExtension, Action<FileInfo> action )
    {
      EditorUtility.DisplayProgressBar( SearchingMessage, path, 0 );
      ProcessFileInDirectory( path, assetExtension, action );
      EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// Calls the specified action on each file in the path
    /// </summary>
    private static void ProcessFileInDirectory( string path, string assetExtension, Action<FileInfo> action,
      bool recursive = true )
    {
      var assetDirectory = new DirectoryInfo( path );
      var assetRegex = $"*{assetExtension}";
      var files = assetDirectory.GetFiles( assetRegex );

      for ( var i = 0; i < files.Length; i++ )
      {
        EditorUtility.DisplayProgressBar( SearchingMessage, path, i / (float) files.Length );
        action( files[i] );
      }

      if ( !recursive )
      {
        return;
      }

      IEnumerable<DirectoryInfo> subdirectories = assetDirectory.GetDirectories();
      foreach ( var subdirectory in subdirectories )
      {
        ProcessFileInDirectory($"{path}{Path.DirectorySeparatorChar}{subdirectory.Name}",
          assetExtension, action, recursive );
      }
    }
    
    /// <summary>
    /// Write some text to a file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="text"></param>
    private static void WriteFile( FileInfo file, string text )
    {
      try
      {
        //Remove the readonly flag if there is one
        if ( file.IsReadOnly )
        {
          File.SetAttributes( file.FullName, FileAttributes.Normal );
        }

        File.WriteAllText( file.FullName, text );
      }
      catch ( Exception e )
      {
        Debug.LogErrorFormat( "Unable to write to file {0}\n{1}", file.FullName, e );
      }
    }

    /// <summary>
    /// Search all assumblies for a given type
    /// </summary>
    /// <param name="typeName">type name to search for</param>
    /// <returns>name of the assembly containing the typeName</returns>
    public static string SearchAssembliesForType( string typeName )
    {
      List<string> list = new List<string>();

      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach ( var assembly in assemblies )
      {
        if ( assembly.GetType( typeName ) != null )
        {
          list.Add( assembly.GetName().Name );
        }
      }

      if ( list.Count > 1 )
      {
        Debug.LogErrorFormat( "More than one assembly contains type named {0}", typeName );
      }

      return list.Count == 1 ? list[0] : null;
    }

    /// <summary>
    /// Parse a type name into just the assembly name
    /// </summary>
    /// <param name="typeName">full type name including assembly</param>
    /// <returns>assembly name</returns>
    private static string GetAssemblyNameFromTypeName( string typeName )
    {
      var parts = typeName.Split( ',' );
      return parts.Length < 2 ? string.Empty : parts[1].Trim();
    }

    /// <summary>
    /// Parse the basic class-only type name from the full type name
    /// </summary>
    /// <param name="typeName">full type name including the assembly</param>
    /// <returns>class type name without the assembly</returns>
    private static string GetTypeNameWithoutAssembly( string typeName )
    {
      var parts = typeName.Split( ',' );
      return parts.Length < 2 ? string.Empty : parts[0].Trim();
    }

    /// <summary>
    /// Takes a full path and converts it to a path relative to the project directory
    /// </summary>
    /// <param name="fullPath">fully qualified path</param>
    /// <returns>path relative to the current directory</returns>
    public static string GetProjectRelativePath( string fullPath ) => fullPath
      .Replace($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}", string.Empty )
      .Replace( "\\", "/" );
    
  }
}
