#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace NewFamilySymbolProfile
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    const string _filepath = "C:/Users/All Users/Autodesk"
      + "/RVT 2018/Libraries/UK/Profiles/Framing/Steel"
      + "/Profiles_L-Angles.rfa";

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      Document profile_doc = app.OpenDocumentFile( 
        _filepath );

      // The profile document contains no family sybols:

      FilteredElementCollector symbols
        = new FilteredElementCollector( profile_doc )
          .OfClass( typeof( FamilySymbol ) );

      Debug.Assert( 0 == symbols.GetElementCount(), 
        "expected no family symbol" );

      // Load the profile family to generate them:

      FamilySymbol profile_symbol = null;

      using( Transaction tx = new Transaction( doc ) )
      {
        tx.Start( "Load Profile Family" );

        Family family;

        doc.LoadFamily( _filepath, out family );

        tx.Commit();

        ISet<ElementId> ids = family.GetFamilySymbolIds();

        foreach( ElementId id in ids )
        {
          profile_symbol = doc.GetElement( id ) 
            as FamilySymbol;

          Debug.Print( profile_symbol.Name );
        }
      }

      // Generate the family symbol profile:

      FamilySymbolProfile fsp = null;

      if( null != profile_symbol )
      {
        using( Transaction tx = new Transaction( doc ) )
        {
          tx.Start( "Create FamilySymbolProfile" );
          fsp = app.Create.NewFamilySymbolProfile( 
            profile_symbol );
          tx.Commit();
        }
      }

      return Result.Succeeded;
    }
  }
}
