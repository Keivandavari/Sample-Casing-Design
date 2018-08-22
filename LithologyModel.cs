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

namespace CasingDesign.Model
{
    [XmlType("DepthData")]
    public class LithologyModel : INotifyPropertyChanged
    {
        private const string PageName = "Lithology";
        private int _layerTypeSelectedIndex;
        private string _layerType;
        private string _compotentLayer;
        private int _compotentLayerSelectedIndex;
        private string _layerName;
        private double _layerTopTVD;
        private double _overbalancedMargin;
        private double _diffStickingLimit;
        private double _stabilityMinMW;
        private double _permeability;
        private double _porosity;
        private double _rOP;
        private double _porePressure;
        private double _fractureGradient;
        private double _tripMargin;
        private double _kickMargin;
        //private double _casingDepths;
        private ObservableCollection<string> _layerTypeList = new ObservableCollection<string>() { "Basalt", "Clay", "Claystone", "Coal, Antracite","Coal, Bituminous"
        ,"Coal, Lignite" , "Conglomerate", "Dolomite", "Evaporites, Anhydrite", "Evaporites, Gypsum","Evaporites, Halite",
        "Granite","Gnesis","Limestone, Argillaceous","Limestione, Fossiliferous","Limestone, Porous", "Limestione, Sparitic", 
        "Sand", "Sandstone, Coarse","Sandstone, Fine", "Sandstone,Fossiliderous", "Sandstone, Medium", "Sandstone, Shaly", 
        "Shale, Calcarous", "Shale, Dolomite","Shale, Sandy","Shale, Siliceous", "Shale, Silty", "Silt","Siltstone","Tuff" };
        [XmlIgnore]
        private List<string> _compotentLayerList = new List<string>() { "Yes", "No" };
        [XmlIgnore]
        public double KickMargin = .5;
        
        [XmlIgnore]
        public List<string> CompotentLayerList
        {
            get { return _compotentLayerList; }
        }
        public string CompotentLayer
        {
            get
            {
                _compotentLayer = CompotentLayerList[CompotentLayerSelectedIndex];
                return _compotentLayer;
            }
            set
            {
                _compotentLayer = value;
                OnPropertyChanged("CompotentLayer");
            }
        }
        [XmlIgnore]
        public int CompotentLayerSelectedIndex
        {
            get { return _compotentLayerSelectedIndex; }
            set
            {
                if (value != -1)
                {
                    _compotentLayerSelectedIndex = value;
                    //SetCompotentLayer();
                    OnPropertyChanged("CompotentLayerSelectedIndex");
                    OnPropertyChanged("CompotentLayer");
                }
            }
        }
        [XmlIgnore]
        public ObservableCollection<string> LayerTypeList
        {
            get { return _layerTypeList; }
        }
        public string LayerType
        {
            get 
            {
                _layerType = LayerTypeList[LayerTypeSelectedIndex];
                return _layerType;
            }
            set
            {
                _layerType= value;
                OnPropertyChanged("LayerType");
            }
        }
        [XmlIgnore]
        public int LayerTypeSelectedIndex
        {
            get { return _layerTypeSelectedIndex; }
            set
            {
                if (value != -1)
                {
                    _layerTypeSelectedIndex = value;
                    //SetLayetType();
                    OnPropertyChanged("LayerTypeSelectedIndex");
                    OnPropertyChanged("LayerType");
                }
            }
        }
        //private void SetLayetType()
        //{
        //    if (LayerTypeSelectedIndex == -1)
        //        return;
        //    else
        //        LayerType = LayerTypeList[LayerTypeSelectedIndex];
        //}
        public string LayerName
        {
            get { return _layerName; }
            set
            {
                _layerName = value;
                OnPropertyChanged("LayerName");
            }
        }
        public double LayerTopTVD
        {
            get { return _layerTopTVD; }
            set
            {
                if (value >= 0 && _layerTopTVD != value)
                {
                    _layerTopTVD = value;
                    OnPropertyChanged("LayerTopTVD");
                }
            }
        }
        public double OverbalancedMargin
        {
            get { return _overbalancedMargin; }
            set
            {
                if (value >= 0 && _overbalancedMargin != value)
                {
                    _overbalancedMargin = value;
                    OnPropertyChanged("OverbalancedMargin");
                }
            }
        }
        public double DiffStickingLimit
        {
            get { return _diffStickingLimit; }
            set
            {
                if (value >= 0 && _diffStickingLimit != value)
                {
                    _diffStickingLimit = value;
                    OnPropertyChanged("DiffStickingLimit");
                }
            }
        }
        public double StabilityMinMW
        {
            get { return _stabilityMinMW; }
            set
            {
                if (value >= 0 && _stabilityMinMW != value)
                {
                    _stabilityMinMW = value;
                    OnPropertyChanged("StabilityMinMW");
                }
            }
        }
        public double Permeability
        {
            get { return _permeability; }
            set
            {
                if (value >= 0 && _permeability != value)
                {
                    _permeability = value;
                    OnPropertyChanged("Permeability");
                }
            }
        }
        public double Porosity
        {
            get { return _porosity; }
            set
            {
                if (value >= 0 && _porosity != value)
                {
                    _porosity = value;
                    OnPropertyChanged("Porosity");
                }
            }
        }

        public double ROP
        {
            get { return _rOP; }
            set
            {
                if (value >= 0 && _rOP != value)
                {
                    _rOP = value;
                    OnPropertyChanged("ROP");
                }
            }
        }
        public double PorePressure
        {
            get { return _porePressure; }
            set
            {
                if (value >= 0 && _porePressure != value)
                {
                    _porePressure = value;
                    OnPropertyChanged("PorePressure");
                }
            }
        }
        public double FractureGradient
        {
            get { return _fractureGradient; }
            set
            {
                if (value >= 0 && _fractureGradient != value)
                {
                    _fractureGradient = value;
                    OnPropertyChanged("FractureGradient");
                }
            }
        }
        public double TripMargin
        {
            get
            {
                if (PorePressure + OverbalancedMargin < StabilityMinMW)
                    _tripMargin = StabilityMinMW;
                else
                    _tripMargin = PorePressure + OverbalancedMargin;
                return _tripMargin;
            }
            set { _tripMargin = value; OnPropertyChanged("TripMargin"); }
        }
        public double KickMarginData
        {
            get
            {
                if ((FractureGradient - KickMargin - PorePressure) * 0.052 * LayerTopTVD > DiffStickingLimit)
                    _kickMargin = PorePressure + (DiffStickingLimit / (.052 * LayerTopTVD));
                else
                    _kickMargin = FractureGradient - KickMargin;
                return _kickMargin;
            }
            set { _kickMargin = value; OnPropertyChanged("KickMarginData"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            Messenger.Default.Send<string, LithologyViewModel>("Change");
        }
    }
    
    public class CasingDepth : INotifyPropertyChanged
    {
        private double _casingDepths;
        public double CasingDepths
        {
            get { return _casingDepths; }
            set { _casingDepths = value; }
        }
        private double _casingPpg;
        public double CasingPpg
        {
            get { return _casingPpg; }
            set { _casingPpg = value; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            Messenger.Default.Send<string, LithologyViewModel>("Change");
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            
        }
    }
}
