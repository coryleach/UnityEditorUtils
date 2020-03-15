using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameframe.EditorUtils
{
    public static class MenuItems
    {
        [MenuItem("Gameframe/Open/PersistentDataPath")]
        public static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
        
        [MenuItem("Gameframe/PlayerPrefs/ClearAll")]
        public static void PlayerPrefsClear()
        {
            if (EditorUtility.DisplayDialog("PlayerPrefs", "Delete all player prefs?", "OK", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                EditorUtility.DisplayDialog("PlayerPrefs", "All PlayerPrefs have been Deleted.", "OK");
            }
        }
    }
}


