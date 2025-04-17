// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.TimesheetDQ
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;
using System;

namespace Bring.Sharepoint
{
    // This class is responsible for updating the approvers in the "Timesheet" SharePoint list
    // It checks for timesheet items that are missing a "Main Approver" and attempts to fill in that data
    // by referencing the appropriate Unit or HR Database list
    internal class TimesheetDQ
    {
        // The SharePoint Online user context used for authentication
        public SPOUser Me { get; set; }
        /// <returns>Returns true if the update process runs without crashing.</returns>
        public bool UpdateApprovers()
        {
            // CAML Query to find items in "Timesheet" list where Main Approver is NULL
            string str = "<View><Query><Where><IsNull><FieldRef Name ='Main_x0020_approver' /></IsNull></Where></Query></View>";

            // Initialize a SPOList object for the "Timesheet" list
            SPOList spoList1 = new SPOList();
            spoList1.Name = "Timesheet";  // Target list: Timesheet entries
            spoList1.Site = "selfservice/timesheet";  // SharePoint subsite location
            spoList1.SPOUser = this.Me;   // Assign user credentials
            spoList1.CAMLQuery = str;     // Apply the CAML filter

            // Alias for easier reference
            SPOList spoList2 = spoList1;

            // Fetch the list items based on the query
            spoList2.Build();

            // Loop through each retrieved ListItem
            foreach (ListItem listItem in (ClientObjectCollection<ListItem>) spoList2.ItemCollection)
            {
                try
                {
                    // Attempt to fetch a matching Unit item for this timesheet entry
                    ListItem unitItem = this.GetUnitItem(listItem);

                    // If a matching unit is found and it has a Main Approver defined:
                    if (unitItem != null && unitItem["Main_x0020_approver"] != null)
                    {
                        // Assign Main Approver from Unit item
                        listItem["Main_x0020_approver"] = unitItem["Main_x0020_approver"];
                        // Assign Optional Approver from Unit item
                        listItem["Optional_x0020_approver"] = unitItem["Optional_x0020_approver"];
                    }
                    else
                    {
                        // If no Unit match or approver is missing, fetch a fallback approver from HR Database
                        listItem["Main_x0020_approver"] = (object) this.GetStructureApprover(listItem);
                    }

                    // Update the current list item in SharePoint's memory
                    listItem.Update();
                }
                catch (Exception ex)
                {
                    // Log errors for easier debugging
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Item ID: " + listItem.Id);
                }
            }

            // Commit all the batched updates to SharePoint in one request
            spoList2.Ctx.ExecuteQuery();

            // Return true to indicate success
            return true;
        }
        /// <param name="item">The timesheet list item being processed.</param>
        /// <returns>The matched Unit list item or null if none is found.</returns>
        private ListItem GetUnitItem(ListItem item)
        {
            // Prepare to query the "Unit" SharePoint list
            SPOList spoList1 = new SPOList();
            spoList1.Name = "Unit";
            spoList1.Site = "seed";
            spoList1.SPOUser = this.Me;

            // Build a CAML query to match the Project ID and check if the Unit is active
            string str = "<View><Query><Where><And><Eq><FieldRef Name ='Project_x0020_ID' /><Value Type='Text'>" 
                       + ((FieldLookupValue) item["Unit_x003a_Project_x0020_ID"]).LookupValue 
                       + "</Value></Eq><Eq><FieldRef Name ='Active' /><Value Type='Text'>Yes</Value></Eq></And></Where></Query></View>";

            // Apply the query
            spoList1.CAMLQuery = str;
            spoList1.Build();

            // Return the first matching unit item, or null if none are found
            return spoList1.ItemCollection.Count != 0 ? spoList1.ItemCollection[0] : null;
        }

        /// <param name="item">The timesheet list item being processed.</param>
        /// <returns>The fallback approver or null if none are found.</returns>
        private FieldUserValue GetStructureApprover(ListItem item)
        {
            // Prepare to query the "HR Database" SharePoint list
            SPOList spoList1 = new SPOList();
            spoList1.Name = "HR Database";
            spoList1.Site = "people";
            spoList1.SPOUser = this.Me;

            // Build CAML query to match the employee's lookup ID
            string str = "<View><Query><Where><Eq><FieldRef Name ='Display_x0020_Name' LookupId='TRUE'/>"
                       + "<Value Type='Integer'>" + ((FieldLookupValue) item["Resource"]).LookupId + "</Value></Eq></Where></Query></View>";

            // Apply the query
            spoList1.CAMLQuery = str;
            spoList1.Build();

            // Return the Approver1 field from the first matching HR item or null if none are found
            return spoList1.ItemCollection.Count != 0 
                   ? (FieldUserValue) spoList1.ItemCollection[0]["Approver1"] 
                   : null;
        }
    }
}
