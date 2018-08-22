# Sample-Casing-Design

Brief Description of each file in the sample project:

WellViewModel.cs:

This is the ViewModel file for well page ( you can the screenshot of the page in the main folder). 
All calculation to find the final well config is in this code page. The final well configs is shown at the right side of the well page.

Well.Xaml:

This is the UI file for the well page (screenshot). Wrote in Xaml for WPF.

Well.Xaml.CS:

This is the CS backend file attached to the well.xaml file. You can find all view changes and events in this file.

AvailableWellConfigControl.CS

As you can see in the screenshot, the control that shows the configs at right side of the page is not the common control that provided
by Microsoft. So we developed our custom control for the purpose.

LithologyModel.CS

This is the Model page for lithology data of the software. 
