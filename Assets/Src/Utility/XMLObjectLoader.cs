using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


public abstract class XMLObjectLoader {

    public delegate void PostLoadCallback<T>(T loaded);

    /// <summary>
    /// Loads the xml object found at the path. Has optional callback method whenthe load has been completed. Loads the file from within
    /// the packaged resources in a build, ie. the Resources folder in the Editor. As such, file name shouldnt include extentions
    /// </summary>
    public static T LoadXMLObjectInternal<T>(string fileName, PostLoadCallback<T> postLoadCallback = null) where T : class {
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
            Debug.LogWarning("XMLObjectLoader::LoadXMLObjectInternal could not find file [" + filePath + "] :: " + e.Message);
            return null;
        }
    }


    /// <summary>
    /// Same as internal functionally, except it loads files external to the build. Safe handling when the folder does not exist, 
    /// as it will create folders / files that do not exist yet. File name should include extension
    /// </summary>
    public static T LoadXMLObjectExternal<T>(string fileName, PostLoadCallback<T> postLoadCallback = null) where T : class {
        XmlSerializer serializer = new XmlSerializer(typeof(T));

        // step up once to get out of assets / data folder
        string filePath = Application.dataPath + "/../" + fileName;

        try {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            T loader = serializer.Deserialize(stream) as T;
            if(postLoadCallback != null) {
                postLoadCallback(loader);
            }

            stream.Close();

            return loader;
        }
        catch(Exception e) {
            Debug.LogWarning("XMLObjectLoader::LoadXMLObjectExternal could not find file [" + filePath + "] :: " + e.Message);
            Debug.Log("XMLObjectLoader::LoadXMLObjectExternal will attempt to create the file at the directory");

            try {
                FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);

                XmlSerializer instanceSerializer = new XmlSerializer(typeof(T));
                instanceSerializer.Serialize(stream, Activator.CreateInstance(typeof(T)));
            }
            catch(Exception e2) {
                Debug.LogWarning("XMLObjectLoader::LoadXMLObjectExternal failed to create the missing file :: " + e2.Message);
            }

            return null;
        }
    }



    /// <summary>
    /// Saves the file externally, doe snot work for packaged files. File must include the extenstion. As such, the file 
    /// must also exist already. If it was loaded uses external loader, it is almost sure to exist.
    /// </summary>
    public static void SaveXMLObject<T>(string file, T instance) {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        string filePath = Application.dataPath + "/../" + file;

        using(TextWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8)) //Set encoding
        {
            serializer.Serialize(sw, instance);
            Debug.Log("XMLObjectLoader::LoadXMLObject saved file: " + filePath);
        }

    }

}

