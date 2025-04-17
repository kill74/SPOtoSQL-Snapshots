// Decompiled with JetBrains decompiler
// Type: Bring.Sharepoint.SPOUser
// Assembly: ConsoleApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2529ACA9-9F81-4C49-8E47-E8B02D261367
// Assembly location: C:\Users\KEVIN\Desktop\Visual Studio\SPtoSP\ConsoleApp1.exe

using Microsoft.SharePoint.Client;
using System.Security;

namespace Bring.Sharepoint
{
    // This class represents a SharePoint Online user
    public class SPOUser
    {
        // Public field for storing the user's username (email or login name)
        public string username;

        // Private field to store the password in a secure and encrypted format
        private SecureString securePW;

        // Internal field holding the SharePoint Online credentials object used for authentication
        internal SharePointOnlineCredentials spoCredentials;

        /// <param name="username">The SharePoint Online username (typically an email address).</param>
        /// <param name="password">The plain-text password for the SharePoint Online account.</param>
        public SPOUser(string username, string password)
        {
            // Store the username
            this.username = username;

            // Initialize the SecureString to hold the password securely in memory
            this.securePW = new SecureString();

            // Loop through each character in the plain-text password and append it to the SecureString
            foreach (char c in password.ToCharArray())
                this.securePW.AppendChar(c);

            // Create a SharePointOnlineCredentials object using the username and secure password
            // This object will be used for authenticating against SharePoint Online
            this.spoCredentials = new SharePointOnlineCredentials(username, this.securePW);
        }
    }
}
