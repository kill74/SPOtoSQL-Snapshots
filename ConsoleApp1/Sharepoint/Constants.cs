using System;
using System.IO;
using System.Xml.Serialization;

[XmlRoot("Configuration")]
public class Configuration
{
    public string SQL_CON_STRING { get; set; }
    public string SPO_USER { get; set; }
    public string SPO_PASSWORD { get; set; }
    public string SNAP_DATE { get; set; }
    public string UNIT_PROJECT_ID_IN { get; set; }
    public string UNIT_UNIT_TYPE_IN { get; set; }
    public string IR_UNIT_IN { get; set; }
    public string MAIN_APPROVER_IN { get; set; }
    public string OPTIONAL_APPROVER_IN { get; set; }
    public string FINANCIAL_APPROVER_IN { get; set; }
    public string PROJECT_ID_IN { get; set; }
    public string HR_DISPLAY_NAME_IN { get; set; }
    public string HR_APPROVER_IN { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // This will load the XML configuration file and deserialize it into the Configuration object
            string filePath = "ConfigXml/Xml_Credenciais.xml";
            Configuration config = LoadConfiguration(filePath);

            Console.WriteLine("SQL Connection String: " + config.SQL_CON_STRING);
            Console.WriteLine("SharePoint User: " + config.SPO_USER);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static Configuration LoadConfiguration(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            return (Configuration)serializer.Deserialize(fs);
        }
    }
}