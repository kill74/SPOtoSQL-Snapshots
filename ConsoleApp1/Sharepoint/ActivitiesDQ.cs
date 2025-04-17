// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.ActivitiesDQ
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;

namespace Bring.Sharepoint
{
    // Defines a class that handles operations on SharePoint "activities" list
    internal class ActivitiesDQ
    {
        // Property representing the current SharePoint user context
        public SPOUser Me { get; set; }
        public bool UpdateIDs()
        {
            // CAML query string to select all list items where the '_OpportunityID' field is null
            string str = "<View><Query><Where><IsNull><FieldRef Name ='_OpportunityID' /></IsNull></Where></Query></View>";

            // Create a new SPOList object to interact with a SharePoint list
            SPOList spoList1 = new SPOList();
            
            // Assign the list name to "activities"
            spoList1.Name = "activities";

            // Assign the SharePoint site identifier (probably a site collection or subsite) to "wolf"
            spoList1.Site = "wolf";

            // Set the SharePoint user context for this list interaction
            spoList1.SPOUser = this.Me;

            // Apply the CAML query to filter the list items
            spoList1.CAMLQuery = str;

            // Assign the configured object to a new variable for clarity
            SPOList spoList2 = spoList1;

            // Initialize or execute the query to fetch the filtered list items from SharePoint
            spoList2.Build();

            // Loop through all items returned by the CAML query
            foreach (ListItem listItem in (ClientObjectCollection<ListItem>) spoList2.ItemCollection)
            {
                // Assign the value of 'OpportunityID' to '_OpportunityID' for each list item
                listItem["_OpportunityID"] = listItem["OpportunityID"];

                // Mark the list item for update in the client context
                listItem.Update();
            }

            // Commit all pending changes to the SharePoint server
            spoList2.Ctx.ExecuteQuery();

            // Return true to indicate that the operation was successful
            return true;
        }
    }
}

