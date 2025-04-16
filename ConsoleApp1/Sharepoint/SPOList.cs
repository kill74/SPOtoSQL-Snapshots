// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.SPOList
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;
using System;
using System.Linq.Expressions;

namespace Bring.Sharepoint
{
    /// <summary>
    /// Represents a SharePoint list and provides methods to interact with it using the SharePoint Client Object Model (CSOM).
    /// Inherits from a base 'Context' class, which is assumed to manage the SharePoint client context.
    /// </summary>
    public class SPOList : Context
    {
        private List list;  // Holds the SharePoint list object

        /// <summary>
        /// Gets or sets the title of the SharePoint list.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of items retrieved from the list.
        /// </summary>
        public ListItemCollection ItemCollection { get; set; }

        /// <summary>
        /// Gets or sets the collection of fields (columns) in the list.
        /// </summary>
        public FieldCollection Fields { get; set; }

        /// <summary>
        /// Gets or sets the CAML query used to filter or retrieve list items.
        /// If null, a default query retrieves all items.
        /// </summary>
        public string CAMLQuery { get; set; }

        /// <summary>
        /// Initializes the SharePoint list object by establishing the context, retrieving the list by title,
        /// and loading its items and fields based on the CAML query.
        /// </summary>
        public void Build()
        {
            // Check if the web object is null or the context URL doesn't match the expected SharePoint site
            if (this.web == null || this.Ctx.Site.Context.Url != "https://bringglobal.sharepoint.com/" + this.Site)
                this.BuildContext();  // Rebuild the context if necessary

            CamlQuery camlQuery;
            if (this.CAMLQuery == null)
            {
                // Create a default CAML query to retrieve all items if none is provided
                camlQuery = CamlQuery.CreateAllItemsQuery();
            }
            else
            {
                // Use the user-specified CAML query
                camlQuery = new CamlQuery();
                camlQuery.ViewXml = this.CAMLQuery;
            }
            CamlQuery query = camlQuery;

            // Get the SharePoint list by its title
            this.list = this.web.Lists.GetByTitle(this.Name);

            // Retrieve list items based on the CAML query
            this.ItemCollection = this.list.GetItems(query);

            // Get the fields (columns) of the list
            this.Fields = this.list.Fields;

            // Load the list, items, and fields into the client context for execution
            this.Ctx.Load<List>(this.list, Array.Empty<Expression<Func<List, object>>>());
            this.Ctx.Load<ListItemCollection>(this.ItemCollection, Array.Empty<Expression<Func<ListItemCollection, object>>>());
            this.Ctx.Load<FieldCollection>(this.Fields, Array.Empty<Expression<Func<FieldCollection, object>>>());

            // Execute the query to fetch data from SharePoint
            this.Ctx.ExecuteQuery();
        }

        /// <summary>
        /// Refreshes the list by executing any pending changes and rebuilding the list object.
        /// </summary>
        public void Update()
        {
            this.Ctx.ExecuteQuery();  // Commit any pending changes to SharePoint
            this.Build();  // Rebuild the list to refresh data
        }

