using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Reflection;
using System.IO;

namespace CADDetective
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.DB.Macros.AddInId("5A47C252-6B19-4AC8-A4BE-395C48FCD6F1")]
    public class ThisApplication : IExternalApplication
    {
        RibbonPanel DefaultPanel;

        public Result OnShutdown(UIControlledApplication uiApp)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication uiApp)
        {
            try
            {
                string ThisDllPath = Assembly.GetExecutingAssembly().Location;

                String exeConfigPath = Path.GetDirectoryName(ThisDllPath) + "\\CADDetective.dll";

                string RibbonTabName = "ASG";
                string PanelName = "CAD Detective";

                // Create ribbon and the panel
                try
                {
                    uiApp.CreateRibbonTab(RibbonTabName);
                }

                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    // Do nothing, tab already exists
                }

                try 
                {
                    DefaultPanel = uiApp.CreateRibbonPanel(RibbonTabName, PanelName);
                }

                // Panel already exists
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    List<RibbonPanel> Panels = uiApp.GetRibbonPanels(RibbonTabName);
                     DefaultPanel = Panels.Find(p => p.Name.Equals(PanelName));
                }

                // Button configuration
                string CadDetectiveName = "CAD Detective";
                PushButtonData CadDetectiveData = new PushButtonData(CadDetectiveName, CadDetectiveName, exeConfigPath, "CADDetective.ThisCommand");
                CadDetectiveData.LargeImage = Utils.RetriveImage("CADDetective.Resources.lupa32x32.ico"); // Pushbutton image
                CadDetectiveData.Image = Utils.RetriveImage("CADDetective.Resources.lupa16x16.ico");
                CadDetectiveData.ToolTip = "Find linked and imported CADs through the model";
                RibbonItem CadDetectiveButton = DefaultPanel.AddItem(CadDetectiveData); // Add pushbutton

                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                Utils.CatchDialog(ex);
                return Result.Failed;
            }
        }
    }
}