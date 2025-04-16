// Decompiled with JetBrains decompiler
// Type: Bring.Sqlserver.RefreshSQLLists
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Bring.Sharepoint;
using System;
using System.Configuration;

namespace Bring.Sqlserver
{
    /// <summary>
    /// Provides functionality to update SQL lists based on data extracted from SharePoint Online.
    /// </summary>
    internal class RefreshSQLLists
    {
        /// <summary>
        /// Initiates the update process for all configured SharePoint lists.
        /// </summary>
        /// <param name="daily">
        /// Determines the type of update:
        /// True for a daily update, False for a current-time update.
        /// </param>
        public static void SPOtoSQLUpdate(bool daily)
        {
            // Create an authenticated user instance to interact with SharePoint Online.
            // Note: The actual credentials have been removed for security purposes.
            SPOUser user = new SPOUser("USERNAME", "PASSWORD"); // Credentials removed

            // Iterate through all keys specified in the application's configuration settings.
            // Each key represents a list name, and its corresponding value is the SharePoint site URL.
            foreach (string allKey in ConfigurationManager.AppSettings.AllKeys)
            {
                // Update the SQL list for each configuration entry.
                // Parameters include the list name, the SharePoint context URL, the authenticated user, and the update type.
                RefreshSQLLists.RefreshListsSQL(allKey, ConfigurationManager.AppSettings[allKey], user, daily);
            }
        }

        /// <summary>
        /// Refreshes an individual SharePoint list by building SQL commands and updating the database.
        /// </summary>
        /// <param name="listName">The name of the SharePoint list as defined in configuration.</param>
        /// <param name="ctxURL">The URL of the SharePoint site context containing the list.</param>
        /// <param name="user">An instance of <see cref="SPOUser"/> containing credentials for SharePoint access.</param>
        /// <param name="daily">
        /// A flag indicating which update method to use:
        /// True for a daily update, False for a current-time update.
        /// </param>
        public static void RefreshListsSQL(string listName, string ctxURL, SPOUser user, bool daily)
        {
            try
            {
                // Instantiate the SPOList object to encapsulate the details necessary to access the list.
                SPOList spoList = new SPOList
                {
                    Site = ctxURL,      // Set the SharePoint site URL.
                    SPOUser = user,     // Attach the authenticated user.
                    Name = listName     // Set the target list name.
                };

                // Instantiate SQLInteraction and assign the configured SPOList object.
                // This class is responsible for building the SQL commands and managing database updates.
                SQLInteraction sqlInteraction = new SQLInteraction
                {
                    List = spoList
                };

                // Prepare SQL commands or perform necessary setup procedures.
                sqlInteraction.Build();

                // Execute the update procedure based on the update frequency specified.
                if (daily)
                {
                    sqlInteraction.DailyUpdate();
                }
                else
                {
                    sqlInteraction.CurrentTimeUpdate();
                }
            }
            catch (Exception ex)
            {
                // Log the exception message to the console.
                // In a production system, consider using a robust logging framework for better error tracking.
                Console.WriteLine(ex.Message);
            }
        }
    }
}
