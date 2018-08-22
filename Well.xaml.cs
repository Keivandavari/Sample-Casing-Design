using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CasingDesign.Common;
using GalaSoft.MvvmLight.Messaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CasingDesign.Plots_And_Graphs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Well : Page
    {

        public Well()
        {
            this.InitializeComponent();
            this.NavigationCacheMode =
        Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            this.DataContext = new ViewModel.WellViewModel();
            //Messenger.Default.Send(new NotificationMessageAction<Windows.Foundation.Size>(
            //    null,
            //    obj =>
            //    {
            //        CreateSizes(obj.Width, obj.Height);
            //    }));
            if (Model.SizeModel.appSize.Width >= 1366)
            {
                CreateSizes(Model.SizeModel.appSize.Width, Model.SizeModel.appSize.Height);

            }
            else
            {
                
                CreateSizes(1366, Model.SizeModel.appSize.Height);
            }
            Messenger.Default.Register<Windows.Foundation.Size>(this, size =>
            {
                if (size.Width >= 1366)
                {
                    CreateSizes(size.Width, size.Height);

                }
                else
                {
                    CreateSizes(1366, size.Height);
                }
            });

        }

        private void CreateSizes(double width,double height)
        {
            this.Width = width;
            this.Height = height;
            RootBorder.Margin = new Thickness(.035 * width, .1 * height, .035 * width, .085 * height);
            CasingBorder.Margin = new Thickness(0, 0, .0025 * width, 0);
            HoleBorder.Margin = new Thickness(.0025 * width, 0, .0025 * width, 0);
            ConfigBorder.Margin = new Thickness(.0025 * width, 0, 0, 0);
            double availableSize = 
                (width -
                (RootBorder.Margin.Left +
                RootBorder.Margin.Right +
                CasingBorder.Margin.Right +
                HoleBorder.Margin.Left +
                HoleBorder.Margin.Right +ConfigBorder.Margin.Left)) / 3;
            int i = 1;
            for (i = 1; i <= 10; i++)
            {
                if (availableSize >= 35 + i * 84 && availableSize <= 35 + (i + 1) * 84)
                {
                    CasingBorder.Width = 35 + (i) * 84;
                    HoleBorder.Width = 35 + (i) * 84;
                    break;
                }
            }
            ConfigGrid.Width = width - 7 - CasingBorder.Width - HoleBorder.Width - (RootBorder.Margin.Left + RootBorder.Margin.Right + CasingBorder.Margin.Right + HoleBorder.Margin.Left + HoleBorder.Margin.Right + ConfigBorder.Margin.Left);
            //ErrorTextBlock.MaxWidth = ConfigOptionStackPanel.ActualWidth;
            //NoConfigCautionTextBlock.MaxWidth = ConfigOptionStackPanel.ActualWidth;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CasingGrid.Children.Remove(CasingListListview);
            CasingGrid.Children.Remove(CasingSourceListview);
            CasingGrid.Children.Remove(CasingChoiceListview);

            CasingListAndReturnStackPanel.Items.Remove(CasingRemove);
            CasingListAndReturnStackPanel.Items.Remove(CasingSelectAll);
            CasingListAndReturnStackPanel.Items.Remove(CasingClearAll);

            HoleListAndReturnStackPanel.Items.Remove(HoleRemove);
            HoleListAndReturnStackPanel.Items.Remove(HoleSelectAll);
            HoleListAndReturnStackPanel.Items.Remove(HoleClearAll);

            HoleGrid.Children.Remove(HoleSourceListview);
            HoleGrid.Children.Remove(HoleListListview);
            HoleGrid.Children.Remove(HoleChoiceListview);
            HoleGrid.Children.Remove(ParameterGrid);
        }


        private void Casing_Inventory_Click(object sender, RoutedEventArgs e)
        {
            Casing_To_Inventory();
            if (HoleGrid.Children.Contains(HoleChoiceListview) || HoleGrid.Children.Contains(HoleSourceListview))
                Hole_To_List();
        }
        private void Hole_Inventory_Click(object sender, RoutedEventArgs e)
        {
            Hole_To_Inventory();
            if (CasingGrid.Children.Contains(CasingChoiceListview) || CasingGrid.Children.Contains(CasingSourceListview))
                Casing_To_List();
        }


        private void Casing_To_Selected_List(object sender, RoutedEventArgs e)
        {
            Casing_To_List();
            if (HoleGrid.Children.Contains(HoleChoiceListview) || HoleGrid.Children.Contains(HoleSourceListview))
                Hole_To_List();
        }


        private void Hole_To_Selected_List(object sender, RoutedEventArgs e)
        {    
            Hole_To_List();
            if (CasingGrid.Children.Contains(CasingChoiceListview) || CasingGrid.Children.Contains(CasingSourceListview))
                Casing_To_List();
        }


        private void CasingAdd_Click(object sender, RoutedEventArgs e)
        {
            Casing_To_List();
        }

        private void HoleAdd_Click(object sender, RoutedEventArgs e)
        {
            Hole_To_List();
        }


        private void HoleAllowable_ForCasing(object sender, RoutedEventArgs e)
        {
            Casing_To_Source();
            Hole_To_Choice();
        }
        private void CasingAllowable_ForHole(object sender, RoutedEventArgs e)
        {
            Hole_To_Source();
            Casing_To_Choice();
            
        }
        private void Hole_Parameters(object sender, RoutedEventArgs e)
        {
            Hole_To_Parameter();
            if (CasingGrid.Children.Contains(CasingChoiceListview) || CasingGrid.Children.Contains(CasingSourceListview))
                Casing_To_List();
        }
        private void Hole_Popup_Click(object sender, RoutedEventArgs e)
        {
            HolePopup.IsOpen = HolePopup.IsOpen ? false : true;      
        }

        private void Casing_Popup_Click(object sender, RoutedEventArgs e)
        {
            CasingPopup.IsOpen = CasingPopup.IsOpen ? false : true;
        }



        private void Remove_Casing_Children()
        {
            if (CasingGrid.Children.Contains(CasingListListview))
                CasingGrid.Children.Remove(CasingListListview);
            if (CasingGrid.Children.Contains(CasingRefListListview))
                CasingGrid.Children.Remove(CasingRefListListview);
            if (CasingGrid.Children.Contains(CasingSourceListview))
                CasingGrid.Children.Remove(CasingSourceListview);
            if (CasingGrid.Children.Contains(CasingChoiceListview))
                CasingGrid.Children.Remove(CasingChoiceListview);
            if (CasingListAndReturnStackPanel.Items.Contains(CasingAdd))
            {
                CasingListAndReturnStackPanel.Items.Remove(CasingAdd);
                CasingListAndReturnStackPanel.Items.Remove(CasingAddCustomSize);
            }
            if (CasingListAndReturnStackPanel.Items.Contains(CasingSelectAll))
            {
                CasingListAndReturnStackPanel.Items.Remove(CasingRemove);
                CasingListAndReturnStackPanel.Items.Remove(CasingSelectAll);
                CasingListAndReturnStackPanel.Items.Remove(CasingClearAll);
            }
        }
        private void Remove_Hole_Children()
        {
            if (HoleGrid.Children.Contains(HoleListListview))
                HoleGrid.Children.Remove(HoleListListview);
            if (HoleGrid.Children.Contains(HoleRefListListview))
                HoleGrid.Children.Remove(HoleRefListListview);
            if (HoleGrid.Children.Contains(HoleSourceListview))
                HoleGrid.Children.Remove(HoleSourceListview);
            if (HoleGrid.Children.Contains(HoleChoiceListview))
                HoleGrid.Children.Remove(HoleChoiceListview);
            if (HoleGrid.Children.Contains(ParameterGrid))
                HoleGrid.Children.Remove(ParameterGrid);
            if (HoleListAndReturnStackPanel.Items.Contains(HoleAdd))
            {
                HoleListAndReturnStackPanel.Items.Remove(HoleAdd);
                HoleListAndReturnStackPanel.Items.Remove(HoleAddCustomSize);
            }
            if (HoleListAndReturnStackPanel.Items.Contains(HoleSelectAll))
            {
                HoleListAndReturnStackPanel.Items.Remove(HoleRemove);
                HoleListAndReturnStackPanel.Items.Remove(HoleSelectAll);
                HoleListAndReturnStackPanel.Items.Remove(HoleClearAll);
            }
        }
        private void Casing_To_Inventory()
        {
            Remove_Casing_Children();
            CasingGrid.Children.Add(CasingRefListListview);
            CasingListAndReturnStackPanel.Items.Add(CasingAdd);
            CasingListAndReturnStackPanel.Items.Add(CasingAddCustomSize);
        }
        private void Casing_To_List()
        { 
            Remove_Casing_Children();
            CasingGrid.Children.Add(CasingListListview);
            CasingListAndReturnStackPanel.Items.Add(CasingRemove);
            CasingListAndReturnStackPanel.Items.Add(CasingSelectAll);
            CasingListAndReturnStackPanel.Items.Add(CasingClearAll);

        }
        private void Casing_To_Source()
        {
            Remove_Casing_Children();
            CasingGrid.Children.Add(CasingSourceListview);
        }
        private void Casing_To_Choice()
        {
            Remove_Casing_Children();
            CasingGrid.Children.Add(CasingChoiceListview);
        }
        private void Hole_To_Inventory()
        {
            Remove_Hole_Children();
            HoleGrid.Children.Add(HoleRefListListview);
            HoleListAndReturnStackPanel.Items.Add(HoleAdd);
            HoleListAndReturnStackPanel.Items.Add(HoleAddCustomSize);
        }
        private void Hole_To_List()
        {
            Remove_Hole_Children();
            HoleGrid.Children.Add(HoleListListview);
            HoleListAndReturnStackPanel.Items.Add(HoleRemove);
            HoleListAndReturnStackPanel.Items.Add(HoleSelectAll);
            HoleListAndReturnStackPanel.Items.Add(HoleClearAll);
        }
        private void Hole_To_Source()
        {
            Remove_Hole_Children();
            HoleGrid.Children.Add(HoleSourceListview);
        }
        private void Hole_To_Choice()
        {
            Remove_Hole_Children();
            HoleGrid.Children.Add(HoleChoiceListview);
        }
        private void Hole_To_Parameter()
        {
            Remove_Hole_Children();
            HoleGrid.Children.Add(ParameterGrid);
        }

        
    }
}
