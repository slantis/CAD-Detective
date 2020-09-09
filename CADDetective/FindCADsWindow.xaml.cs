using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace CADDetective
{
    public class CADItem
    {
        public ImportInstance CADImportInstance { get; set; }
        public View OwnerView { get; set; }
        public string IsHide { get; set; }
        public string IsLink { get; set; }
        public string ViewName { get; set; }

    }

    public partial class FindCADsWindow : Window
    {
        readonly UIDocument uidoc;
        readonly Document doc;
        private readonly List<CADItem> CADList = new List<CADItem>();

        public FindCADsWindow(UIDocument uid)
        {
            InitializeComponent();
            CADListView.ItemsSource = CADList;
            uidoc = uid;
            doc = uidoc.Document;

            // Get all imports in the model
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ImportInstance))
                     .WhereElementIsNotElementType()
                     .Cast<ImportInstance>();

            foreach (ImportInstance IInstance in collector)
            {
                // This section gets view specific imports
                if (IInstance.OwnerViewId != null && IInstance.OwnerViewId.IntegerValue != -1)
                {
                    View ownerView = doc.GetElement(IInstance.OwnerViewId) as View;

                    ViewFamilyType ViewTy = doc.GetElement(ownerView.GetTypeId()) as ViewFamilyType;

                    string ViewN = ViewTy.Name + " - " + ownerView.Name;

                    CADItem addition = new CADItem
                    {
                        ViewName = ViewN,
                        CADImportInstance = IInstance,
                        OwnerView = ownerView,
                        IsHide = IInstance.IsHidden(ownerView).ToString()
                    };
                    CADList.Add(addition);
                }

                // This section gets model-wide imports. These CADs can be viewed in every view
                else
                {
                    CADItem addition = new CADItem
                    {
                        ViewName = null,
                        CADImportInstance = IInstance,
                        OwnerView = null,
                        IsHide = null
                    };
                    CADList.Add(addition);
                }
            }
        }

        // Zoom to the CAD
        private void GoCAD_Click(object sender, RoutedEventArgs e)
        {
            CADItem SelectedCad = CADListView.SelectedItem as CADItem;
            uidoc.ActiveView = SelectedCad.OwnerView;
            uidoc.ShowElements(SelectedCad.CADImportInstance.Id);
        }

        // Select the CAD
        private void SelectCAD_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.IList SelectedCadsIL = CADListView.SelectedItems;
            List<CADItem> SelectedCads = new List<CADItem>();

            foreach (CADItem item in SelectedCadsIL) // there should be a better way
            {
                SelectedCads.Add(item);
            }

            List<ElementId> Ids = new List<ElementId>();

            foreach (CADItem SelectedCad in SelectedCads) // add selected cads to selection
            {
                Ids.Add(SelectedCad.CADImportInstance.Id);
            }
            uidoc.Selection.SetElementIds(Ids);
            Close();
        }

        // Open the view
        private void GoView_Click(object sender, RoutedEventArgs e)
        {
            CADItem SelectedCad = CADListView.SelectedItem as CADItem;

            View selectedCadView = SelectedCad.OwnerView;
            uidoc.ActiveView = selectedCadView;
        }

        private void onSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            System.Collections.IList SelectedCadsIL = CADListView.SelectedItems;
            List<CADItem> SelectedCads = new List<CADItem>();

            foreach (CADItem item in SelectedCadsIL) // there should be a better way
            {
                SelectedCads.Add(item);
            }

            if (SelectedCads.Count == 1 && SelectedCads.First<CADItem>().OwnerView != null) // If only one cad selected and is view-specific
            {
                GoCAD.IsEnabled = true;
                GoView.IsEnabled = true;
                SelCAD.IsEnabled = true;
            }
            else // Else allow only to select the cads
            {
                GoCAD.IsEnabled = false;
                GoView.IsEnabled = false;
                SelCAD.IsEnabled = true;
            }
        }
    }
}
