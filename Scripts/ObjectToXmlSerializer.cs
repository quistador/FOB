using UnityEngine; 
using System.Collections; 
using System.Xml; 
using System.Xml.Serialization; 
using System.IO; 
using System.Text; 
 
public class ObjectToXmlSerializer
{
    public string SerializeUnitsToFile(string profileName, object data)
    {
        string saveFolder = Application.dataPath;    
        string saveFile = profileName + ".xml";
        string serialized = this.SerializeObject(data); 
        Debug.Log("Saving to: "+saveFolder+saveFile); 
        CreateXML(serialized, saveFolder, saveFile);
        return serialized;
    }

    public Army DeserializeUnitsFromFile(string fileName)
    {
        string xml = this.LoadXML(fileName);
        Army units = (Army)this.DeserializeObject(xml);
        return units;
    }
     
    /* The following metods came from the referenced URL */ 
    string UTF8ByteArrayToString(byte[] characters) 
    {      
        UTF8Encoding encoding = new UTF8Encoding(); 
        string constructedString = encoding.GetString(characters); 
        return (constructedString); 
    } 
  
    byte[] StringToUTF8ByteArray(string pXmlString) 
    { 
        UTF8Encoding encoding = new UTF8Encoding(); 
        byte[] byteArray = encoding.GetBytes(pXmlString); 
        return byteArray; 
    } 
  
    // Here we serialize our object of type Army 
    private string SerializeObject(object pObject) 
    { 
        string XmlizedString = null; 
        MemoryStream memoryStream = new MemoryStream(); 
        XmlSerializer xs = new XmlSerializer(typeof(Army)); 
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
        xs.Serialize(xmlTextWriter, pObject); 
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray()); 
        return XmlizedString; 
    } 
  
    // Here we deserialize it back into its original form 
    object DeserializeObject(string pXmlizedString) 
    { 
        XmlSerializer xs = new XmlSerializer(typeof(Army)); 
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString)); 
        return xs.Deserialize(memoryStream); 
    } 
  
    // Finally our save and load methods for the file itself 
    void CreateXML(string dataToSerialize, string folder, string fileName) 
    { 
        StreamWriter writer; 
        FileInfo saveFile = new FileInfo(folder+"\\"+ fileName); 
        if(!saveFile.Exists) 
        { 
           writer = saveFile.CreateText(); 
        } 
        else 
        { 
           saveFile.Delete(); 
           writer = saveFile.CreateText(); 
        } 

        writer.Write(dataToSerialize); 
        writer.Close(); 
        Debug.Log("File written."); 
    } 

    public string[] SavedProfiles
    {
        get
        {
            DirectoryInfo saveDir = new DirectoryInfo(Application.dataPath);
            FileInfo[] profiles = saveDir.GetFiles("char*xml");

            // replace with characters once we get this figured out. 
            return new string[]{};
        }
    }

    private string LoadXML(string profileFile) 
    { 
        StreamReader r = File.OpenText(Application.dataPath+"\\"+ profileFile); 
        string _info = r.ReadToEnd(); 
        r.Close(); 
        Debug.Log("File Read"); 
        return _info; 
    } 
}
