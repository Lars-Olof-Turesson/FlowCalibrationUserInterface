using OxyPlot;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowCalibration
{
    class ControlPageViewModel : INotifyPropertyChanged
    {
        #region Variable declarations

        ModbusCommunication modCom;
        MotorControl motorControl;
        ProfileConverter ProfileConverter;

        public String recordedProfile;
        public String recordedDateTime;
        public Double recordedMaxTime;
        public Double recordedMinFlow;
        public Double recordedMaxFlow;
        public Double recordedMinVolume;
        public Double recordedMaxVolume;
        private Boolean usbConnected;

        #endregion

        #region Property declarations

        public ObservableCollection<DataPoint> LogFlowPoints { get; private set; }

        public ObservableCollection<DataPoint> LogVolumePoints { get; private set; }

        public List<Double> logTime;
        public List<int>    logRecordedSensor;
        public List<Double> logRecordedLinearPositions;
        public List<Double> logRecordedPressure;
        public List<Double> logRecordedVelocity;
        public List<Double> logRecordedFlow;
        public List<Double> logRecordedVolume;
        public List<Double> logRecordedTarget;
        public List<Double> logRecordedTargetVelocity;
        public List<Double> integrals;
        public List<Double> intTimes;



        public ObservableCollection<DataPoint> IdealIntegral { get; private set; }
        public ObservableCollection<DataPoint> LogLinearPoints { get; private set; }
        public ObservableCollection<DataPoint> LogSensorPoints { get; private set; }
        public ObservableCollection<DataPoint> LogTargetPoints { get; private set; }
        public ObservableCollection<DataPoint> LogVelocityPoints { get; private set; }
        public ObservableCollection<DataPoint> LogPressurePoints { get; private set; }


        public ObservableCollection<DataPoint> ControlFlowPoints { get; private set; }
        public ObservableCollection<PointTracker> TrackedFlowPoints { get; private set; }

        public ObservableCollection<String> FlowProfileNames { get; private set; }

        public String CurrentProfileName { get; set; }

        public Double Amplitude { get; set; }
        public Double Frequency { get; set; }
        public Double SamplingInterval { get; set; }
        public Double Repeat { get; set; }
        
        public String RecordedProfile
        {
            get { return recordedProfile; }
            set { if (value != recordedProfile) {
                    recordedProfile = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public String RecordedDateTime
        {
            get { return recordedDateTime; }
            set { if (value != recordedDateTime) {
                    recordedDateTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Double RecordedMaxTime
        {
            get { return recordedMaxTime; }
            set { if (value != recordedMaxTime) {
                    recordedMaxTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Double RecordedMinFlow
        {
            get { return recordedMinFlow; }
            set { if (value != recordedMinFlow) {
                    recordedMinFlow = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Double RecordedMaxFlow
        {
            get { return recordedMaxFlow; }
            set { if (value != recordedMaxFlow) {
                    recordedMaxFlow = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Double RecordedMinVolume
        {
            get { return recordedMinVolume; }
            set { if (value != recordedMinVolume) {
                    recordedMinVolume = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Double RecordedMaxVolume
        {
            get { return recordedMaxVolume; }
            set { if (value != recordedMaxVolume) {
                    recordedMaxVolume = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Boolean USBConnected
        {
            get { return usbConnected; }
            set { if (value != usbConnected) {
                    usbConnected = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string PortName { get; set; }
  
        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Function definitions

        public ControlPageViewModel()
        {
            FlowProfileNames = new ObservableCollection<string>
            {
                "Sine", "Square", "Triangle", "Peaks", "Custom"
            };

            TrackedFlowPoints = new ObservableCollection<PointTracker>();
            ControlFlowPoints = new ObservableCollection<DataPoint>();
            IdealIntegral = new ObservableCollection<DataPoint>();

            LogFlowPoints = new ObservableCollection<DataPoint>();
            LogVolumePoints = new ObservableCollection<DataPoint>();
            //Recorded Collections
            LogLinearPoints = new ObservableCollection<DataPoint>();
            LogSensorPoints = new ObservableCollection<DataPoint>();
            LogTargetPoints = new ObservableCollection<DataPoint>();
            LogVelocityPoints = new ObservableCollection<DataPoint>();
            LogPressurePoints = new ObservableCollection<DataPoint>(); 


            Amplitude = 20;
            Frequency = 3;
            SamplingInterval = 0.04;
            Repeat = 1;

            RecordedProfile = "";
            RecordedDateTime = "";
            RecordedMaxTime = 0;
            RecordedMinFlow = 0;
            RecordedMaxFlow = 0;
            RecordedMinVolume = 0;
            RecordedMaxVolume = 0;

            ProfileConverter = new ProfileConverter();

            USBConnected = false;
            PortName = "COM5";

        }

        public void UpdateProfile()
        {
            List<DataPoint> points = ProfileGenerator.GetPeriodic(CurrentProfileName, Amplitude, Frequency, SamplingInterval, Repeat);
            //UpdateObservableCollectionFromIList(ControlFlowPoints, points);
            UpdateFlowProfileFromIList(points);
            //UpdateObservableCollectionFromLists(IdealIntegral, intTimes, integrals);
        }

        private void UpdateObservableCollectionFromIList(ObservableCollection<DataPoint> observablePoints, IList<DataPoint> pointList)
        {
            observablePoints.Clear();
            foreach(DataPoint point in pointList)
            {
                observablePoints.Add(point);
            }
        }

        private void UpdateObservableCollectionFromLists(ObservableCollection<DataPoint> observablePoints, List<double> times, List<double> values)
        {
            observablePoints.Clear();
            for(int i=0; i<values.Count(); i++)
            {
                observablePoints.Add(new DataPoint(times[i], values[i]));
            }
        }

        private void UpdateObservableCollectionFromLists(ObservableCollection<DataPoint> observablePoints, List<double> times, List<int> values)
        {
            observablePoints.Clear();
            for (int i = 0; i < values.Count(); i++)
            {
                observablePoints.Add(new DataPoint(times[i], values[i]));
            }
        }

        private void UpdateFlowProfileFromIList(IList<DataPoint> pointList)
        {
            ControlFlowPoints.Clear();
            TrackedFlowPoints.Clear();
            int i = 0;
            foreach(DataPoint point in pointList)
            {
                ControlFlowPoints.Add(point);
                TrackedFlowPoints.Add(new PointTracker(point.X, point.Y, i, ControlFlowPoints));
                i++;
            }
        }

        private void UpdateFlowProfileFromLists(List<Double> times, List<Double> values)
        {
            ControlFlowPoints.Clear();
            TrackedFlowPoints.Clear();
            for(int i=0; i<values.Count(); i++)
            {
                DataPoint point = new DataPoint(times[i], values[i]);
                ControlFlowPoints.Add(point);
                TrackedFlowPoints.Add(new PointTracker(point.X, point.Y, i, ControlFlowPoints));
                integrals = new List<Double>();
                integrals = ProfileConverter.SimpleIntegrate(values, SamplingInterval);
                intTimes = times;

            }
        }

        public void RunFlowProfile()
        {
            if (!USBConnected) return;

            List<Double> times = new List<Double>();
            List<Double> values = new List<Double>();
          
            foreach (DataPoint point in ControlFlowPoints)
            {
                times.Add(point.X);
                values.Add(point.Y);
            }
           
            values = ProfileConverter.FlowToVelocity(values);
            //values = ProfileConverter.FlowToPosition(times, values);
            // Run sequence on motor
            try
            {
                motorControl.RunWithVelocity(values, times);
                //motorControl.RunWithPosition(values, times);
            }
            catch(Exception) // Not the best practice to catch all possible exceptions, but it works for now.
            {
                USBConnected = false;
                return;
            }
            
            // Old recordings that are not used
            //List<Double> recordedFlows = ProfileConverter.PositionToFlow(motorControl.RecordedPositions, motorControl.RecordedTimes);
            //List<Double> recordedVolumes = ProfileConverter.PositionToVolume(motorControl.RecordedPositions);
            
            //List<Double> recordedTimes = motorControl.RecordedTimes;

            logTime                    = motorControl.LoggedTime;
            logRecordedLinearPositions = motorControl.LoggedLinearPositions;
            logRecordedSensor          = motorControl.LoggedSensorValues;
            logRecordedPressure        = motorControl.LoggedPressures;
            logRecordedVelocity        = motorControl.LoggedVelocities;
            logRecordedTarget          = motorControl.LoggedTargets;
            logRecordedTarget          = ProfileConverter.VelocityToFlow(logRecordedTarget);
            logRecordedVolume          = ProfileConverter.PositionToVolume(logRecordedLinearPositions);
            logRecordedFlow            = ProfileConverter.PositionToFlow(logRecordedLinearPositions, logTime); 

            UpdateObservableCollectionFromLists(LogFlowPoints,      logTime, logRecordedFlow);
            UpdateObservableCollectionFromLists(LogVolumePoints,    logTime, logRecordedVolume);
            UpdateObservableCollectionFromLists(LogLinearPoints,    logTime, logRecordedLinearPositions);
            UpdateObservableCollectionFromLists(LogSensorPoints,    logTime, logRecordedSensor);
            UpdateObservableCollectionFromLists(LogPressurePoints,  logTime, logRecordedPressure);
            UpdateObservableCollectionFromLists(LogVelocityPoints,  logTime, logRecordedVelocity);
            UpdateObservableCollectionFromLists(LogTargetPoints,    logTime, logRecordedTarget);
            

       

            RecordedProfile = CurrentProfileName;
            RecordedDateTime = DateTime.Now.ToString();
            RecordedMaxTime = logTime.Last();
            RecordedMaxFlow = logRecordedFlow.Max();
            RecordedMinFlow = logRecordedFlow.Min();
            RecordedMaxVolume = logRecordedVolume.Max();
            RecordedMinVolume = logRecordedVolume.Min();
        }


        public void InitializeMotor(String portName)
        {
            modCom = new ModbusCommunication(portName);
            motorControl = new MotorControl(modCom);
            USBConnected = true;
        }

        public void SaveProfile(String filePath, IList<DataPoint> dataPoints)
        {
            List<Double> times = new List<Double>();
            List<Double> values = new List<Double>();

            foreach (DataPoint point in dataPoints)
            {
                times.Add(point.X);
                values.Add(point.Y);
            }
            DataExporter.SaveTimeAndValuesToCsv(times, values, filePath);
        }

        public void LoadProfile(String filePath)
        {
            List<Double> times = new List<Double>();
            List<Double> values = new List<Double>();

            DataExporter.LoadTimeAndValuesFromCsv(times, values, filePath);

            UpdateFlowProfileFromLists(times, values);
        }
    }
    #endregion


    /// <summary>
    /// The function of this class is to track an Oxyplot Datapoint contained in an ObservableCollection.
    /// This is necessary because the X and Y value of an Oxyplot data point is read-only. 
    /// What this allows us to achieve is to be able to dynamically update the X or Y value of a 
    /// data point from the data grid in the program.
    /// </summary>
    public class PointTracker
    {
        double y;
        double x;

        public Double X
        {
            get { return x; }
            set
            {
                x = value;
                TrackedCollection[Index] = new DataPoint(x, y);
            }
        }
        public Double Y {
            get { return y; }
            set
            {
                y = value;
                TrackedCollection[Index] = new DataPoint(x, y);
            }
        }
        public int Index { get; set; }

        public ObservableCollection<DataPoint> TrackedCollection { get; set; }

        public PointTracker(Double x, Double y, int i, ObservableCollection<DataPoint> t)
        {
            this.x = x;
            this.y = y;
            Index = i;
            TrackedCollection = t;
        }
    }

}
