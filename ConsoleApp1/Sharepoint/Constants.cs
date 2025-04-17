// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.Constants
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe


// This can be removed as it is not used in the code 
// We have the Xml_Credencials file
namespace Bring.Sharepoint
{
  public class Constants
  {
// FJ -> APAGUEI AS CREDENCIAIS QUE ESTÂO EM USO
// Isto não pode estar no código... estas constantes têm de passar para um ficheiro de configuração
// Connection string used to connect to a Microsoft SQL Server database.
// It includes the server address, database name, and login credentials.
// WARNING: Hardcoding passwords like this is not recommended for production code
public const string SQL_CON_STRING = "Server=109.71.46.223;Database=LAKEDB;Persist Security Info=False;User ID=SQL_Server_user;Password=pass";

// SharePoint Online username used for authentication
public const string SPO_USER = "USERNAME";

// SharePoint Online password for the corresponding user account
public const string SPO_PASSWORD = "PASSWORD";

// A fixed "snapshot" date in string format (YYYY-MM-DD HH:MM:SS.MMM)
public const string SNAP_DATE = "2100-01-01 00:00:00.000";

// Internal SharePoint field name for the "Unit:Project ID" field
public const string UNIT_PROJECT_ID_IN = "Unit_x003a_Project_x0020_ID";

// Internal SharePoint field name for the "Unit:Unit type" field
public const string UNIT_UNIT_TYPE_IN = "Unit_x003a_Unit_x0020_type";

// Internal SharePoint field name for the "Unit/Project:Project0" field
public const string IR_UNIT_IN = "Unit_x002f_Project_x003a_Project0";

// Internal SharePoint field name for the "Main approver" field
public const string MAIN_APPROVER_IN = "Main_x0020_approver";

// Internal SharePoint field name for the "Optional approver" field
public const string OPTIONAL_APPROVER_IN = "Optional_x0020_approver";

// Internal SharePoint field name for the "Financial approver" field
public const string FINANCIAL_APPROVER_IN = "Financial_x0020_approver";

// Internal SharePoint field name for the "Project ID" field
public const string PROJECT_ID_IN = "Project_x0020_ID";

// Internal SharePoint field name for the "Display Name" field (usually for HR records or user profiles)
public const string HR_DISPLAY_NAME_IN = "Display_x0020_Name";

// Internal SharePoint field name for the "Approver1" field, probably an HR approval field
public const string HR_APPROVER_IN = "Approver1";

  }
}
