using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class BuildExtender {


    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        // Creates a new /data folder in the build output folder, where saves will be put
        int trimIdx = pathToBuiltProject.LastIndexOf('/');
        string outputFolder = pathToBuiltProject.Substring(0, trimIdx);

        string newDirectory = outputFolder + "/data";

        Debug.Log("BuildExtender::OnPostprocessBuild - Adding data folder for saves @ " + newDirectory);
        System.IO.Directory.CreateDirectory(newDirectory);
    }
}
