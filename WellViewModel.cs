using System;
using System.Collections.Generic;
using System.Linq;
using CasingDesign.Model;
using CasingDesign.Common;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.IO;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Provider;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Windows.Foundation;
using Windows.System.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace CasingDesign.ViewModel
{
    [XmlRoot("WellPage", IsNullable = false)]
    public class WellViewModel : INotifyPropertyChanged 
    {
        TaskFactory uiFactory;
        int tempSelected = 0, tempSelected2 = 0;
        //int removeIndex = 0, addIndex, tempHoleSize = 0;
        private List<double> casingItemsToAdd;
        private List<double> holeItemsToAdd;
        private NotificationMessageAction<ObservableCollection<string>> _dataRequest;

        private IAsyncAction ThreadPoolWorkItem;
        private bool _inProgress;
        private int _configCount;
        private int _configCurrentIndex;
        [XmlIgnore]
        public int ConfigCount
        {
            get
            {
                return _configCount;
            }
            set
            {
                if (_configCount != value)
                    _configCount = value;
                OnPropertyChanged("ConfigCount");
            }
        }
        [XmlIgnore]
        public int ConfigCurrentIndex
        {
            get
            {
                return _configCurrentIndex;
            }
            set
            {
                if (_configCurrentIndex != value)
                    _configCurrentIndex = value;
                OnPropertyChanged("ConfigCurrentIndex");
            }
        }
        [XmlIgnore]
        public bool InProgress { get { return _inProgress; } set { _inProgress = value; OnPropertyChanged("InProgress"); } }
        
        public WellViewModel()
        {
            Messenger.Default.Register<WellViewModel>(this, WellDataRecieved);
            Messenger.Default.Register<NotificationMessageAction<ObservableCollection<string>>>(this, SendSizeList);
            Messenger.Default.Register<List<string>>(this, AddSizeToList);
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        private List<string> _complationTypeList;
        private string _complationType;
        [XmlIgnore]
        public List<string> ComplationTypeList
        {
            get
            {
                if (_complationTypeList == null)
                    _complationTypeList = new List<string>() { "Open Hole", "Cased Hole" };
                return _complationTypeList;
            }
        }
        public string ComplationType { get { return _complationType; } set { _complationType = value; OnPropertyChanged("ComplationTypeSelected"); } }
        
        public void FindConfig()
        {
            if (ValidateData())
            {
                ThreadPoolWorkItem = Windows.System.Threading.ThreadPool.RunAsync((source) =>
                    {
                        uiFactory.StartNew(() => { InProgress = true; }).Wait();
                        ObservableCollection<List<string>> TempFinalWellConfig = new ObservableCollection<List<string>>();
                        ObservableCollection<List<string>> WellConfig = new ObservableCollection<List<string>>();
                        if (!FirstCasingInclude && !FirstHoleInclude)
                        {
                            if (LastCasingInclude)
                            {
                                if (ComplationType == "Cased Hole")
                                {
                                    var holeAllows = HolesData.Where(item => item.AllowableCasingSizes.Contains(LastCasing.CasingSizeString)).ToList();
                                    for (int i = 0; i <= holeAllows.Count - 1; i++)
                                    {
                                        List<string> startCasingList = new List<string>();
                                        startCasingList.Add(holeAllows[i].HoleSizeString);
                                        startCasingList.Add(LastCasing.CasingSizeString);
                                        WellConfig.Add(startCasingList);
                                    }
                                }
                                else if (ComplationType == "Open Hole")
                                {
                                    var casingItem = CasingsData.Where(item => item.CasingSizeString == LastCasing.CasingSizeString).FirstOrDefault();
                                    for (int i = 0; i <= casingItem.AllowableHoleSizes.Count - 1; i++)
                                    {
                                        List<string> initialSizes = new List<string>();
                                        initialSizes.Add(LastCasing.CasingSizeString);
                                        initialSizes.Add(casingItem.AllowableHoleSizes[i]);
                                        WellConfig.Add(initialSizes);
                                    }
                                }
                            }
                            else if (LastHoleInclude)
                            {
                                if (ComplationType == "Open Hole")
                                {
                                    var casingAllows = CasingsData.Where(item => item.AllowableHoleSizes.Contains(LastHole.HoleSizeString)).ToList();
                                    for (int i = 0; i <= casingAllows.Count - 1; i++)
                                    {
                                        List<string> startHoleList = new List<string>();
                                        startHoleList.Add(casingAllows[i].CasingSizeString);
                                        startHoleList.Add(LastHole.HoleSizeString);
                                        WellConfig.Add(startHoleList);
                                    }
                                }
                                else if (ComplationType == "Cased Hole")
                                {
                                    var holeItem = HolesData.Where(item => item.HoleSizeString == LastHole.HoleSizeString).FirstOrDefault();
                                    for (int i = 0; i <= holeItem.AllowableCasingSizes.Count - 1; i++)
                                    {
                                        List<string> initialSizes = new List<string>();
                                        initialSizes.Add(LastHole.HoleSizeString);
                                        initialSizes.Add(holeItem.AllowableCasingSizes[i]);
                                        WellConfig.Add(initialSizes);
                                    }
                                }
                            }
                            if (WellConfig.Count == 0)
                            {
                                uiFactory.StartNew(() => { InProgress = false; });
                                return;
                            }
                            do
                            {
                                int itemCount;
                                List<string> itemForAdd = new List<string>();
                                List<string> wellConfigItem = new List<string>();
                                for (int i = 0; i <= WellConfig[0].Count - 1; i++)
                                {
                                    wellConfigItem.Add(WellConfig[0][i]);
                                }
                                itemCount = wellConfigItem.Count;
                                if(ComplationType == "Open Hole")
                                {
                                    if(itemCount %2 == 0)
                                    {
                                        var holeItem = HolesData.Where(item => item.AllowableCasingSizes.Contains(WellConfig[0][0])).ToList();
                                        for (int i = 0; i <= holeItem.Count - 1; i++)
                                            itemForAdd.Add(holeItem[i].HoleSizeString);
                                    }
                                    else
                                    {
                                        var casingItem = CasingsData.Where(item => item.AllowableHoleSizes.Contains(WellConfig[0][0])).ToList();
                                        for (int i = 0; i <= casingItem.Count - 1; i++)
                                            itemForAdd.Add(casingItem[i].CasingSizeString);
                                    }
                                }
                                else if (ComplationType == "Cased Hole")
                                {
                                    if (itemCount % 2 == 0)
                                    {
                                        var casingItem = CasingsData.Where(item => item.AllowableHoleSizes.Contains(WellConfig[0][0])).ToList();
                                        for (int i = 0; i <= casingItem.Count - 1; i++)
                                            itemForAdd.Add(casingItem[i].CasingSizeString);
                                    }
                                    else
                                    {
                                        var holeItem = HolesData.Where(item => item.AllowableCasingSizes.Contains(WellConfig[0][0])).ToList();
                                        for (int i = 0; i <= holeItem.Count - 1; i++)
                                            itemForAdd.Add(holeItem[i].HoleSizeString);  
                                    }
                                }
                                if(itemForAdd.Count == 0)
                                {
                                    bool numberValidate;
                                    bool endToHoleValidate = ComplationType == "Cased Hole" ? itemCount % 2 == 0 : itemCount % 2 != 0;
                                    if (!endToHoleValidate)
                                        wellConfigItem.RemoveAt(0);
                                    if (NumberOfSizeInclude)
                                    {
                                        numberValidate = wellConfigItem.Count % 2 == 0 ? wellConfigItem.Count / 2 == NumberOfCasing :
                                             (wellConfigItem.Count - 1) / 2 == NumberOfCasing;
                                    }
                                    else numberValidate = true;
                                        
                                    if (numberValidate)
                                    {
                                        TempFinalWellConfig.Add(wellConfigItem);
                                    }
                                }
                                else
                                {
                                    for (int k = 0; k <= itemForAdd.Count - 1; k++)
                                    {
                                        List<string> allowSize = new List<string>();
                                        for (int g = 0; g <= wellConfigItem.Count - 1; g++)
                                            allowSize.Add(wellConfigItem[g]);
                                        allowSize.Insert(0, itemForAdd[k]);
                                        WellConfig.Add(allowSize);
                                    }
                                }
                                WellConfig.RemoveAt(0);
                            } while (WellConfig.Count != 0);   
                        }
                        else
                        {
                            if (FirstCasingInclude)
                            {
                                var startHoleList = HolesData.Where(item => item.AllowableCasingSizes.Contains(FirstCasing.CasingSizeString)).ToList();
                                for(int i = 0 ; i<= startHoleList.Count - 1 ;i++)
                                {
                                    List<string> initialSizes = new List<string>();
                                    initialSizes.Add(startHoleList[i].HoleSizeString);
                                    initialSizes.Add(FirstCasing.CasingSizeString);
                                    WellConfig.Add(initialSizes);
                                }
                            }
                            else if (FirstHoleInclude)
                            {
                                var startHole = HolesData.Where(item => item.HoleSizeString == FirstHole.HoleSizeString).FirstOrDefault();
                                for(int i = 0 ; i<= startHole.AllowableCasingSizes.Count - 1; i++)
                                    {
                                        var itemtoadd = startHole.AllowableCasingSizes[i];
                                        List<string> initialSizes = new List<string>();
                                        initialSizes.Add(startHole.HoleSizeString);
                                        initialSizes.Add(itemtoadd);
                                        WellConfig.Add(initialSizes);
                                    }
                            }
                            if (WellConfig.Count == 0)
                            {
                                uiFactory.StartNew(() => { InProgress = false;});
                                return;
                            }
                            do
                            {
                                List<string> wellConfigItem = new List<string>();
                                ObservableCollection<string> itemForAdd = new ObservableCollection<string>();
                                for (int i = 0; i <= WellConfig[0].Count - 1; i++)
                                {
                                    wellConfigItem.Add(WellConfig[0][i]);
                                }
                                int countoflist = WellConfig[0].Count;
                                itemForAdd = countoflist % 2 == 0 ? CasingsData.Where(size => size.CasingSizeString == wellConfigItem[WellConfig[0].Count - 1]).FirstOrDefault().AllowableHoleSizes
                                    : HolesData.Where(size => size.HoleSizeString == wellConfigItem[WellConfig[0].Count - 1]).FirstOrDefault().AllowableCasingSizes;
                                if (LastCasingInclude &&
                                    !LastHoleInclude &&
                                    countoflist % 2 == 1 &&
                                    itemForAdd.Contains(LastCasing.CasingSizeString))
                                {
                                    bool numberValidate;
                                    var lastCasigItem = itemForAdd.Where(item => item == LastCasing.CasingSizeString).FirstOrDefault();
                                    List<string> allowSize = new List<string>();
                                    for (int g = 0; g <= wellConfigItem.Count - 1; g++)
                                        allowSize.Add(wellConfigItem[g]);
                                    allowSize.Add(lastCasigItem);
                                    if (ComplationType == "Open Hole")
                                    {
                                        var casingItem = CasingsData.Where(item => item.CasingSizeString == LastCasing.CasingSizeString).FirstOrDefault();
                                        for (int i = 0; i <= casingItem.AllowableHoleSizes.Count - 1; i++)
                                        {
                                            List<string> lastHoles = new List<string>(allowSize);
                                            lastHoles.Add(casingItem.AllowableHoleSizes[i]);
                                            numberValidate = NumberOfSizeInclude ? (lastHoles.Count-1) / 2 == NumberOfCasing : true;
                                            if (numberValidate)
                                                TempFinalWellConfig.Add(lastHoles);

                                        }
                                    }
                                    else if (ComplationType == "Cased Hole")
                                    {
                                        numberValidate = NumberOfSizeInclude ? (allowSize.Count ) / 2 == NumberOfCasing : true;
                                        if (numberValidate)
                                            TempFinalWellConfig.Add(allowSize);
                                    }
                                }
                                else if (!LastCasingInclude &&
                                    LastHoleInclude &&
                                    countoflist % 2 == 0 &&
                                    itemForAdd.Contains(LastHole.HoleSizeString))
                                {
                                    bool numberValidate;
                                    var lastHoleItem = itemForAdd.Where(item => item == LastHole.HoleSizeString).FirstOrDefault();
                                    List<string> allowSize = new List<string>();
                                    for (int g = 0; g <= wellConfigItem.Count - 1; g++)
                                        allowSize.Add(wellConfigItem[g]);
                                    allowSize.Add(lastHoleItem);
                                    if (ComplationType == "Cased Hole")
                                    {
                                        var holeItem = HolesData.Where(item => item.HoleSizeString == LastHole.HoleSizeString).FirstOrDefault();
                                        for (int i = 0; i <= holeItem.AllowableCasingSizes.Count - 1; i++)
                                        {
                                            List<string> lastCasings = new List<string>(allowSize);
                                            lastCasings.Add(holeItem.AllowableCasingSizes[i]);
                                            numberValidate = NumberOfSizeInclude ? lastCasings.Count / 2 == NumberOfCasing : true;
                                            if (numberValidate)
                                                TempFinalWellConfig.Add(lastCasings);
                                        }
                                    }
                                    else if (ComplationType == "Open Hole")
                                    {
                                        numberValidate = NumberOfSizeInclude ? (allowSize.Count - 1) / 2 == NumberOfCasing : true;
                                        if (numberValidate)
                                            TempFinalWellConfig.Add(allowSize);
                                    }
                                }
                                else if (!LastCasingInclude && !LastHoleInclude && NumberOfSizeInclude)
                                {
                                    bool numberValidate = ComplationType == "Open Hole" ? (wellConfigItem.Count - 1) / 2 == NumberOfCasing : wellConfigItem.Count / 2 == NumberOfCasing;
                                    if (wellConfigItem.Count == NumberOfCasing)
                                        TempFinalWellConfig.Add(wellConfigItem);
                                }
                                else if (
                                    itemForAdd.Count == 0 &&
                                    !LastCasingInclude &&
                                    !LastHoleInclude &&
                                    !NumberOfSizeInclude)
                                {
                                    bool isValidateToAdd = ComplationType == "Open Hole" ? countoflist % 2 == 1 : countoflist % 2 == 0;
                                    if (isValidateToAdd)
                                        TempFinalWellConfig.Add(wellConfigItem);
                                }
                                if (itemForAdd.Count != 0)
                                {
                                    for (int k = 0; k <= itemForAdd.Count - 1; k++)
                                    {
                                        List<string> allowSize = new List<string>();
                                        for (int g = 0; g <= wellConfigItem.Count - 1; g++)
                                            allowSize.Add(wellConfigItem[g]);
                                        allowSize.Add(itemForAdd[k]);
                                        WellConfig.Add(allowSize);
                                    }
                                }
                                WellConfig.RemoveAt(0);
                            } while (WellConfig.Count != 0);
                        }
                        uiFactory.StartNew(() =>
                        {
                            FinalWellConfig.Clear();
                            for (int k = 0; k <= TempFinalWellConfig.Count - 1; k++)
                            {
                                FinalWellConfig.Add(TempFinalWellConfig[k]);
                            }
                            InProgress = false;
                            ConfigCount = FinalWellConfig.Count;
                            if (ConfigCount == 0)
                            {
                                NoConfigCaution = "There Are No Configurations With Selected Options";
                            }
                            else
                            {
                                NoConfigCaution = null;
                            }
                        }).Wait();
                    });
                ThreadPoolWorkItem.Completed += Calculatecompleted;
            }
        }

        private void Calculatecompleted(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
           
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                if( _errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged("ErrorMessage");
                }
            }
        }
        private string _noConfigCaution;
        public string NoConfigCaution
        {
            get
            {
                return _noConfigCaution;
            }
            set
            {
                if (_noConfigCaution != value)
                {
                    _noConfigCaution = value;
                    OnPropertyChanged("NoConfigCaution");
                }
            }
        }
        private bool ValidateData()
        {
            ErrorMessage = null;
            NoConfigCaution = null;
            ErrorMessage = ComplationType == null ? "Plaese Select ComplationType." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = CasingsData.Count == 0 ? "Please Select Some Casings." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = HolesData.Count == 0 ? "Please Select Some Holes." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = 
                !FirstCasingInclude &&
                !FirstHoleInclude &&
                !LastCasingInclude &&
                !LastHoleInclude &&
                !NumberOfSizeInclude ? "Please Select Some Option For Customize By Click Option Titles." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = FirstCasingInclude && FirstCasing == null ? "Please Select Value For Selected Options." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = FirstHoleInclude && FirstHole == null ? "Please Select Value For Selected Options." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = LastCasingInclude && LastCasing == null ? "Please Select Value For Selected Options." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = LastHoleInclude && LastHole == null ? "Please Select Value For Selected Options." : null;
            if (ErrorMessage != null) return false;
            ErrorMessage = NumberOfSizeInclude && NumberOfCasing == 0 ? "Please Enter Validated Value for Number." : null;
            if (ErrorMessage != null) return false;
            if (FirstCasingInclude)
            {
                if (!HolesData.Where(item => item.AllowableCasingSizes.Contains(FirstCasing.CasingSizeString)).Any())
                {
                    ErrorMessage = "Selected First Casing Must Be In Some Allowable Casing Sizes";
                    return false;
                }
                else ErrorMessage = null;
            }
            else ErrorMessage = null;
            if (FirstHoleInclude)
            {
                if (FirstHole.AllowableCasingSizes.Count == 0)
                {
                    ErrorMessage = "Selected First Hole Must Have Some Allowable Casing Sizes";
                    return false;
                }
                else ErrorMessage = null;
            }
            else ErrorMessage = null;
            ErrorMessage =
               !FirstCasingInclude &&
               !FirstHoleInclude &&
               !LastCasingInclude &&
               !LastHoleInclude &&
               NumberOfSizeInclude ? "Please Select At Least One Option About Size" : null;
            if (ErrorMessage != null) return false;
            if(FirstCasingInclude && LastCasingInclude && Helpers.ConvertStringToDouble(FirstCasing.CasingSizeString) <= Helpers.ConvertStringToDouble(LastCasing.CasingSizeString))
            {
                ErrorMessage = "Last Casing Size Must be Smaller Than Fisrt Casing Size.";
                return false;
            }
            else
            {
                ErrorMessage = null;
            }
            if (FirstHoleInclude && LastHoleInclude && Helpers.ConvertStringToDouble(FirstHole.HoleSizeString) <= Helpers.ConvertStringToDouble(LastHole.HoleSizeString))
            {
                ErrorMessage = "Last Hole Size Must be Smaller Than Fisrt Hole Size.";
                return false;
            }
            if (FirstCasingInclude && LastHoleInclude)
            {
                for(int i = 0; i<= FirstCasing.AllowableHoleSizes.Count - 1 ;i++)
                {
                    if (Helpers.ConvertStringToDouble(FirstCasing.AllowableHoleSizes[i]) <= Helpers.ConvertStringToDouble(LastHole.HoleSizeString))
                    {
                        ErrorMessage = "Last Hole Selected Size Must Be Smaller Than Casing Selected Allowable Hole Sizes.";
                        return false;
                    }
                }
            }
            else
            {
                ErrorMessage = null;
            }
            if (FirstHoleInclude && LastCasingInclude)
            {
                for (int i = 0; i <= FirstHole.AllowableCasingSizes.Count - 1; i++)
                {
                    if (Helpers.ConvertStringToDouble(FirstHole.AllowableCasingSizes[i]) <= Helpers.ConvertStringToDouble(LastCasing.CasingSizeString))
                    {
                        ErrorMessage = "Last Casing Selected Size Must Be Smaller Than Hole Selected Allowable Casing Sizes.";
                        return false;
                    }
                }
            }
            else
            {
                ErrorMessage = null;
            }
            
            return true;
        }
        private CasingsModel _firstCasing;
        private HolesModel _firstHole;
        private CasingsModel _lastCasing;
        private HolesModel _lastHole;
        private double _numberOfsize;
        [XmlIgnore]
        public double NumberOfCasing
        {
            get
            {
                return _numberOfsize;
            }
            set
            {
                if(_numberOfsize != value)
                {
                    _numberOfsize = value;
                    OnPropertyChanged("NumberOfCasing");
                }
            }
        }
        [XmlIgnore]
        public CasingsModel FirstCasing
        {
            get { return _firstCasing; }
            set
            {
                _firstCasing = value;
                OnPropertyChanged("FirstCasing");
            }
        }
        [XmlIgnore]
        public HolesModel FirstHole
        {
            get { return _firstHole; }
            set
            {
                _firstHole = value;
                OnPropertyChanged("FirstHole");
            }
        }
        [XmlIgnore]
        public CasingsModel LastCasing
        {
            get { return _lastCasing; }
            set
            {
                _lastCasing = value;
                OnPropertyChanged("LastCasing");
            }
        }
        [XmlIgnore]
        public HolesModel LastHole
        {
            get { return _lastHole; }
            set
            {
                _lastHole = value;
                OnPropertyChanged("LastHole");
            }
        }
        
        [XmlIgnore]
        public bool FirstCasingInclude
        {
            get
            {
                return _firstCasingInclude;
            }
            set
            {
                _firstCasingInclude = value;
                if(value && FirstHoleInclude)
                {
                    FirstHoleInclude = false;
                }
                OnPropertyChanged("FirstCasingInclude");
            }
        }
        [XmlIgnore]
        public bool LastCasingInclude
        {
            get
            {
                return _lastCasingInclude;
            }
            set
            {
                _lastCasingInclude = value;
                if (value && LastHoleInclude)
                    LastHoleInclude = false;
                OnPropertyChanged("LastCasingInclude");
            }
        }

        [XmlIgnore]
        public bool FirstHoleInclude
        {
            get
            { 
                return _firstHoleInclude; 
            } 
            set 
            { 
                _firstHoleInclude = value;
                if (value && FirstCasingInclude)
                    FirstCasingInclude = false;
                OnPropertyChanged("FirstHoleInclude");
            }
        }
        [XmlIgnore]
        public bool LastHoleInclude
        {
            get
            { 
                return _lastHoleInclude;
            } 
            set
            { 
                _lastHoleInclude = value;
                if (value && LastCasingInclude)
                    LastCasingInclude = false;
                OnPropertyChanged("LastHoleInclude");
            } 
        }
        [XmlIgnore]
        public bool NumberOfSizeInclude
        {
            get
            {
                return _numberOfSizeInclude;
            }
            set
            {
                if (_numberOfSizeInclude != value)
                {
                    _numberOfSizeInclude = value;
                    OnPropertyChanged("NumberOfSizeInclude");
                }
            }
        }


        #region Methods For Casing
        private void Show_Casing_Allowable()
        {
            CasingSourceSelectedIndex = -1;
        }
        private void Casing_Source_Selection_Changed()
        {
            if (CasingSourceSelectedIndex == -1)
            {
                AllowableHoleItems.Clear();
                if (IsCasingSourceItemSelected == true) IsCasingSourceItemSelected = false;
            }
            else
            {
                if (IsCasingSourceItemSelected == false)
                {
                    var item5 = CasingsData[CasingSourceSelectedIndex];
                    for (int f = 0; f <= HolesData.Count - 1; f++)
                    {
                        var item1 = HolesData[f];
                        if (item1.HoleSize < item5.CasingSize)
                            AllowableHoleItems.Add(item1.HoleSizeString);
                    }
                    CasingSourceTempIndex = CasingSourceSelectedIndex;
                    IsCasingSourceItemSelected = true;
                }
                else
                {
                    //Copy selectedTarget To Dic[temp].
                    var item2 = CasingsData[CasingSourceTempIndex];
                    item2.AllowableHoleSizes.Clear();
                    for (int l = 0; l <= AllowableHoleSelectedItems.Count - 1; l++)
                        item2.AllowableHoleSizes.Add((string)AllowableHoleSelectedItems[l]);
                    // Clear Target Selected Items.
                    AllowableHoleSelectedItems.Clear();
                    AllowableHoleItems.Clear();
                    // Create available Hole List for Choice.
                    var item4 = CasingsData[CasingSourceSelectedIndex];
                    for (int f = 0; f <= HolesData.Count - 1; f++)
                    {
                        var item7 = HolesData[f];
                        if (item7.HoleSize < item4.CasingSize)
                            AllowableHoleItems.Add(item7.HoleSizeString);
                    }
                }
                //Copy Dic[temp] to selecteditems.
                var item6 = CasingsData[CasingSourceSelectedIndex];
                {
                    for (int n = 0; n <= item6.AllowableHoleSizes.Count - 1; n++)
                        AllowableHoleSelectedItems.Add(item6.AllowableHoleSizes[n]);
                }
                SaveCountor = SaveCountor + 1;
                CasingSourceTempIndex = CasingSourceSelectedIndex;
            }
        }
        private void Return_Casing_Allowable_To_List()
        {

        }
        private void Return_Casing_Ref_To_List()
        {
        }
        private void Clear_All_Casing_List()
        {
            CasingListSelectedItems.Clear();
        }
        private void Select_All_Casing_List()
        {
            CasingListSelectedItems.Clear();
            for (int i = 0; i <= CasingsData.Count - 1; i++)
            {
                var item = CasingsData[i];
                CasingListSelectedItems.Add(item);
            }
        }
        private void Add_To_Casing_List()
        {
            casingItemsToAdd = new List<double>();
            casingItemsToAdd.Clear();
            for (int i = 0; i <= CasingRefSelectedItems.Count - 1; i++)
            {
                casingItemsToAdd.Add(_casingInventory[(string)CasingRefSelectedItems[i]]);
            }
            casingItemsToAdd.Sort();
            if (CasingsData.Count == 0)
            {
                for (int l = casingItemsToAdd.Count - 1; l >= 0; l--)
                {
                    CasingsData.Add(new CasingsModel { CasingSizeString = _casingInventory.FirstOrDefault(x => x.Value == casingItemsToAdd[l]).Key, CasingSize = casingItemsToAdd[l] });
                }
            }
            //add Item too Casing List
            else
            {
                for (int x = casingItemsToAdd.Count - 1; x >= 0; x--)
                {
                    for (int y = 0; y <= CasingsData.Count - 1; y++)
                    {
                        var item = CasingsData[y];
                        if (casingItemsToAdd[x] > item.CasingSize)
                        {
                            tempSelected++;
                            CasingsData.Add(new CasingsModel { CasingSize = casingItemsToAdd[x], CasingSizeString = _casingInventory.FirstOrDefault(a => a.Value == casingItemsToAdd[x]).Key });
                            CasingsData.Move(CasingsData.Count - 1, y);
                            break;
                        }
                        else if (casingItemsToAdd[x] == item.CasingSize)
                        {
                            tempSelected++;
                            break;
                        }
                    }
                    if (tempSelected == 0)
                        CasingsData.Add(new CasingsModel { CasingSize = casingItemsToAdd[x], CasingSizeString = _casingInventory.FirstOrDefault(a => a.Value == casingItemsToAdd[x]).Key });
                    tempSelected = 0;
                }
                //remove items from Casing List
                List<int> indexlist = new List<int>();
                for (int r = CasingsData.Count - 1; r >= 0; r--)
                {
                    var item = CasingsData[r];
                    for (int n = 0; n <= casingItemsToAdd.Count - 1; n++)
                    {
                        if (item.CasingSize == casingItemsToAdd[n])
                        {
                            tempSelected2++;
                            break;
                        }
                    }
                    if (tempSelected2 == 0)
                        indexlist.Add(r);
                    tempSelected2 = 0;
                }
                indexlist.Sort();
                for (int s = indexlist.Count - 1; s >= 0; s--)
                {
                    CasingsData.RemoveAt(indexlist[s]);
                }
            }
            SaveCountor = 0;
            SaveCountor = SaveCountor + 1;
        }
        private void Show_Casing_Ref_List()
        {
            CasingRefSelectedItems.Clear();
            for (int i = 0; i <= CasingsData.Count - 1; i++)
            {
                var item = CasingsData[i];
                CasingRefSelectedItems.Add(item.CasingSizeString);
            }
        }
        private void Filter_Casing_Ref_List()
        {

        }

        private void Remove_From_Casing_List()
        {
            for (int k = 0; k <= CasingListSelectedItems.Count - 1; k++)
            {
                for (int l = 0; l <= HolesData.Count - 1; l++)
                {
                    var holeItem = HolesData[l];
                    for (int n = 0; n <= holeItem.AllowableCasingSizes.Count - 1; n++)
                    {
                        var allowableCasings = holeItem.AllowableCasingSizes[n];
                        if (CasingListSelectedItems[k].CasingSizeString == allowableCasings)
                            holeItem.AllowableCasingSizes.RemoveAt(n);
                    }
                }
            }
            if (CasingListSelectedItems == null)
                return;
            List<int> indexlist = new List<int>();
            for (int i = CasingListSelectedItems.Count - 1; i >= 0; i--)
            {
                var item = CasingListSelectedItems[i];
                int indexOfSelectionItems = CasingsData.IndexOf(item);
                indexlist.Add(indexOfSelectionItems);
            }
            indexlist.Sort();
            for (int j = indexlist.Count - 1; j >= 0; j--)
            {
                CasingsData.RemoveAt(indexlist[j]);
            }


            SaveCountor = SaveCountor + 1;
        }

        #endregion

        #region Methods For Hole
        private void Show_Hole_Allowable()
        {
            HoleSourceSelectedIndex = -1;
        }
        private void Hole_Source_Selection_Changed()
        {
            if (HoleSourceSelectedIndex == -1)
            {
                AllowableCasingItems.Clear();
                if (IsHoleSourceItemSelected == true) IsHoleSourceItemSelected = false;

            }
            else
            {
                if (IsHoleSourceItemSelected == false)
                {
                    var item5 = HolesData[HoleSourceSelectedIndex];
                    for (int f = 0; f <= CasingsData.Count - 1; f++)
                    {
                        var item1 = CasingsData[f];
                        if (item1.CasingSize < item5.HoleSize)
                            AllowableCasingItems.Add(item1.CasingSizeString);
                    }
                    HoleSourceTempIndex = HoleSourceSelectedIndex;
                    IsHoleSourceItemSelected = true;
                }
                else
                {
                    //Copy selectedTarget To Dic[temp].
                    var item2 = HolesData[HoleSourceTempIndex];
                    item2.AllowableCasingSizes.Clear();
                    for (int l = 0; l <= AllowableCasingSelectedItems.Count - 1; l++)
                        item2.AllowableCasingSizes.Add((string)AllowableCasingSelectedItems[l]);
                    // Clear Target Selected Items.
                    AllowableCasingSelectedItems.Clear();
                    AllowableCasingItems.Clear();
                    // Create available Hole List for Choice.
                    var item4 = HolesData[HoleSourceSelectedIndex];
                    for (int f = 0; f <= CasingsData.Count - 1; f++)
                    {
                        var item7 = CasingsData[f];
                        if (item7.CasingSize < item4.HoleSize)
                            AllowableCasingItems.Add(item7.CasingSizeString);
                    }
                }
                //Copy Dic[temp] to selecteditems.
                var item6 = HolesData[HoleSourceSelectedIndex];
                {
                    for (int n = 0; n <= item6.AllowableCasingSizes.Count - 1; n++)
                        AllowableCasingSelectedItems.Add(item6.AllowableCasingSizes[n]);
                }
                HoleSourceTempIndex = HoleSourceSelectedIndex;
            }
            SaveCountor = SaveCountor + 1;
        }
        private void Return_Hole_Allowable_To_List()
        {
        }
        private void Return_Hole_Ref_To_List()
        {
        }
        private void Clear_All_Hole_List()
        {
            HoleListSelectedItems.Clear();
        }
        private void Select_All_Hole_List()
        {
            HoleListSelectedItems.Clear();
            for (int i = 0; i <= HolesData.Count - 1; i++)
            {
                var item = HolesData[i];
                HoleListSelectedItems.Add(item);
            }
        }
        private void Add_To_Hole_List()
        {
            holeItemsToAdd = new List<double>();
            holeItemsToAdd.Clear();
            for (int i = 0; i <= HoleRefSelectedItems.Count - 1; i++)
            {
                holeItemsToAdd.Add(_holeInventory[(string)HoleRefSelectedItems[i]]);
            }
            holeItemsToAdd.Sort();
            if (HolesData.Count == 0)
            {
                for (int l = holeItemsToAdd.Count - 1; l >= 0; l--)
                {
                    HolesData.Add(new HolesModel { HoleSize = holeItemsToAdd[l], HoleSizeString = _holeInventory.FirstOrDefault(x => x.Value == holeItemsToAdd[l]).Key });
                }
            }
            //add Item too Hole List
            else
            {
                for (int x = holeItemsToAdd.Count - 1; x >= 0; x--)
                {
                    for (int y = 0; y <= HolesData.Count - 1; y++)
                    {
                        var item = HolesData[y];
                        if (holeItemsToAdd[x] > item.HoleSize)
                        {
                            tempSelected++;
                            HolesData.Add(new HolesModel { HoleSize = holeItemsToAdd[x], HoleSizeString = _holeInventory.FirstOrDefault(a => a.Value == holeItemsToAdd[x]).Key });
                            HolesData.Move(HolesData.Count - 1, y);
                            break;
                        }
                        else if (holeItemsToAdd[x] == item.HoleSize)
                        {
                            tempSelected++;
                            break;
                        }
                    }
                    if (tempSelected == 0)
                        HolesData.Add(new HolesModel { HoleSize = holeItemsToAdd[x], HoleSizeString = _holeInventory.FirstOrDefault(a => a.Value == holeItemsToAdd[x]).Key });
                    tempSelected = 0;
                }
                //remove items from Hole List
                List<int> indexlist = new List<int>();
                for (int r = HolesData.Count - 1; r >= 0; r--)
                {
                    var item = HolesData[r];
                    for (int n = 0; n <= holeItemsToAdd.Count - 1; n++)
                    {
                        if (item.HoleSize == holeItemsToAdd[n])
                        {
                            tempSelected2++;
                            break;
                        }
                    }
                    if (tempSelected2 == 0)
                        indexlist.Add(r);
                    tempSelected2 = 0;
                }
                indexlist.Sort();
                for (int s = indexlist.Count - 1; s >= 0; s--)
                {
                    HolesData.RemoveAt(indexlist[s]);
                }
            }
            SaveCountor = SaveCountor + 1;
        }
        private void Show_Hole_Ref_List()
        {
            HoleRefSelectedItems.Clear();
            for (int i = 0; i <= HolesData.Count - 1; i++)
            {
                var item = HolesData[i];
                HoleRefSelectedItems.Add(item.HoleSizeString);
            }
        }
        private void Filter_Hole_Ref_List()
        {     
        }
        private void Remove_From_Hole_List()
        {
            for (int k = 0; k <= HoleListSelectedItems.Count - 1; k++)
            {
                for (int l = 0; l <= CasingsData.Count - 1; l++)
                {
                    var casingItem = CasingsData[l];
                    for (int n = 0; n <= casingItem.AllowableHoleSizes.Count - 1; n++)
                    {
                        var allowableHoles = casingItem.AllowableHoleSizes[n];
                        if (HoleListSelectedItems[k].HoleSizeString == allowableHoles)
                            casingItem.AllowableHoleSizes.RemoveAt(n);
                    }
                }
            }
            List<int> indexlist = new List<int>();
            for (int i = HoleListSelectedItems.Count - 1; i >= 0; i--)
            {
                var item = HoleListSelectedItems[i];
                int indexOfSelectionItems = HolesData.IndexOf(item);
                indexlist.Add(indexOfSelectionItems);
            }
            indexlist.Sort();
            for (int j = indexlist.Count - 1; j >= 0; j--)
            {
                HolesData.RemoveAt(indexlist[j]);
            }
            SaveCountor = SaveCountor + 1;
        }

        #endregion

        #region Functions
        private void AddSizeToList(List<string> obj)
        {
            if (obj.Count != 3) return;
            if (obj[2] == "Add")
            {
                double recievedSize = Helpers.ConvertStringToDouble(obj[0]);
                if (obj[1] == "Casing")
                {
                    CasingInventory.Add(obj[0], recievedSize);
                    SortNewRefList("Casing", recievedSize, obj[0], "Add");
                }
                if (obj[1] == "Hole")
                {
                    HoleInventory.Add(obj[0], recievedSize);
                    SortNewRefList("Hole", recievedSize, obj[0], "Add");
                }
            }
            if (obj[2] == "Delete")
            {
                double recievedSize = Helpers.ConvertStringToDouble(obj[0]);
                if (obj[1] == "Casing")
                {
                    CasingInventory.Remove(obj[0]);
                    SortNewRefList("Casing", recievedSize, obj[0], "Delete");
                }
                if (obj[1] == "Hole")
                {
                    HoleInventory.Remove(obj[0]);
                    SortNewRefList("Hole", recievedSize, obj[0], "Delete");
                }
            }
        }

        private void SortNewRefList(string sizeType, double itemDouble, string stringItem, string command)
        {
            if (command == "Add")
            {
                if (sizeType == "Hole")
                {
                    for (int i = 0; i <= HoleInventoryList.Count - 1; i++)
                    {
                        if (Helpers.ConvertStringToDouble(HoleInventoryList[i]) > itemDouble)
                        {
                            HoleInventoryList.Insert(i, stringItem);
                            break;
                        }
                        if (i == HoleInventoryList.Count - 1)
                        {
                            HoleInventoryList.Add(stringItem);
                            break;
                        }
                    }
                }
                if (sizeType == "Casing")
                {
                    for (int i = 0; i <= CasingInventoryList.Count - 1; i++)
                    {
                        if (Helpers.ConvertStringToDouble(CasingInventoryList[i]) > itemDouble)
                        {
                            CasingInventoryList.Insert(i, stringItem);
                            break;
                        }
                        if (i == CasingInventoryList.Count - 1)
                        {
                            CasingInventoryList.Add(stringItem);
                            break;
                        }
                    }
                }
            }
            else if (command == "Delete")
            {
                if (sizeType == "Hole")
                {
                    for (int i = 0; i <= HoleInventoryList.Count - 1; i++)
                    {
                        if (Helpers.ConvertStringToDouble(HoleInventoryList[i]) == itemDouble)
                        {
                            HoleInventoryList.Remove(stringItem);
                            break;
                        }
                    }
                }
                if (sizeType == "Casing")
                {
                    for (int i = 0; i <= CasingInventoryList.Count - 1; i++)
                    {
                        if (Helpers.ConvertStringToDouble(CasingInventoryList[i]) == itemDouble)
                        {
                            CasingInventoryList.Remove(stringItem);
                            break;
                        }
                    }
                }
            }
        }

        private void SendSizeList(NotificationMessageAction<ObservableCollection<string>> obj)
        {
            _dataRequest = obj;
            if (obj.Notification == "Well")
            {
                _dataRequest.Execute(CasingInventoryList);
            }
            if (obj.Notification == "Hole")
            {
                _dataRequest.Execute(HoleInventoryList);
            }
        }

        //private void CreateListinList()
        //{
        //    for (int i = 0; i <= 10; i++)
        //    {
        //        List<string> abc = new List<string>();
        //        for (int j = 0; j <= 10 - i; j++)
        //        {
        //            abc.Add("13 7/8");
        //        }
        //        ListSource.Add(abc);
        //    }
        //}

        private void WellDataRecieved(WellViewModel obj)
        {
            CasingsData = obj.CasingsData;
            HolesData = obj.HolesData;
            Messenger.Default.Send<CasingsDataCollection, DataViewModel>(CasingsData);
            Messenger.Default.Send<HolesDataCollection, DataViewModel>(HolesData);
        }

        //private async void SaveDatatoXml()
        //{
        //    SessionFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
        //    if (SessionFile != null)
        //    {
        //        using (IRandomAccessStream sessionRandomAccess = await SessionFile.OpenAsync(FileAccessMode.ReadWrite))
        //        {
        //            using (IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0))
        //            {
        //                CachedFileManager.DeferUpdates(SessionFile);
        //                var serializer = new XmlSerializer(typeof(WellViewModel));
        //                //Using DataContractSerializer , look at the cat-class
        //                //var sessionSerializer = new DataContractSerializer(typeof(List<object>), new Type[] { typeof(T) });
        //                //sessionSerializer.WriteObject(sessionOutputStream.AsStreamForWrite(), _data);

        //                //Using XmlSerializer , look at the Dog-class
        //                serializer.Serialize(sessionOutputStream.AsStreamForWrite(), this);
        //                sessionRandomAccess.Dispose();
        //                await sessionOutputStream.FlushAsync();
        //                sessionOutputStream.Dispose();
        //            }
        //        }
        //        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(SessionFile);
        //    }
        //}
        //async private void CheckFileForSave()
        //{
        //    WellViewModel dataForLastSaveFile = new WellViewModel();
        //    StorageFolder saveFilesFolder = KnownFolders.DocumentsLibrary;
        //    StorageFile lastSaveFile;
        //    var option = new Windows.Storage.Search.QueryOptions { FolderDepth = Windows.Storage.Search.FolderDepth.Shallow, FileTypeFilter = { ".dhcd" } };
        //    var query = saveFilesFolder.CreateFileQueryWithOptions(option);
        //    var saveFiles = await query.GetFilesAsync();
        //    switch (saveFiles.Count)
        //    {
        //        case 0:
        //            lastSaveFile = null;
        //            break;
        //        case 1:
        //            lastSaveFile = saveFiles[0];
        //            break;
        //        default:
        //            lastSaveFile = saveFiles[saveFiles.Count - 2];
        //            break;
        //    }
        //    if (lastSaveFile == null)
        //    {
        //        SaveDatatoXml();
        //    }
        //    else
        //    {
        //        IInputStream sessionInputStream = await lastSaveFile.OpenReadAsync();
        //        var serializer = new XmlSerializer(typeof(WellViewModel));
        //        dataForLastSaveFile = (WellViewModel)serializer.Deserialize(sessionInputStream.AsStreamForRead());
        //        sessionInputStream.Dispose();
        //        //var savedFileProperties = dataForLastSaveFile.GetType().GetTypeInfo().DeclaredProperties;
        //        //var runtimeProperties = this.GetType().GetTypeInfo().DeclaredProperties;
        //        //foreach (var savedFileProperty in savedFileProperties)
        //        //    if (savedFileProperty.Name == "CasingsData")
        //        //    {
        //        //    }
        //        //var savedFilePropertyValue = savedFileProperty.GetValue(savedFileProperties, null);
        //        //var runtimepropertyValue = runtimeProperty.GetValue(this, null);
        //        //if (savedFilePropertyValue == runtimepropertyValue) checkFilesTemp++;

        //        if (dataForLastSaveFile.CasingsData.Count != this.CasingsData.Count) SaveDatatoXml();
        //        else
        //        {
        //            for (int i = 0; i <= this.CasingsData.Count - 1; i++)
        //            {
        //                var item1 = dataForLastSaveFile.CasingsData[i];
        //                var item2 = this.CasingsData[i];
        //                if (item1.CasingSize != item2.CasingSize)
        //                {
        //                    checkFilesTemp++;
        //                    break;
        //                }
        //                if (item1.AllowableHoleSizes.Count != item2.AllowableHoleSizes.Count) SaveDatatoXml();
        //                else
        //                {
        //                    for (int j = 0; j <= item1.AllowableHoleSizes.Count - 1; j++)
        //                    {
        //                        if (item1.AllowableHoleSizes[j] != item2.AllowableHoleSizes[j])
        //                        {
        //                            checkFilesTemp2++;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (checkFilesTemp != 0 || checkFilesTemp2 != 0)
        //                    SaveDatatoXml();
        //                checkFilesTemp = 0;
        //                checkFilesTemp2 = 0;
        //            }
        //        }
        //    }
        //}
        #endregion

        #region Properties

        [XmlIgnore]
        public ObservableCollection<List<string>> FinalWellConfig
        {
            get
            {
                if (_finalWellConfig == null)
                    _finalWellConfig = new ObservableCollection<List<string>>();
                return _finalWellConfig;
            }
            set
            {

                _finalWellConfig = value;
                OnPropertyChanged("FinalWellConfig");
            }
        }
        [XmlIgnore]
        public List<string> DrillPipeODList
        {
            get
            {
                return _drillPipeODList;
            }

        }
        [XmlIgnore]
        public List<string> BHAODList
        {
            get
            {
                return _BHAODList;
            }

        }
        [XmlIgnore]
        public Dictionary<string, double> CasingInventory
        {
            get
            {
                return _casingInventory;
            }

        }
        
        [XmlIgnore]
        public ObservableCollection<string> CasingInventoryList
        {
            get
            {
                if (_casingInventoryList == null)
                {
                    _casingInventoryList = new ObservableCollection<string>();
                    CasingInventoryList.Clear();
                    foreach (var item in CasingInventory)
                        CasingInventoryList.Add(item.Key);
                }
                return _casingInventoryList;
            }
        }
        [XmlIgnore]
        public Dictionary<string, double> HoleInventory
        {
            get
            {
                return _holeInventory;
            }
        }
        
        [XmlIgnore]
        public ObservableCollection<string> HoleInventoryList
        {
            get
            {
                if (_holeInventoryList == null)
                {
                    _holeInventoryList = new ObservableCollection<string>();
                    HoleInventoryList.Clear();
                    foreach (var item in HoleInventory)
                        HoleInventoryList.Add(item.Key);
                }
                return _holeInventoryList;
            }
        }
        [XmlArray("Holes")]
        public HolesDataCollection HolesData
        {
            get
            {
                return _holesData;
                
            }
            set
            {
                if (_holesData != value)
                {
                    _holesData = value;
                    OnPropertyChanged("HolesData");
                }
            }
        }
        [XmlArray("Casings")]
        public CasingsDataCollection CasingsData
        {
            get
            {
                return _casingsData;
            }
            set
            {
                    _casingsData = value;
                    OnPropertyChanged("CasingsData");
            }
        }

        [XmlIgnore]
        public ObservableCollection<Model.HolesModel> HoleListSelectedItems { get { return _holeListSelectedItems; } set { _holeListSelectedItems = value; OnPropertyChanged("HoleListSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<object> HoleRefSelectedItems { get { return _holeRefSelectedItems; } set { _holeRefSelectedItems = value; OnPropertyChanged("HoleRefSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<object> AllowableHoleSelectedItems { get { return _allowableHoleSelectedItems; } set { _allowableHoleSelectedItems = value; OnPropertyChanged("AllowableHoleSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<object> AllowableHoleItems { get { return _allowableHoleItems; } set { _allowableHoleItems = value; OnPropertyChanged("AllowableHoleItems"); } }
        [XmlIgnore]
        public int HoleSourceSelectedIndex { get { return _holeSourceSelectedIndex; } set { _holeSourceSelectedIndex = value; OnPropertyChanged("HoleSourceSelectedIndex"); } }
        [XmlIgnore]
        public int HoleSourceTempIndex { get { return _holeSourceTempIndex; } set { _holeSourceTempIndex = value; OnPropertyChanged("HoleSourceTempIndex"); } }
        [XmlIgnore]
        public object HoleSourceSelectedItem { get { return _holeSourceSelectedItem; } set { _holeSourceSelectedItem = value; OnPropertyChanged("HoleSourceSelectedItem"); } }

        [XmlIgnore]
        public ObservableCollection<object> CasingRefSelectedItems { get { return _casingRefSelectedItems; } set { _casingRefSelectedItems = value; OnPropertyChanged("CasingRefSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<Model.CasingsModel> CasingListSelectedItems { get { return _casingListSelectedItems; } set { _casingListSelectedItems = value; OnPropertyChanged("CasingListSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<object> AllowableCasingSelectedItems { get { return _allowableCasingSelectedItems; } set { _allowableCasingSelectedItems = value; OnPropertyChanged("AllowableCasingSelectedItems"); } }
        [XmlIgnore]
        public ObservableCollection<object> AllowableCasingItems { get { return _allowableCasingItems; } set { _allowableCasingItems = value; OnPropertyChanged("AllowableCasingItems"); } }
        [XmlIgnore]
        public int CasingSourceSelectedIndex { get { return _casingSourceSelectedIndex; } set { _casingSourceSelectedIndex = value; OnPropertyChanged("CasingSourceSelectedIndex"); } }
        [XmlIgnore]
        public int CasingSourceTempIndex { get { return _casingSourceTempIndex; } set { _casingSourceTempIndex = value; OnPropertyChanged("CasingSourceTempIndex"); } }
        [XmlIgnore]
        public object CasingSourceSelectedItem { get { return _casingSourceSelectedItem; } set { _casingSourceSelectedItem = value; OnPropertyChanged("CasingSourceSelectedItem"); } }
        
        [XmlIgnore]
        public bool CasingRefListButtonsShow { get { return _casingRefListButtonsShow; } set { _casingRefListButtonsShow = value; OnPropertyChanged("CasingRefListButtonsShow"); } }
        [XmlIgnore]
        public bool CasingListButtonsShow { get { return _casingListButtonsShow; } set { _casingListButtonsShow = value; OnPropertyChanged("CasingListButtonsShow"); } }
        [XmlIgnore]
        public bool CasingAllowableButtonShow { get { return _casingAllowableButtonShow; } set { _casingAllowableButtonShow = value; OnPropertyChanged("CasingAllowableButtonShow"); } }
        [XmlIgnore]
        public bool IsCasingSourceItemSelected { get { return _iscasingSourceItemSelected; } set { _iscasingSourceItemSelected = value; OnPropertyChanged("IsCasingSourceItemSelected"); } }

        

        [XmlIgnore]
        public bool HoleRefListButtonsShow { get { return _holeRefListButtonsShow; } set { _holeRefListButtonsShow = value; OnPropertyChanged("HoleRefListButtonsShow"); } }
        [XmlIgnore]
        public bool HoleListButtonsShow { get { return _holeListButtonsShow; } set { _holeListButtonsShow = value; OnPropertyChanged("HoleListButtonsShow"); } }
        [XmlIgnore]
        public bool HoleAllowableButtonShow { get { return _HoleAllowableButtonShow; } set { _HoleAllowableButtonShow = value; OnPropertyChanged("HoleAllowableButtonShow"); } }
        [XmlIgnore]
        public bool IsHoleSourceItemSelected { get { return _isHoleSourceItemSelected; } set { _isHoleSourceItemSelected = value; OnPropertyChanged("IsHoleSourceItemSelected"); } }

        //[XmlIgnore]
        //public string CasingBoxTitle { get { return _casingBoxTitle; } set { _casingBoxTitle = value; OnPropertyChanged("CasingBoxTitle"); } }
        //[XmlIgnore]
        //public string HoleBoxTitle { get { return _holeBoxTitle; } set { _holeBoxTitle = value; OnPropertyChanged("HoleBoxTitle"); } }

        [XmlIgnore]
        public int SaveCountor { get { return _saveCountor; } set { _saveCountor = value; OnPropertyChanged("SaveCountor"); } }




        public event PropertyChangedEventHandler PropertyChanged;

        #region CommandProperties
        public ICommand CreateWellConfig
        {
            get
            {
                _createWellConfig = new Command(FindConfig);
                return _createWellConfig;
            }
        }

        public ICommand RemoveFromCasingList
        {
            get
            {
                _removeFromCasingList = new Command(Remove_From_Casing_List);
                return _removeFromCasingList;
            }
        }


        public ICommand FilterCasingRefList
        {
            get
            {
                _filterCasingRefList = new Command(Filter_Casing_Ref_List);
                return _filterCasingRefList;
            }
        }
        public ICommand ShowCasingRefList
        {
            get
            {
                _showCasingRefList = new Command(Show_Casing_Ref_List);
                return _showCasingRefList;
            }
        }

        public ICommand SelectAllCasingList
        {
            get
            {
                _selectAllCasingList = new Command(Select_All_Casing_List);
                return _selectAllCasingList;
            }
        }

        public ICommand ClearAllCasingList
        {
            get
            {
                _clearAllCasingList = new Command(Clear_All_Casing_List);
                return _clearAllCasingList;
            }
        }

        public ICommand ReturnCasingAllowableToList
        {
            get
            {
                _returnCasingAllowableToList = new Command(Return_Casing_Allowable_To_List);
                return _returnCasingAllowableToList;
            }
        }
        public ICommand AddToHoleList
        {
            get
            {
                _addToHoleList = new Command(Add_To_Hole_List);
                return _addToHoleList;
            }
        }
        public ICommand RemoveFromHoleList
        {
            get
            {
                _removeFromHoleList = new Command(Remove_From_Hole_List);
                return _removeFromHoleList;
            }
        }

        public ICommand FilterHoleRefList
        {
            get
            {
                _filterHoleRefList = new Command(Filter_Hole_Ref_List);
                return _filterHoleRefList;
            }
        }
        public ICommand ShowHoleRefList
        {
            get
            {
                _showHoleRefList = new Command(Show_Hole_Ref_List);
                return _showHoleRefList;
            }
        }
        public ICommand SelectAllHoleList
        {
            get
            {
                _selectAllHoleList = new Command(Select_All_Hole_List);
                return _selectAllHoleList;
            }
        }
        public ICommand ClearAllHoleList
        {
            get
            {
                _clearAllHoleList = new Command(Clear_All_Hole_List);
                return _clearAllHoleList;
            }
        }
        public ICommand ReturnHoleAllowableToList
        {
            get
            {
                _returnHoleAllowableToList = new Command(Return_Hole_Allowable_To_List);
                return _returnHoleAllowableToList;
            }
        }
        public ICommand CasingSourceSelectionChanged
        {
            get
            {
                _casingSourceSelectionChanged = new Command(Casing_Source_Selection_Changed);
                return _casingSourceSelectionChanged;
            }
        }
        public ICommand ShowCasingAllowable
        {
            get
            {
                _showCasingAllowable = new Command(Show_Casing_Allowable);
                return _showCasingAllowable;
            }
        }
        public ICommand HoleSourceSelectionChanged
        {
            get
            {
                _holeSourceSelectionChanged = new Command(Hole_Source_Selection_Changed);
                return _holeSourceSelectionChanged;
            }
        }
        public ICommand ShowHoleAllowable
        {
            get
            {
                _showHoleAllowable = new Command(Show_Hole_Allowable);
                return _showHoleAllowable;
            }
        }
        public ICommand AddToCasingList
        {
            get
            {
                _addToCasingList = new Command(Add_To_Casing_List);
                return _addToCasingList;
            }
        }
        public ICommand ReturnHoleRefToList
        {
            get
            {
                _returnHoleRefToList = new Command(Return_Hole_Ref_To_List);
                return _returnHoleRefToList;
            }
        }


        public ICommand ReturnCasingRefToList
        {
            get
            {
                _returnCasingRefToList = new Command(Return_Casing_Ref_To_List);
                return _returnCasingRefToList;
            }
        }
        #endregion

        #endregion

        #region Fields

        private ICommand _addToCasingList;
        private ICommand _removeFromCasingList;
        private ICommand _filterCasingRefList;
        private ICommand _showCasingRefList;
        private ICommand _selectAllCasingList;
        private ICommand _clearAllCasingList;
        private ICommand _returnCasingAllowableToList;
        private ICommand _returnCasingRefToList;
        private ICommand _casingSourceSelectionChanged;
        private ICommand _showCasingAllowable;
        private ICommand _createWellConfig;


        private ICommand _addToHoleList;
        private ICommand _removeFromHoleList;
        private ICommand _filterHoleRefList;
        private ICommand _showHoleRefList;
        private ICommand _selectAllHoleList;
        private ICommand _clearAllHoleList;
        private ICommand _returnHoleAllowableToList;
        private ICommand _returnHoleRefToList;
        private ICommand _holeSourceSelectionChanged;
        private ICommand _showHoleAllowable;

        private ObservableCollection<List<string>> _finalWellConfig;

        private ObservableCollection<string> _holeInventoryList;
        private ObservableCollection<string> _casingInventoryList;

        private HolesDataCollection _holesData = new HolesDataCollection();
        private CasingsDataCollection _casingsData = new CasingsDataCollection();

        private List<string> _drillPipeODList = new List<string> {
            "6 5/8", "5 1/2", "5", "4 1/2",
            "4", "3 1/2", "2 7/8", "2 3/8" };

        private List<string> _BHAODList = new List<string> { "14","12",
            "11 1/4","11","10 3/4","10 1/2",
            "9 3/4","9 1/2","9 1/4","9",
            "8 3/4","8 1/2","8 1/4","8",
            "7 3/4","7 1/2","7 1/4","7",
            "6 3/4","6 1/2","6 1/4","6",
            "5 3/4","5 1/2","5 1/4","5",
            "4 3/4","4 1/2","4 1/4","4",
            "3 3/4","3 1/2","3 1/4","3",
            "2 7/8"};

        private Dictionary<string, double> _casingInventory = new Dictionary<string, double> { 
        { "2 3/8", 2.375 }, { "2 7/8", 2.875 }, { "3 1/2", 3.5 }, { "4", 4 },
        { "4 1/2", 4.5 }, { "5", 5 }, { "5 1/2", 5.5 }, { "6 5/8", 6.625 },
        { "7", 7 }, { "7 5/8", 7.625 }, { "7 3/4", 7.75 }, { "8 5/8", 8.625 },
        { "9 5/8", 9.625 }, { "10 3/4", 10.75 }, { "11 3/4", 11.75 }, { "11 7/8", 11.875 },
        { "13 3/8", 13.375 }, { "13 5/8", 13.625 }, { "14", 14 }, { "16", 16 }, 
        { "18 5/8", 18.625 }, { "20", 20 }, { "22", 22 }, { "24", 24 }, 
        { "26", 26 }, { "30", 30 }, { "32", 32 }, { "36", 36 } };

        private Dictionary<string, double> _holeInventory = new Dictionary<string, double> { { "3 3/4", 3.75 }, { "3 7/8", 3.875 }, { "4 1/8", 4.125 }, { "4 1/2", 4.5 }, { "4 3/4", 4.75 }, { "5 5/8", 5.625 }, { "6", 6 }, { "6 1/8", 6.125 }, { "6 1/4", 6.25 }, { "6 3/8", 6.375 }, { "6 1/2", 6.5 }, { "6 3/4", 6.75 }, { "7 1/2", 7.5 }, { "7 7/8", 7.875 }, { "8 3/8", 8.375 }, { "8 1/2", 8.5 }, { "8 5/8", 8.625 }, { "8 3/4", 8.75 }, { "9 1/2", 9.5 }, { "10 3/8", 10.375 }, { "10 5/8", 10.625 }, { "11", 11 }, { "11 5/8", 11.625 }, { "12", 12 }, { "12 1/4", 12.25 }, { "13 1/2", 13.5 }, { "13 3/4", 13.75 }, { "14 1/2", 14.5 }, { "14 3/4", 14.75 }, { "15", 15 }, { "15 1/2", 15.5 }, { "16", 16 }, { "17", 17 }, { "17 1/2", 17.5 }, { "18 1/2", 18.5 }, { "18 5/8", 18.625 }, { "20", 20 }, { "22", 22 }, { "23", 23 }, { "24", 24 }, { "26", 26 }, { "28", 28 }, { "36", 36 }, { "48", 48 } };

        private ObservableCollection<object> _casingRefSelectedItems = new ObservableCollection<object>();
        private ObservableCollection<Model.CasingsModel> _casingListSelectedItems = new ObservableCollection<Model.CasingsModel>();
        private ObservableCollection<object> _allowableCasingItems = new ObservableCollection<object>();
        private ObservableCollection<object> _allowableCasingSelectedItems = new ObservableCollection<object>();
        private int _casingSourceSelectedIndex = -1;
        private int _casingSourceTempIndex = new int();
        private object _casingSourceSelectedItem = new object();

        private HolesModel _holeParameterListSelectedItem;

        public HolesModel HoleParameterListSelectedItem
        {
            get
            {
                if (_holeParameterListSelectedItem == null) _holeParameterListSelectedItem = new HolesModel();
                return _holeParameterListSelectedItem;
            }
            set
            {
                _holeParameterListSelectedItem = value;
                OnPropertyChanged("HoleParameterListSelectedItem");
            }
        }

        private ObservableCollection<object> _holeRefSelectedItems = new ObservableCollection<object>();
        private ObservableCollection<Model.HolesModel> _holeListSelectedItems = new ObservableCollection<Model.HolesModel>();
        private ObservableCollection<object> _allowableHoleItems = new ObservableCollection<object>();
        private ObservableCollection<object> _allowableHoleSelectedItems = new ObservableCollection<object>();
        private int _holeSourceSelectedIndex;
        private int _holeSourceTempIndex;
        private object _holeSourceSelectedItem;

        private bool _casingRefListButtonsShow = true;
        private bool _casingListButtonsShow = false;
        private bool _casingAllowableButtonShow = false;
        private bool _iscasingSourceItemSelected = false;

        private bool _holeRefListButtonsShow = true;
        private bool _holeListButtonsShow = false;
        private bool _HoleAllowableButtonShow = false;
        private bool _isHoleSourceItemSelected = false;

        private bool _firstCasingInclude;
        private bool _lastCasingInclude;

        private bool _firstHoleInclude;
        private bool _lastHoleInclude;

        private bool _numberOfSizeInclude;

        //private string _casingBoxTitle = "Available CS Inventory";
        //private readonly string casingListTitle = "Casing Selected List";
        //private readonly string casingRefListTitle = "Casing Reference List";
        //private readonly string casingForAllowableTitle = "Casing Size Selection";
        //private readonly string casingChoiceTitle = "Allowable Casing Sizes";

        //private string _holeBoxTitle = "Available HS Inventory";
        //private readonly string holeListTitle = "Hole Selected List";
        //private readonly string holeRefListTitle = "Hole Reference List";
        //private readonly string holeForAllowableTitle = "Hole Size Selection";
        //private readonly string holeChoiceTitle = "Allowable Hole Sizes";

        private int _saveCountor = 0;

        //var sf = await Package.Current.InstalledLocation.GetFileAsync(@"somedirectory\mydata.xml");
        //var file = await sf.OpenAsync(FileAccessMode.Read);
        //Stream inStream = file.AsStreamForRead();
        //XElement myFirstXmlElement = XDocument.Load(inStream).Elements().First();
        #endregion

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));            
            }
            if (propertyName == "SaveCountor")
            {
                Messenger.Default.Send<WellViewModel, AppData>(this);
                Messenger.Default.Send<CasingsDataCollection, DataViewModel>(CasingsData);
                Messenger.Default.Send<HolesDataCollection, DataViewModel>(HolesData);
            }
        }
    }

    #region HoleDataCollection
    public class HolesDataCollection : ObservableCollection<Model.HolesModel>
    {
        public HolesDataCollection()
            : base()
        {
            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        void TrulyObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(a);
        }
    }
    #endregion

    #region CasingsDataCollection
    public class CasingsDataCollection : ObservableCollection<Model.CasingsModel>
    {
        public CasingsDataCollection()
            : base()
        {
            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        void TrulyObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(a);
        }
    }
    #endregion
}