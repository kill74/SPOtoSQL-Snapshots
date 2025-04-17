// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.InvoiceRequestDQ
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;

namespace Bring.Sharepoint
{
    // Internal class responsible for updating approver fields in SharePoint "invoice request" list items
    internal class InvoiceRequestDQ
    {
        // Property that holds the SharePoint user session/context
        public SPOUser Me { get; set; }
        public bool UpdateApprovers()
        {
            // Calculate the date 3 days ago from today
            DateTime dateTime = DateTime.Today.AddDays(-3.0);

            // Build a CAML query to select 'Unit' list items modified in the last 3 days
            string str = "<View><Query><Where><Geq><FieldRef Name='Modified' /><Value Type='DateTime'>" 
                         + (dateTime.Year.ToString() + "-" + this.PadStr(dateTime.Month) + "-" + this.PadStr(dateTime.Day) + "T00:00:00Z") 
                         + "</Value></Geq></Where></Query></View>";

            // Initialize the SPOList for the 'Unit' list
            SPOList spoList1 = new SPOList();
            spoList1.SPOUser = this.Me;
            spoList1.Name = "Unit";
            spoList1.Site = "seed";
            spoList1.CAMLQuery = str;
            SPOList spoList2 = spoList1;

            // Dictionary to store Unit list items by their 'Project ID' field
            Dictionary<string, ListItem> unitDictionary = new Dictionary<string, ListItem>();

            // Execute the CAML query and fetch the items
            spoList2.Build();

            // Check if any items were found.
            if ((uint)spoList2.ItemCollection.Count > 0U)
            {
                // Loop through each item and map it into the dictionary using 'Project ID' as the key
                for (int index = 0; index < spoList2.ItemCollection.Count; ++index)
                    unitDictionary.Add(
                        (string)spoList2.ItemCollection[index]["Project_x0020_ID"], 
                        spoList2.ItemCollection[index]
                    );

                // Initialize another SPOList for the 'invoice request' list
                SPOList spoList3 = new SPOList();
                spoList3.SPOUser = this.Me;
                spoList3.Name = "invoice request";
                spoList3.Site = "selfservice/invoicerequest";

                // Build a CAML query dynamically to match items linked to any Project IDs from the Unit list
                spoList3.CAMLQuery = this.QueryBuilder(unitDictionary);

                SPOList spoList4 = spoList3;
                int num = 0;

                // Execute the CAML query to fetch matching invoice requests
                spoList4.Build();

                // Loop through each invoice request list item
                foreach (ListItem listItem1 in (ClientObjectCollection<ListItem>)spoList4.ItemCollection)
                {
                    // Look up the matching Unit item based on the lookup field 'Unit_x002f_Project_x003a_Project0'
                    ListItem listItem2 = unitDictionary[
                        ((FieldLookupValue)listItem1["Unit_x002f_Project_x003a_Project0"]).LookupValue
                    ];

                    // Copy approver values from the Unit item to the Invoice Request item
                    listItem1["Main_x0020_approver"] = listItem2["Main_x0020_approver"];
                    listItem1["Optional_x0020_approver"] = listItem2["Optional_x0020_approver"];
                    listItem1["Financial_x0020_approver"] = listItem2["Financial_x0020_approver"];

                    // Mark the item for update
                    listItem1.Update();
                    ++num;

                    // Execute the batch update every 80 items to avoid large requests
                    if (num % 80 == 0)
                        spoList4.Ctx.ExecuteQuery();
                }

                // Log and execute any remaining updates that didn’t reach the batch size
                Console.WriteLine("Executing last query");
                spoList4.Ctx.ExecuteQuery();
                Console.WriteLine("Done Executing last query");
            }

            // Return true to confirm the process has finished
            return true;
        }

        private string PadStr(int i)
        {
            return i < 10 ? "0" + i : i.ToString();
        }

        private string QueryBuilder(Dictionary<string, ListItem> unitDictionary)
        {
            bool flag = true;
            // Start the CAML query with the appropriate number of <Or> wrappers
            string str = this.OrAppend("<View><Query><Where>", unitDictionary.Keys.Count);

            // Loop through each Project ID key
            foreach (string key in unitDictionary.Keys)
            {
                str += "<Eq><FieldRef Name='Unit_x002f_Project_x003a_Project0' /><Value Type='Text'>" + key + "</Value></Eq>";
                if (!flag)
                    str += "</Or>";
                flag = false;
            }

            // Close the CAML query syntax
            return str + "</Where></Query></View>";
        }

        private string OrAppend(string str, int size)
        {
            string result;
            if (size <= 1)
                result = str;
            else
                str = result = this.OrAppend(str + "<Or>", size - 1);
            return result;
        }
    }
}

