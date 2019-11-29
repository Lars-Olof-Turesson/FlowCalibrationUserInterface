using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


// Nanmespace is a way of organizing code in C#, just like directories in a computer. This namespace
// contains the main program that runs everything
namespace Model
{
    class Program
    {
        static class Register
        {

            // DEFINE REGISTERS related to motor drive, inputs and outputs
            public const ushort TargetInput     = 450;      // Regulator target
            public const ushort Position        = 200;      // Motor position           [ticks]
            public const ushort Speed           = 202;      // Motor speed              [positions/sec/16]
            public const ushort Torque          = 203;      // Motor torque             [nNm]
            public const ushort Time            = 420;      // Time                     [2000 counts/second]
            public const ushort Pressure        = 170;      // AnalogIn 1 (Pressure)    [Vdc]
            public const ushort LinearPosition  = 171;      // AnalogIn 2 (Linear pos.) [Vdc]
            public const ushort Acceleration    = 353;      // Acceleration value       [positions/second^2 / 256]
            public const ushort Deacceleration  = 354;      // Deacceleration value     [positions/second^2 / 256]
            public const ushort PositionRamp    = 400;
            public const ushort SpeedRamp       = 400;
            public const ushort Shutdown        = 400;
            public const ushort Mode            = 400;      // Mode of motor drive
            public const ushort MotorTorqueMax  = 204;      // Torque limit value       [nNm]
            public const ushort Status          = 410;      // Current motor drive status (info in different bits)

            // DEFINE REGISTERS related to events. (see p.19 in manual for information)
            public const ushort EventControl    = 680;      // Control Register for events  (see p.11 in manual)
            public const ushort EventTrgReg     = 700;      // Event Target Register number (see p.12 in manual)
            public const ushort EventTrgData    = 720;      // Event Target Data            (see p.12 in manual)
            public const ushort EventSrcReg     = 740;      // Event Source Register number (see p.12 in manual)
            public const ushort EventSrcData    = 760;      // Event Source Data            (see p.12 in manual)
            public const ushort EventDstReg     = 780;      // Event Destination Register   (see p.12 in manual)
        }

        // Defines a struct for the motor modes
        static class Mode
        {
            public const Int16 PositionRamp = 21;   // PositionRamp(Mode 21) : Closed control of position with ramp control.
            public const Int16 SpeedRamp    = 33;   // SpeedRamp    (Mode 33): Speed control mode with ramp control.
            public const Int16 Shutdown     = 4;    // Shutdown     (Mode 4)
            public const Int16 MotorOff     = 0;    // Stop mode, motor is off 
            public const Int16 Beep         = 60;   // Motor produces sound att 500 Hz
        }

        // The main program that runs everything
        public static void Main(string[] args)
        {
            // Get the COM-port that the motor is connected to
            String portName = ModbusCommunication.GetSerialPortName();

            // Create an instance of the ModbusCommunication
            ModbusCommunication modCom = new ModbusCommunication(portName);
            
            // Reset all running data and then enter Off-mode for the motor 
            modCom.RunModbus(Register.Mode, (Int16)1);
            
            // Set the target to 0
            modCom.RunModbus(Register.TargetInput, 0);
            
            // Set the mode to position ramp (closed control of position with ramp)
            modCom.RunModbus(Register.Mode, (Int16)21);

            modCom.RunModbus(Register.TargetInput,4096);
            //modCom.RunModbus(Register.Mode,(Int16)33);
            //modCom.RunModbus(Register.TargetInput,100);
            //Thread.Sleep(2000);
            //modCom.RunModbus(Register.TargetInput,2000);
            //Thread.Sleep(2000);
            //modCom.RunModbus(Register.TargetInput,8000);
            //Thread.Sleep(2000);
            //modCom.RunModbus(Register.TargetInput,30000);
            //Thread.Sleep(2000);
            //modCom.RunModbus(Register.TargetInput,2000);
            //Thread.Sleep(2000);
            //modCom.RunModbus(Register.TargetInput,0);
            //modCom.RunModbus(Register.Mode,(Int16)1);

            // Create an instance o´f MotorControl
            MotorControl motCon = new MotorControl(modCom);

            // Test of event safety function
            int currentTorque = modCom.ReadModbus(Register.Torque, (ushort)1, false);
            Console.WriteLine("current Torque:");
            Console.WriteLine(currentTorque);

            // Create event that gets the torque
            motCon.CreateEvent((ushort)0,
                                (Int16)(0B000000000100000), // Bitmask to get torque from status register
                                (Int16)(Register.Status),
                                (ushort)0XF007,             // AND between bitmask and status register
                                (Int16)(Register.Mode),
                                (ushort)0,
                                (Int16)0);                  // No source register

            // Set the max torque to 100 nNm
            modCom.RunModbus(Register.MotorTorqueMax, (Int16)100);

            // Variable for dummie read
            int dummieRead;

            // Lists for recorded values
            Double[] RecordedTimes1 = new Double[100];
            Double[] RecordedTimes2 = new Double[100];
            Double[] RecordedTimesRead = new Double[100];
            Double[] RecordedTimesWrite = new Double[100];

            // Start to measure time
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // The two loops below checks how long time it takes to read from the motor controller
            for (int i = 0; i < 99; i++)
            {
                RecordedTimes1[i] = stopWatch.Elapsed.TotalSeconds;
                dummieRead = modCom.ReadModbus(Register.Position, 2, true);
                RecordedTimes2[i] = stopWatch.Elapsed.TotalSeconds;
            }
            for (int i = 0; i < 100; i++)
            {
                RecordedTimesRead[i] = RecordedTimes2[i] - RecordedTimes1[i];
            }

            // The two loops below checks how long time it takes to write from the motor controller
            for (int i = 0; i < 100; i++)
            {
                RecordedTimes1[i] = stopWatch.Elapsed.TotalSeconds;
                modCom.RunModbus((ushort)450, (int)0);
                RecordedTimes2[i] = stopWatch.Elapsed.TotalSeconds;
            }
            for (int i = 0; i < 100; i++)
            {
                RecordedTimesWrite[i] = RecordedTimes2[i] - RecordedTimes1[i];
            }

            // Write the recorded read and write times to the console 
            //Console.WriteLine("Read: Max: {0}, Min: {1}, Avr: {2}", RecordedTimesRead.Max(), RecordedTimesRead.Min(), RecordedTimesRead.Average());
            //Console.WriteLine("Write: Max: {0}, Min: {1}, Avr: {2}", RecordedTimesWrite.Max(), RecordedTimesWrite.Min(), RecordedTimesWrite.Average());

            // Define a test sequence
            //List<Int32> ticks = new List<Int32>() { 0, 100, 1000, 2000, 3000, 2000, 1000, 100, 0 };
            //List<Int32> ticks = new List<Int32>() {0,2000,4000,8000,4000,500,-2000,-2000,0};
            //List<double> times = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

            // Run the test sequence
            //motCon.RunTickSequence(ticks, times, Mode.PositionRamp);
            //motCon.ManualControl();
            //Console.ReadLine();

            double currentTime = stopWatch.Elapsed.TotalSeconds;
            double lastTime = stopWatch.Elapsed.TotalSeconds;

            //motCon.testLinearSensor();

            modCom.EndModbus();

            Console.ReadLine();

        }
    }
}
