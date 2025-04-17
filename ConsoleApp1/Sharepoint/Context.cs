// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.Context
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;
using System;
using System.Linq.Expressions;
using System.Net;

namespace Bring.Sharepoint
{
    // Defines a class responsible for handling the connection context to a SharePoint Online site
    public class Context
    {
        // Internal field representing the SharePoint Web object, which refers to a specific SharePoint site
        internal Web web;

        // Public property for holding the subsite or site identifier within the SharePoint domain
        public string Site { get; set; }

        // Public property holding the SharePoint user credentials, encapsulated in a custom SPOUser class
        public SPOUser SPOUser { get; set; }

        // Public property for the SharePoint ClientContext, which manages connections and operations
        public ClientContext Ctx { get; set; }

        public void BuildContext()
        {
            // Create a new ClientContext targeting the specified SharePoint Online site
            ClientContext clientContext = new ClientContext("https://bringglobal.sharepoint.com/" + this.Site);

            // Assign the credentials from the SPOUser object to authenticate against SharePoint
            clientContext.Credentials = (ICredentials)this.SPOUser.spoCredentials;

            // Store the client context in the class property for later reuse
            this.Ctx = clientContext;

            // Store the Web object, which represents the site, for further list and item operations
            this.web = this.Ctx.Web;

            // Load the Web object into the context without requesting any additional properties
            this.Ctx.Load<Web>(this.web, Array.Empty<Expression<Func<Web, object>>>());
        }
        public ListCollection GetAllLists()
        {
            // Check whether the context is not yet built, or the URL is incorrect
            if (this.web == null || this.Ctx.Site.Context.Url != "https://bringglobal.sharepoint.com/" + this.Site)
                this.BuildContext();  // Rebuilds the context to ensure valid connection

            // Access the Lists collection, representing all lists in the site
            ListCollection lists = this.web.Lists;

            // Queue the list collection to be loaded from SharePoint, without extra properties
            this.Ctx.Load<ListCollection>(lists, Array.Empty<Expression<Func<ListCollection, object>>>());

            // Execute the queued query, sending the request to SharePoint and loading the data
            this.Ctx.ExecuteQuery();

            // Return the retrieved list collection to the caller
            return lists;
        }
    }
}
