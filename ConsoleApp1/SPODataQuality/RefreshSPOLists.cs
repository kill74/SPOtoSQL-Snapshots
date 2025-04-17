// Decompiled with JetBrains decompiler
// Type: Bring.SPODataQuality.RefreshSPOLists
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Bring.Sharepoint;
using Bring.Sqlserver;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bring.SPODataQuality
{
    internal class RefreshSPOLists
    {
        private static void Main(string[] args)
        {
            // Log the current timestamp
            Console.WriteLine("CURRENT TIME: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            // Create a SharePoint Online User for authentication
            SPOUser spoUser = new SPOUser("USERNAME", "PASSWORD"); // FJ -> I HAVE REMOVED THE ACTUAL CREDENTIALS

            // Assign the user to two new SPOList instances
            new SPOList().SPOUser = spoUser;
            new SPOList().SPOUser = spoUser;

            // If arguments are passed when starting the program
            if ((uint)args.Length > 0U)
            {
                string lower = args[0].ToLower();

                // If the argument is "daily" -> perform daily sync
                if (!(lower == "daily"))
                {
                    // If the argument is "monthly" -> perform monthly sync
                    if (lower == "monthly")
                        RefreshSQLLists.SPOtoSQLUpdate(false);
                    else
                        // Invalid argument handling
                        Console.WriteLine("Unrecognized argument, please use daily or monthly as the argument");
                }
                else
                    RefreshSQLLists.SPOtoSQLUpdate(true);
            }

            // Log end of process
            Console.WriteLine("End of requests.");
            Console.WriteLine();
        }

        public static void GetAllLists()
        {
            // Create a SharePoint Online User for authentication
            SPOUser spoUser = new SPOUser("USERNAME", "PASSWORD"); // FJ -> I HAVE REMOVED THE ACTUAL CREDENTIALS

            // Create a context targeting the 'seed' site
            Context context = new Context()
            {
                Site = "seed",
                SPOUser = spoUser
            };

            // Loop through all SharePoint lists from the context
            foreach (List allList in (ClientObjectCollection<List>)context.GetAllLists())
            {
                // Load the 'IsSystemList' property for each list
                context.Ctx.Load<List>(allList, new Expression<Func<List, object>>[1]
                {
                    (Expression<Func<List, object>>) (l => (object) l.IsSystemList)
                });
                context.Ctx.ExecuteQuery();

                // Print out the list name and system list status
                Console.WriteLine("List Name: " + allList.Title + "; is: " + allList.IsSystemList.ToString());
            }
        }

        private static void SPODebug(string listName, string ctxURL, SPOUser user)
        {
            // Initialize a new SPOList object for debugging
            SPOList spoList1 = new SPOList();
            spoList1.Name = listName;
            spoList1.Site = ctxURL;
            spoList1.SPOUser = user;

            // Set a CAML query to limit results to 1 item
            spoList1.CAMLQuery = "<View><RowLimit>1</RowLimit></View>";

            // Build the list (query the server)
            SPOList spoList2 = spoList1;
            spoList2.Build();

            // Output the properties of the first list item
            spoList2.PropsToString(spoList2.ItemCollection[0]);
        }

        private static void RefreshListsSPO(SPOList sourceList, SPOList destList)
        {
            try
            {
                // Fetch data from both source and destination lists
                sourceList.Build();
                destList.Build();

                int num1 = 0;
                int num2 = 0;

                // Create field mappings between the two lists
                string[,] actualFields = RefreshSPOLists.GetActualFields(sourceList, destList);

                // Determine the last ID in both source and destination lists
                if ((uint)sourceList.ItemCollection.Count > 0U)
                    num1 = (int)sourceList.ItemCollection[sourceList.ItemCollection.Count - 1]["ID"];
                if ((uint)destList.ItemCollection.Count > 0U)
                    num2 = (int)destList.ItemCollection[destList.ItemCollection.Count - 1]["ID"];

                // Add missing items to destination list to match source list size
                if (num2 < num1)
                {
                    do
                    {
                        destList.AddItem();
                        ++num2;
                    }
                    while (num2 < num1);
                    Console.WriteLine("Adding new items...");
                    destList.Update();
                    Console.WriteLine("Done adding items.");
                }

                // Copy field values from source to destination for each item
                for (int index1 = 0; index1 < sourceList.ItemCollection.Count; ++index1)
                {
                    int id = (int)sourceList.ItemCollection[index1]["ID"];
                    for (int index2 = 0; index2 < actualFields.Length / 2; ++index2)
                        destList.ItemCollection.GetById(id)[actualFields[index2, 0]] = sourceList.ItemCollection[index1][actualFields[index2, 1]];
                    destList.ItemCollection.GetById(id).Update();
                }

                // Submit all changes to SharePoint
                destList.Ctx.ExecuteQuery();

                // Log success message
                Console.WriteLine(sourceList.Site + " " + sourceList.Name + " -> " + destList.Site + " " + destList.Name + ": Done!");
            }
            catch (Exception ex)
            {
                // Log error message
                Console.WriteLine(ex.Message);
            }
        }

        private static string[,] GetActualFields(SPOList listone, SPOList listtwo)
        {
            // Retrieve user-defined fields from both lists
            List<Field> fields1 = RefreshSPOLists.GetFields(listone);
            List<Field> fields2 = RefreshSPOLists.GetFields(listtwo);

            // Initialize a 2D array for matching field pairs
            string[,] strArray = new string[fields1.Count, 2];

            int index1 = 0;
            int index2 = 0;

            // Match fields by their display name
            foreach (Field field1 in fields1)
            {
                Field field2;
                do
                {
                    field2 = fields2[index2];
                    if (field1.Title == field2.Title)
                    {
                        strArray[index1, 0] = field2.InternalName; // Destination internal name
                        strArray[index1, 1] = field1.InternalName; // Source internal name
                    }
                    ++index2;
                }
                while (field1.Title != field2.Title && index2 < fields2.Count);
                ++index1;
                index2 = 0;
            }
            return strArray;
        }

        private static List<Field> GetFields(SPOList list)
        {
            // Return a list of custom fields (excluding system fields)
            List<Field> fieldList = new List<Field>();
            foreach (Field field in (ClientObjectCollection<Field>)list.Fields)
            {
                if (!field.FromBaseType || field.InternalName == "Title")
                    fieldList.Add(field);
            }
            return fieldList;
        }
    }
}
