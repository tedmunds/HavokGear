using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


public abstract class XMLObjectLoader {

    public delegate void PostLoadCallback<T>(T loaded);

    /// <summary>
    /// Loads the xml object found at the path. Has optional callback method whenthe load has been completed
    /// </summary>
    public static T LoadXMLObject<T>(string file, PostLoadCallback<T> postLoadCallback = null) where T : class {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        string filePath = file;

        try {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            T loader = serializer.Deserialize(stream) as T;

            Debug.Log("Loaded " + typeof(T).Name + " @ " + file);

            if(postLoadCallback != null) {
                postLoadCallback(loader);
            }

            stream.Close();

            return loader;
        }
        catch(IOException e) {
            Debug.LogWarning("XMLObjectLoader::LoadXMLObject could find file [" + filePath + "] :: " + e.Message);
            return null;
        }
    }





    public static void SaveXMLObject<T>(string file, T instance) {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        string filePath = file;

        try {
            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            serializer.Serialize(stream, instance);

            Debug.Log("Saved " + typeof(T).Name + " @ " + file);

            stream.Close();
        }
        catch(IOException e) {
            Debug.LogWarning("XMLObjectLoader::SaveXMLObjec could save to file [" + filePath + "] :: " + e.Message);
            return;
        }
    }
}