        /// <summary>
        /// Prints detailed information about each field in a given list item.
        /// Displays field properties and values, handling various field types like Lookup, User, and multi-value Lookups.
        /// </summary>
        /// <param name="item">The list item to inspect.</param>
        public void PropsToString(ListItem item)
        {
            // Print the header for the output table
            Console.WriteLine("Field|InternalName|Value|CBD|Hidden|FieldType|ReadOnly|FromBaseType|Required|ItemValueType");

            bool flag;  // Temporary variable to store boolean field properties

            // Loop through each field in the list
            foreach (Field field in (ClientObjectCollection<Field>)this.Fields)
            {
                try
                {
                    // Get the value of the field for the specified item
                    object obj = item[field.InternalName];
                    string title = field.Title;
                    flag = obj == null;
                    string str = flag.ToString();
                    Console.WriteLine("We good for: " + title + " .   Is null: " + str);

                    if (obj != null)
                    {
                        // Handle different field value types
                        if (obj.GetType() == typeof(FieldLookupValue))
                        {
                            // Handle Lookup fields by displaying the lookup value
                            object[] objArray = new object[19];
                            objArray[0] = (object)field.Title;
                            objArray[1] = (object)"|";
                            objArray[2] = (object)field.InternalName;
                            objArray[3] = (object)"|";
                            objArray[4] = (object)((FieldLookupValue)obj).LookupValue;
                            objArray[5] = (object)"|";
                            flag = field.CanBeDeleted;
                            objArray[6] = (object)flag.ToString();
                            objArray[7] = (object)"|";
                            flag = field.Hidden;
                            objArray[8] = (object)flag.ToString();
                            objArray[9] = (object)"|";
                            objArray[10] = (object)field.TypeAsString;
                            objArray[11] = (object)"|";
                            flag = field.ReadOnlyField;
                            objArray[12] = (object)flag.ToString();
                            objArray[13] = (object)"|";
                            flag = field.FromBaseType;
                            objArray[14] = (object)flag.ToString();
                            objArray[15] = (object)"|";
                            flag = field.Required;
                            objArray[16] = (object)flag.ToString();
                            objArray[17] = (object)"|";
                            objArray[18] = (object)obj.GetType();
                            Console.WriteLine(string.Concat(objArray));
                        }
                        else if (obj.GetType() == typeof(FieldUserValue))
                        {
                            // Handle User fields by displaying the lookup ID
                            object[] objArray = new object[19];
                            objArray[0] = (object)field.Title;
                            objArray[1] = (object)"|";
                            objArray[2] = (object)field.InternalName;
                            objArray[3] = (object)"|";
                            objArray[4] = (object)((FieldLookupValue)obj).LookupId;  // Note: This may be a bug; should cast to FieldUserValue
                            objArray[5] = (object)"|";
                            flag = field.CanBeDeleted;
                            objArray[6] = (object)flag.ToString();
                            objArray[7] = (object)"|";
                            flag = field.Hidden;
                            objArray[8] = (object)flag.ToString();
                            objArray[9] = (object)"|";
                            objArray[10] = (object)field.TypeAsString;
                            objArray[11] = (object)"|";
                            flag = field.ReadOnlyField;
                            objArray[12] = (object)flag.ToString();
                            objArray[13] = (object)"|";
                            flag = field.FromBaseType;
                            objArray[14] = (object)flag.ToString();
                            objArray[15] = (object)"|";
                            flag = field.Required;
                            objArray[16] = (object)flag.ToString();
                            objArray[17] = (object)"|";
                            objArray[18] = (object)obj.GetType();
                            Console.WriteLine(string.Concat(objArray));
                        }
                        else if (obj.GetType() == typeof(FieldLookupValue[]))
                        {
                            // Handle multi-value Lookup fields by displaying each lookup ID
                            foreach (FieldLookupValue fieldLookupValue in (FieldLookupValue[])obj)
                            {
                                object[] objArray = new object[19];
                                objArray[0] = (object)field.Title;
                                objArray[1] = (object)"|";
                                objArray[2] = (object)field.InternalName;
                                objArray[3] = (object)"|";
                                objArray[4] = (object)fieldLookupValue.LookupId;
                                objArray[5] = (object)"|";
                                flag = field.CanBeDeleted;
                                objArray[6] = (object)flag.ToString();
                                objArray[7] = (object)"|";
                                flag = field.Hidden;
                                objArray[8] = (object)flag.ToString();
                                objArray[9] = (object)"|";
                                objArray[10] = (object)field.TypeAsString;
                                objArray[11] = (object)"|";
                                flag = field.ReadOnlyField;
                                objArray[12] = (object)flag.ToString();
                                objArray[13] = (object)"|";
                                flag = field.FromBaseType;
                                objArray[14] = (object)flag.ToString();
                                objArray[15] = (object)"|";
                                flag = field.Required;
                                objArray[16] = (object)flag.ToString();
                                objArray[17] = (object)"|";
                                objArray[18] = (object)obj.GetType();
                                Console.WriteLine(string.Concat(objArray));
                            }
                        }
                        else
                        {
                            // Handle other field types by displaying the value directly
                            object[] objArray = new object[19];
                            objArray[0] = (object)field.Title;
                            objArray[1] = (object)"|";
                            objArray[2] = (object)field.InternalName;
                            objArray[3] = (object)"|";
                            objArray[4] = obj;
                            objArray[5] = (object)"|";
                            flag = field.CanBeDeleted;
                            objArray[6] = (object)flag.ToString();
                            objArray[7] = (object)"|";
                            flag = field.Hidden;
                            objArray[8] = (object)flag.ToString();
                            objArray[9] = (object)"|";
                            objArray[10] = (object)field.TypeAsString;
                            objArray[11] = (object)"|";
                            flag = field.ReadOnlyField;
                            objArray[12] = (object)flag.ToString();
                            objArray[13] = (object)"|";
                            flag = field.FromBaseType;
                            objArray[14] = (object)flag.ToString();
                            objArray[15] = (object)"|";
                            flag = field.Required;
                            objArray[16] = (object)flag.ToString();
                            objArray[17] = (object)"|";
                            objArray[18] = (object)obj.GetType();
                            Console.WriteLine(string.Concat(objArray));
                        }
                    }
                    else
                    {
                        // Handle null field values
                        object[] objArray = new object[17];
                        objArray[0] = (object)field.Title;
                        objArray[1] = (object)"|";
                        objArray[2] = (object)field.InternalName;
                        objArray[3] = (object)"|NULL|";
                        flag = field.CanBeDeleted;
                        objArray[4] = (object)flag.ToString();
                        objArray[5] = (object)"|";
                        flag = field.Hidden;
                        objArray[6] = (object)flag.ToString();
                        objArray[7] = (object)"|";
                        objArray[8] = (object)field.TypeAsString;
                        objArray[9] = (object)"|";
                        flag = field.ReadOnlyField;
                        objArray[10] = (object)flag.ToString();
                        objArray[11] = (object)"|";
                        flag = field.FromBaseType;
                        objArray[12] = (object)flag.ToString();
                        objArray[13] = (object)"|";
                        flag = field.Required;
                        objArray[14] = (object)flag.ToString();
                        objArray[15] = (object)"|";
                        objArray[16] = (object)obj.GetType();
                        Console.WriteLine(string.Concat(objArray));
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors when accessing field values and print error details
                    string[] strArray = new string[16];
                    strArray[0] = field.Title;
                    strArray[1] = "|";
                    strArray[2] = field.InternalName;
                    strArray[3] = "|ERROR|";
                    strArray[4] = field.CanBeDeleted.ToString();
                    strArray[5] = "|";
                    flag = field.Hidden;
                    strArray[6] = flag.ToString();
                    strArray[7] = "|";
                    strArray[8] = field.TypeAsString;
                    strArray[9] = "|";
                    flag = field.ReadOnlyField;
                    strArray[10] = flag.ToString();
                    strArray[11] = "|";
                    flag = field.FromBaseType;
                    strArray[12] = flag.ToString();
                    strArray[13] = "|";
                    flag = field.Required;
                    strArray[14] = flag.ToString();
                    strArray[15] = "|CANT GET TYPE";
                    Console.WriteLine(string.Concat(strArray));
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds a new item to the SharePoint list and returns it.
        /// </summary>
        /// <returns>The newly created list item.</returns>
        public ListItem AddItem()
        {
            // Create a new list item and update it to save to SharePoint
            ListItem listItem = this.list.AddItem(new ListItemCreationInformation());
            listItem.Update();
            return listItem;
        }
    }
}