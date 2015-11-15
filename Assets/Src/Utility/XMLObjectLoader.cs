using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


public abstract class XMLObjectLoader {

    public delegate void PostLoadCallback<T>(T loaded);

    /// <summary>
    /// Loads the xml object found at the path. Has optional callback method whenthe load has been completed
    /// </summary>
    public static T LoadXMLObject<T>(string fileName, PostLoadCallback<T> postLoadCallback = null) where T : class {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        string filePath = fileName;

        // First load the asset as a text file, and read it as a xml doc
        TextAsset textFile = (TextAsset)Resources.Load(filePath, typeof(TextAsset));
        if(textFile == null) {
            Debug.LogWarning("XMLObjectLoader::LoadXMLObject failed to load text asset [" + filePath + "]");
            return null;
        }

        // Fromt the text file, create an xml reader for the data stream
        MemoryStream assetStream = new MemoryStream(textFile.bytes);
        XmlReader reader = XmlReader.Create(assetStream);

        try {
            //FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            T loader = serializer.Deserialize(reader) as T;

            Debug.Log("Loaded " + typeof(T).Name + " @ " + fileName);

            if(postLoadCallback != null) {
                postLoadCallback(loader);
            }

            //stream.Close();
            reader.Close();

            return loader;
        }
        catch(Exception e) {
            Debug.LogWarning("XMLObjectLoader::LoadXMLObject could find file [" + filePath + "] :: " + e.Message);
            return null;
        }
    }





    public static void SaveXMLObject<T>(string file, T instance) {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        string filePath = Application.dataPath + "/Resources/" + file + ".xml";

        using(TextWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8)) //Set encoding
        {
            serializer.Serialize(sw, instance);
            Debug.Log("XMLObjectLoader::LoadXMLObject saved file: " + filePath);
        }

    }

}

