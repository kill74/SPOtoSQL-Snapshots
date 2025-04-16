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
      Console.WriteLine("CURRENT TIME: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
      SPOUser spoUser = new SPOUser("USERNAME", "PASSWORD"); // FJ -> APAGUEI AS CREDENCIAIS QUE ESTÂO EM USO
      new SPOList().SPOUser = spoUser;
      new SPOList().SPOUser = spoUser;
      if ((uint) args.Length > 0U)
      {
        string lower = args[0].ToLower();
        if (!(lower == "daily"))
        {
          if (lower == "monthly")
            RefreshSQLLists.SPOtoSQLUpdate(false);
          else
            Console.WriteLine("Unrecognized argument, please use daily or monthly as the argument");
        }
        else
          RefreshSQLLists.SPOtoSQLUpdate(true);
      }
      Console.WriteLine("End of requests.");
      Console.WriteLine();
    }

    public static void GetAllLists()
    {
      SPOUser spoUser = new SPOUser("USERNAME", "PASSWORD"); // FJ -> APAGUEI AS CREDENCIAIS QUE ESTÂO EM USO
      Context context = new Context()
      {
        Site = "seed",
        SPOUser = spoUser
      };
      foreach (List allList in (ClientObjectCollection<List>) context.GetAllLists())
      {
        context.Ctx.Load<List>(allList, new Expression<Func<List, object>>[1]
        {
          (Expression<Func<List, object>>) (l => (object) l.IsSystemList)
        });
        context.Ctx.ExecuteQuery();
        Console.WriteLine("List Name: " + allList.Title + "; is: " + allList.IsSystemList.ToString());
      }
    }

    private static void SPODebug(string listName, string ctxURL, SPOUser user)
    {
      SPOList spoList1 = new SPOList();
      spoList1.Name = listName;
      spoList1.Site = ctxURL;
      spoList1.SPOUser = user;
      spoList1.CAMLQuery = "<View><RowLimit>1</RowLimit></View>";
      SPOList spoList2 = spoList1;
      spoList2.Build();
      spoList2.PropsToString(spoList2.ItemCollection[0]);
    }

    private static void RefreshListsSPO(SPOList sourceList, SPOList destList)
    {
      try
      {
        sourceList.Build();
        destList.Build();
        int num1 = 0;
        int num2 = 0;
        string[,] actualFields = RefreshSPOLists.GetActualFields(sourceList, destList);
        if ((uint) sourceList.ItemCollection.Count > 0U)
          num1 = (int) sourceList.ItemCollection[sourceList.ItemCollection.Count - 1]["ID"];
        if ((uint) destList.ItemCollection.Count > 0U)
          num2 = (int) destList.ItemCollection[destList.ItemCollection.Count - 1]["ID"];
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
        for (int index1 = 0; index1 < sourceList.ItemCollection.Count; ++index1)
        {
          int id = (int) sourceList.ItemCollection[index1]["ID"];
          for (int index2 = 0; index2 < actualFields.Length / 2; ++index2)
            destList.ItemCollection.GetById(id)[actualFields[index2, 0]] = sourceList.ItemCollection[index1][actualFields[index2, 1]];
          destList.ItemCollection.GetById(id).Update();
        }
        destList.Ctx.ExecuteQuery();
        Console.WriteLine(sourceList.Site + " " + sourceList.Name + " -> " + destList.Site + " " + destList.Name + ": Done!");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    private static string[,] GetActualFields(SPOList listone, SPOList listtwo)
    {
      List<Field> fields1 = RefreshSPOLists.GetFields(listone);
      List<Field> fields2 = RefreshSPOLists.GetFields(listtwo);
      string[,] strArray = new string[fields1.Count, 2];
      int index1 = 0;
      int index2 = 0;
      foreach (Field field1 in fields1)
      {
        Field field2;
        do
        {
          field2 = fields2[index2];
          if (field1.Title == field2.Title)
          {
            strArray[index1, 0] = field2.InternalName;
            strArray[index1, 1] = field1.InternalName;
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
      List<Field> fieldList = new List<Field>();
      foreach (Field field in (ClientObjectCollection<Field>) list.Fields)
      {
        if (!field.FromBaseType || field.InternalName == "Title")
          fieldList.Add(field);
      }
      return fieldList;
    }
  }
}
