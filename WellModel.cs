using CasingDesign.Common;
using CasingDesign.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;        
  

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



