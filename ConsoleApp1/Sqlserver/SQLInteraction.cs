// Decompiled with JetBrains decompiler
// Type: Bring.Sqlserver.SQLInteraction
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Bring.Sharepoint;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace Bring.Sqlserver
{
    internal class SQLInteraction
    {
        public SqlConnection Connection { get; set; } // SQL Server connection instance
        public SqlCommand Command { get; set; } // Command to execute SQL queries
        public SqlTransaction Transaction { get; set; } // SQL transaction for data integrity
        public SPOList List { get; set; } // SharePoint Online list to sync
        public string TableName { get; set; } // Target SQL table name
        public Dictionary<string, Field> FNDictionary { get; set; } // Field mapping dictionary
        public string CurrentTime { get; set; } // Timestamp for snapshot labeling

        // Builds connection, table and metadata setup
        public void Build()
        {
            this.TableName = this.ToPascalCase(this.List.Name, false);
            // TODO: move credentials to a config file (credentials are hardcoded here!)
            this.Connection = new SqlConnection("Server=109.71.46.223;Database=LAKEDB;Persist Security Info=False;User ID=SQL_Server_user;Password=pass");
            this.Connection.Open();

            this.Command = this.Connection.CreateCommand();
            this.Transaction = this.Connection.BeginTransaction(this.TableName + " TXN.");
            this.Command.Connection = this.Connection;
            this.Command.Transaction = this.Transaction;

            this.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            this.List.Build();
            this.FNDictionary = new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase);
            this.BuildDictionary();

            // If table doesn't exist, create it, otherwise update its schema
            if (!this.TableExists(this.TableName))
                this.CreateTable();
            else
                this.UpdateTableDesign();
        }

        // Updates data for a daily sync
        public void DailyUpdate()
        {
            try
            {
                this.Command.CommandText = "DELETE FROM [" + this.TableName + "] WHERE Snapshot = '2100-01-01 00:00:00.000'";
                this.Command.ExecuteNonQuery();

                this.TransferData("2100-01-01 00:00:00.000");
                this.UpdateMetadata();
                this.Transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.Transaction.Rollback();
            }
            Console.WriteLine("Daily Update done for: " + this.TableName);
        }

        // Updates data with the current timestamp
        public void CurrentTimeUpdate()
        {
            try
            {
                this.TransferData(this.CurrentTime);
                this.UpdateMetadata();
                this.Transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.Transaction.Rollback();
            }
            Console.WriteLine(this.CurrentTime + " Update done for: " + this.TableName);
        }

        // Transfers SharePoint list items into SQL table
        private void TransferData(string snapDate)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string sqlColNames = this.GetSQLColNames();

            foreach (ListItem listItem in this.List.ItemCollection)
            {
                stringBuilder.Clear();
                stringBuilder.AppendLine("INSERT INTO [" + this.TableName + "] " + sqlColNames);
                stringBuilder.Append("VALUES ('" + snapDate + "', ");

                foreach (Field field in this.FNDictionary.Values)
                {
                    object obj = listItem[field.InternalName];
                    if (obj != null)
                    {
                        // Type-specific SQL-safe formatting
                        if (obj is FieldLookupValue)
                            stringBuilder.Append("'" + ((FieldLookupValue)obj).LookupId + "', ");
                        else if (obj is FieldUserValue)
                            stringBuilder.Append(((FieldLookupValue)obj).LookupId.ToString() + ", ");
                        else if (obj is FieldUrlValue)
                            stringBuilder.Append("'" + ((FieldUrlValue)obj).Url + "', ");
                        else if (obj is ContentTypeId)
                            stringBuilder.Append(obj.ToString() + ", ");
                        else if (obj is DateTime)
                            stringBuilder.Append($"'{(DateTime)obj:yyyy-MM-dd HH:mm:ss.fff}', ");
                        else if (obj is FieldLookupValue[])
                        {
                            stringBuilder.Append("'");
                            foreach (var lookup in (FieldLookupValue[])obj)
                                stringBuilder.Append(lookup.LookupId + ";");
                            stringBuilder.Append("', ");
                        }
                        else if (obj is FieldUserValue[])
                        {
                            stringBuilder.Append("'");
                            foreach (var user in (FieldUserValue[])obj)
                                stringBuilder.Append(user.LookupId + ";");
                            stringBuilder.Append("', ");
                        }
                        else
                        {
                            if (obj is string str)
                                obj = str.Replace("'", "''");
                            stringBuilder.Append("'" + obj + "', ");
                        }
                    }
                    else
                        stringBuilder.Append("NULL, ");
                }

                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(")");

                this.Command.CommandText = stringBuilder.ToString();
                try
                {
                    this.Command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't insert values: " + ex.Message);
                    Console.WriteLine("INSERT STATEMENT: " + stringBuilder.ToString());
                }
            }
        }

        // Checks whether the target table exists
        private bool TableExists(string listName)
        {
            this.Command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + listName + "'";
            return (int)this.Command.ExecuteScalar() != 0;
        }

        // Generates and executes CREATE TABLE statement
        private void CreateTable()
        {
            StringBuilder stringBuilder = new StringBuilder($"CREATE TABLE [{this.TableName}] (");
            stringBuilder.AppendLine("[Snapshot] datetime NULL,");

            foreach (var fn in this.FNDictionary)
            {
                string sqlType = this.SQLFieldType(fn.Value);
                if (sqlType != null)
                    stringBuilder.AppendLine($"[{fn.Key}] {sqlType} NULL,");
            }

            stringBuilder.Remove(stringBuilder.Length - 3, 3);
            stringBuilder.Append(")");

            this.Command.CommandText = stringBuilder.ToString();
            try
            {
                this.Command.ExecuteNonQuery();
                Console.WriteLine("Created table " + this.TableName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't create table: " + ex.Message);
                Console.WriteLine("CREATE TABLE STATEMENT: " + stringBuilder.ToString());
            }
        }

        // Builds a dictionary mapping field names to Field objects
        private void BuildDictionary()
        {
            foreach (Field field in this.List.Fields)
            {
                if (field.TypeAsString != "Computed")
                    this.FNDictionary.Add(this.GetKeyName(this.GetActualColName(field), 1), field);
            }
        }

        // Recursively ensures unique key names for dictionary entries
        private string GetKeyName(string key, int i = 1)
        {
            string newKey = i == 1 ? key : key + i;
            return this.FNDictionary.ContainsKey(newKey) ? GetKeyName(key, i + 1) : newKey;
        }

        // Resolves the SQL-friendly name for a SharePoint field
        private string GetActualColName(Field pField)
        {
            string name = this.ColNameConvetions(pField);
            int duplicates = 0;

            foreach (Field field in this.List.Fields)
            {
                if (field.TypeAsString != "Computed" && name.Equals(this.ColNameConvetions(field), StringComparison.OrdinalIgnoreCase))
                    duplicates++;
            }

            return duplicates > 1 ? this.ToPascalCase(pField.InternalName, true) : name;
        }

        // Applies SharePoint-to-SQL naming conventions
        private string ColNameConvetions(Field pField)
        {
            StringBuilder sb = new StringBuilder(this.ToPascalCase(pField.Title, false));

            switch (pField.TypeAsString)
            {
                case "Choice":
                    sb.Append("Value");
                    break;
                case "User":
                    sb.Append("Id");
                    break;
                case "Lookup":
                    if (!pField.FromBaseType) sb.Append("Id");
                    break;
            }
            return sb.ToString();
        }

        // Maps SharePoint field types to SQL Server types
        private string SQLFieldType(Field field)
        {
            switch (field.TypeAsString)
            {
                case "Attachments":
                case "Boolean": return "[bit]";
                case "Calculated": return "[sql_variant]";
                case "Choice":
                case "File":
                case "LookupMulti":
                case "Note":
                case "Text":
                case "URL":
                case "UserMulti": return "[nvarchar](MAX)";
                case "ContentTypeId": return "[varbinary](MAX)";
                case "Counter":
                case "Integer":
                case "ModStat":
                case "User": return "[int]";
                case "Currency":
                case "Number": return "[float]";
                case "DateTime": return "[datetime]";
                case "Guid": return "[uniqueidentifier]";
                case "Lookup": return field.FromBaseType ? "[nvarchar](MAX)" : "[int]";
                default:
                    Console.WriteLine($"{field.Title} gave an error:");
                    Console.WriteLine($"FOUND A UNKNOWN FIELD TYPE: {field.TypeAsString}. PLEASE ADD THIS FIELD TO THE CODE");
                    return null;
            }
        }

        // Updates existing table structure to match SharePoint list
        private void UpdateTableDesign()
        {
            foreach (var fn in this.FNDictionary)
            {
                string fieldType = this.SQLFieldType(fn.Value);
                int startIndex = fieldType.IndexOf("[") + 1;
                int endIndex = fieldType.LastIndexOf("]");
                string dataType = fieldType.Substring(startIndex, endIndex - startIndex);

                string key = fn.Key;
                this.Command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{this.TableName}' AND COLUMN_NAME = '{key}'";

                if ((int)this.Command.ExecuteScalar() == 0)
                {
                    this.Command.CommandText = $"ALTER TABLE [{this.TableName}] ADD [{key}] {fieldType}";
                    this.Command.ExecuteNonQuery();
                }
                else
                {
                    this.Command.CommandText = $"SELECT [DATA_TYPE] FROM LAKEDB.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{this.TableName}' AND COLUMN_NAME = '{key}'";
                    if ((string)this.Command.ExecuteScalar() != dataType)
                    {
                        this.Command.CommandText = $"ALTER TABLE [{this.TableName}] ALTER COLUMN [{key}] {fieldType}";
                        this.Command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Generates SQL-safe column list for INSERT statements
        private string GetSQLColNames()
        {
            StringBuilder sb = new StringBuilder("([Snapshot], ");
            foreach (var fn in this.FNDictionary)
                sb.Append($"[{fn.Key}], ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");
            return sb.ToString();
        }

        // Updates Metadata table to store last sync time
        private void UpdateMetadata()
        {
            this.Command.CommandText = $"DELETE FROM Metadata WHERE TableName = '{this.TableName}'";
            this.Command.ExecuteNonQuery();

            this.Command.CommandText = $"INSERT INTO Metadata (TableName, LastRefreshDate) VALUES ('{this.TableName}', '{this.CurrentTime}')";
            this.Command.ExecuteNonQuery();
        }

        // Converts a string to PascalCase while handling special characters
        private string ToPascalCase(string pText, bool internalName)
        {
            if (internalName && pText[0] == '_')
                pText += "IN";

            StringBuilder sb = new StringBuilder();
            foreach (char c in pText)
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sb.ToString())
                .Replace(" ", string.Empty)
                .Replace("X0020", string.Empty)
                .Replace("X003a", string.Empty);
        }
    }
}

