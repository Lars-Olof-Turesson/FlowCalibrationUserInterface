// Importing stuff
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

// Nanmespace is a way of organizing code in C#, just like directories in a computer. This namespace
// contains everything related to running the motor, reading inputs and writing outputs.
namespace Model
{
    /// <summary>
    /// MotorControl - Controls the motor with a list of position and times or velocity and times.
    ///                Also reads the inputs.
    /// </summary>
     
    // A class for everything related to the MotorControl 
    public class MotorControl
    {
        // Instance of the class ModbusCommuniation 
        ModbusCommunication ModCom { get; set; }


        // Define lists to stime recorded values from the motor
        public List<Double> RecordedTimes { get; set; }          // List to store the timestamps from the motor
        public List<Double> RecordedPositions { get; set; }      // List to store the positions from motor
        public List<Double> RecordedVelocities { get; set; }     // List to store the velocities from the motor
        public List<Double> RecordedTorques { get; set; }        // List to store the torques from the motor
        public List<Double> RecordedPressures { get; set; }      // List to store the measured pressure (Analog input)
        public List<Double> RecordedLinearPositions { get; set; } // List to store the measured linear position (Analog input)

         // Creates a struct for the hardware
        struct Hardware
        {
            //DEFINE HARDWARE PARAMETERS
            public const Double Pitch = 32;                  // Circumference of gearwheel [mm]
            public const Double TicksPerRev = 4096;          // ticks per revolution (4096 positions on one revolution)
            public const Double VelocityResolution = 16;     // velocity resolution (position resolution / constant)
            public const Double TimePerSecond = 2000;        // time register (+2000 each second)
            public const Double MotorTorquePerTorque = 1000; // motor Torque [mNm] per Torque [Nm]
            public const Double PressureGain = 1;            // Analog in [VDC] to Pressure [?] gain
            public const Double PressureBias = 0;            // Analog in [VDC] to Pressure [?] bias
            public const Double LinearPosGain = 100.0/65420.0;   // Analog in [VDC] to linear position [mm] gain.
            public const Double LinearPosBias = 0;           // Analog in [VDc] to linear position [mm] bias.       
            public const Int16  MaxTorque = 200;             // Maximum allowed torque [mNm]
        }

        // Defines a struct for all registers in the motor
        public struct Register
        {
            // DEFINE REGISTERS related to motor drive, inputs and outputs
            public const ushort TargetInput     = 450;      // Regulator target
            public const ushort Position        = 200;      // Motor position           [ticks]
            public const ushort Speed           = 202;      // Motor speed              [positions/sec/16]
            public const ushort Torque          = 203;      // Motor torque             [nNm]
            public const ushort Time            = 420;      // Time                     [2000 counts/second]
            public const ushort Pressure        = 170;      // AnalogIn 1 (Pressure)    [Vdc]
            public const ushort LinearPosition  = 172;      // AnalogIn 2 (Linear pos.) [Vdc]
			public const ushort OutputControl3  = 152;      // Mode of digital output 3
			public const ushort Output3         = 162;      // Value of digital output 3
            public const ushort Acceleration    = 353;      // Acceleration value       [positions/second^2 / 256]
            public const ushort Deacceleration  = 354;      // Deacceleration value     [positions/second^2 / 256]
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
        struct Mode
        {
            public const Int16 PositionRamp = 21;   // PositionRamp(Mode 21) : Closed control of position with ramp control.
            public const Int16 SpeedRamp    = 33;   // SpeedRamp    (Mode 33): Speed control mode with ramp control.
            public const Int16 Shutdown     = 4;    // Shutdown     (Mode 4)
            public const Int16 MotorOff     = 0;    // Stop mode, motor is off 
            public const Int16 Beep         = 60;   // Motor produces sound att 500 Hz
        }

        //EventLogic,
        // The operands are used according to: "Trigger value = <Register> OPERATOR DataValue"
        // see p.20 in manual for more information about thie
        /* Note that only bits 0-3 of Register.EventControl is considered here.
        0           Always true
        1   =       Equal
        2   !=      Not equal
        3   <       Less than
        4   >       Greater than
        5   or      Bitwise or
        6   nor     Bitwise not or
        7   and     Bitwise and
        8   nand    Bitwise not and
        9   xor     Bitwise exclusive or
        10  nxor    Bitwise not exclusive or
        11  +       Add
        12  -       Subtract
        13  *       Multiply
        14  /       Divide
        15  Value   Takes data value directly
        */
        //

        // Function to create events. See p.19-21 in manual for more information about events
        public void CreateEvent(ushort EventNr,
                                Int16 TrgData,
                                Int16 TrgReg,
                                ushort EventLogic,
                                Int16 DstRegister,
                                ushort SrcData,
                                Int16 SrcRegister)
        {
            /* (ushort) EventNr,        Number between 0-19 
             * (Int16)  TrgData,        Value that is used for event triggers
             * (Int16)  TrgReg,         Register that is read and used to trigger event
             * (ushort) EventLogic,     0XA--B where A is how the event should behave
             *                          and B is how the event should be triggered
             * (Int16)  DstRegister,    Destination register
             * (ushort) SrcData,        What should be written to the destination register
             * (Int16)  SrcRegister,    Combined with SrcData
             */

            // Write to the registers to "implement/create" the events
            ModCom.RunModbus((ushort)(Register.EventTrgData + EventNr), TrgData);
            ModCom.RunModbus((ushort)(Register.EventTrgReg + EventNr), TrgReg);
            ModCom.RunModbus((ushort)(Register.EventControl + EventNr), EventLogic);
            ModCom.RunModbus((ushort)(Register.EventDstReg + EventNr), DstRegister);
            ModCom.RunModbus((ushort)(Register.EventSrcData + EventNr), SrcData);
            ModCom.RunModbus((ushort)(Register.EventSrcReg + EventNr),SrcRegister);
        }

        // Function to set a maximum torque to the motor and to create lists for
        // values to be read from motor controller.
        public MotorControl(ModbusCommunication modCom)
        {
            ModCom = modCom;

            // Create empty lists to store values that should be read from the motor controller
            RecordedTimes            = new List<Double>();
            RecordedPositions        = new List<Double>();
            RecordedVelocities       = new List<Double>();
            RecordedTorques          = new List<Double>();
            RecordedPressures        = new List<Double>();
            RecordedLinearPositions  = new List<Double>();

            // Create event reading the maximum torque status register
            CreateEvent((ushort)0,
                                   (Int16)(0B000000000100000),              // Bitmask to get torque from status register
                                   (Int16)(MotorControl.Register.Status),
                                   (ushort)0XF007,                          // Logical 'and' between bitmask and status register
                                   (Int16)(MotorControl.Register.Mode),
                                   (ushort)0,
                                   (Int16)0);                               // No source register
            
            // Set a maximum allowed torque
            ModCom.RunModbus(MotorControl.Register.MotorTorqueMax, Hardware.MaxTorque);

			// Make sure output register 1 is 0
			//ModCom.RunModbus(MotorControl.Register.Output3, (Int16) 0);

		}

        // Function to run the motor based on positions
        public void RunWithPosition(List<Double> positions, List<Double> times)
        {
            List<Int32> ticks = PositionToTick(positions);
            RunTickSequence(ticks, times, Mode.PositionRamp);
        }

        // Function to run the motor based on velocities
        public void RunWithVelocity(List<Double> velocities, List<Double> times)
        {
            List<Int32> ticks = VelocityToTicksPerSecond(velocities);
            RunTickSequence(ticks, times, Mode.SpeedRamp);
        }

        // Function to actually run the motor based on a list of ticks, i.e a list with angular positions
        // for the motor, a list of times and a given mode.
        public void RunTickSequence(List<Int32> ticks, List<Double> times, Int16 mode)
        {
            // If the number of ticks and number of times are not equal, throw an error
            if (ticks.Count() != times.Count())
            {
                throw new Exception("Input lists not of equal length");
            }

            // Create event reading the maximum torque status register
            CreateEvent((ushort)0,
                                   (Int16)(0B000000000100000),              // Bitmask to get torque from status register
                                   (Int16)(MotorControl.Register.Status),
                                   (ushort)0XF007,                          // Logical 'and' between bitmask and status register
                                   (Int16)(MotorControl.Register.Mode),
                                   (ushort)0,
                                   (Int16)0);                               // No source register

            // Set a maximum allowed torque
            ModCom.RunModbus(MotorControl.Register.MotorTorqueMax, Hardware.MaxTorque);

            // Define the sequence length
            int sequenceLength = ticks.Count();

            // Stopwatch is used to measure the time, creating an instance
            Stopwatch stopWatch = new Stopwatch();

            // Define lists to save recorded motor values in
            Int32 [] MotorRecordedTimes             = new int[sequenceLength];
            Int32 [] MotorRecordedPositions         = new Int32 [sequenceLength];
            Int32 [] MotorRecordedVelocities        = new Int32 [sequenceLength];
            Int32 [] MotorRecordedTorques           = new Int32 [sequenceLength];
            Int32 [] MotorRecordedPressures         = new Int32 [sequenceLength];
            Int32 [] MotorRecordedLinearPositions   = new Int32 [sequenceLength];
            Double [] StopwatchRecordedTimes        = new Double [sequenceLength];

            ModCom.RunModbus(Register.Mode, Mode.MotorOff);     // Turn off the motor
            ModCom.RunModbus(Register.Position, (Int32)0);      // Set the position to 0
            ModCom.RunModbus(Register.Speed, (Int32)0);         // Set the speed to 0
            ModCom.RunModbus(Register.TargetInput,(Int32)0);    // Set the motor to be continously controlled via communication bus

            // Set the mode of the motor to the given mode
            ModCom.RunModbus(Register.Mode, mode);
            // Set time = 0
            ModCom.RunModbus(Register.Time, (Int32)0);
            // Start to measure time
            stopWatch.Start();

            // Start to run the actual sequence
            int i = 0;
            while(i < sequenceLength)
            {   
                // If more time have elapsed than time[i]
                if (times[i] <= stopWatch.Elapsed.TotalSeconds)
                {
                    // Write a new target value to the motor
                    ModCom.RunModbus(Register.TargetInput,(Int32)ticks[i]);

                    // Read values that should be logged

                    // Read time
                    //MotorRecordedTimes[i] = ModCom.ReadModbus(Register.Time, 2, false);
                    // Read motor position
                    MotorRecordedPositions[i] = ModCom.ReadModbus(Register.Position, 2, true);
                    // Read motor velocity
                    //MotorRecordedVelocities[i] = ModCom.ReadModbus(Register.Speed, 1, false);
                    // Read time
                    StopwatchRecordedTimes[i] = stopWatch.Elapsed.TotalSeconds;
                    // Read motor torque
                    //MotorRecordedTorques[i] = ModCom.ReadModbus(Register.Torque, 1, false);
                    // Read pressure
                    //MotorRecordedPressures[i] = ModCom.ReadModbus(Register.Pressure, 1, false);
                    // Read linear position sensor
                    //MotorRecordedLinearPositions[i] = ModCom.ReadModbus(Register.LinearPosition, 1, false); 
                    i++;
                }
                
            }

            // Set target to zero
            ModCom.RunModbus(Register.TargetInput, (Int32)0);
            // Turn off the motor
            ModCom.RunModbus(Register.Mode, Mode.MotorOff);

            // Stop counting the time
            stopWatch.Stop();

            // Convert units below

            // Time to seconds
            //RecordedTimes = TimeToSeconds(MotorRecordedTimes);
            // Ticks to positions
            RecordedPositions = TickToPosition(MotorRecordedPositions);
            // Ticks per second to velocities
            RecordedVelocities = TicksPerSecondToVelocity(MotorRecordedVelocities);
            // Torque from [mNm] to [Nm]
            RecordedTorques = MotorTorquesToTorques(MotorRecordedTorques);
            // Pressure in [VDc] to [?]
            RecordedPressures = MotorPressureToPressure(MotorRecordedPressures);
            // Get time in seconds
            RecordedTimes = StopwatchRecordedTimes.ToList<Double>();
            // Linear position in [VDc] to [mm]
            RecordedLinearPositions = MotorVDcToLinPos(MotorRecordedLinearPositions);
        }

        // Function to convert from positions to ticks
        public List<int> PositionToTick(List<Double> positions)
        {
            List<int> ticks = new List<int>();
            for (int i = 0; i < positions.Count; i++)
            {
                ticks.Add(-(int)Math.Round(positions[i] * 10 * Hardware.TicksPerRev / Hardware.Pitch));
            }
            return ticks;
        }

        // Function to convert from Velocities to ticks per second
        public List<Int32> VelocityToTicksPerSecond(IList<Double> velocities)
        {
            List<Int32> ticks = new List<Int32>();
            for (int i = 0; i < velocities.Count; i++)
            {
                ticks.Add(-(int)Math.Round(velocities[i] * 10 * Hardware.TicksPerRev / Hardware.Pitch / Hardware.VelocityResolution));
            }
            return ticks;
        }

        // Function to convert from ticks to positions
        public List<Double> TickToPosition(IList<int> ticks)
        {
            List<Double> position = new List<Double>();
            for (int i = 0; i < ticks.Count; i++)
            {
                position.Add(-(Double)ticks[i] * Hardware.Pitch / Hardware.TicksPerRev /10);
            }
            return position;
        }

        // Function to convert from ticks per second to velocity
        public List<Double> TicksPerSecondToVelocity(IList<int> ticksPerSecond)
        {
            List<Double> velocity = new List<Double>();
            for (int i = 0; i < ticksPerSecond.Count; i++)
            {
                velocity.Add(-(Double)ticksPerSecond[i] * Hardware.Pitch * Hardware.VelocityResolution/Hardware.TicksPerRev/10);
            }
            return velocity;
        }

        // Function to convert from time to seconds
        public List<Double> TimeToSeconds(IList<int> time)
        {
            List<Double> seconds = new List<Double>();
            for (int i = 0; i < time.Count; i++)
            {
                seconds.Add( (Double)time[i] / Hardware.TimePerSecond);
            }
            return seconds;
        }

        // Function to convert from motortorques [nNm] to torques (Nm)
        public List<Double> MotorTorquesToTorques(IList<int> motorTorques)
        {
            List<Double> torques = new List<Double>();
            for (int i = 0; i < motorTorques.Count; i++)
            {
                torques.Add((Double)motorTorques[i] / Hardware.MotorTorquePerTorque);
            }
            return torques;
        }

        // Function to convert from Analogin 1 [VDc] to Pressure [?]
        public List<Double> MotorPressureToPressure(IList<int> motorPressure)
        {
            List<Double> pressure = new List<Double>();
            for (int i = 0; i < motorPressure.Count; i++)
            {
                pressure.Add((Double)motorPressure[i] * Hardware.PressureGain + Hardware.PressureBias);
            }
            return pressure;
        }

        // Function to convert from Analogin 3 [VDc] to Linear position [mm]
        public List<Double> MotorVDcToLinPos(IList<int> VDc)
        {
            List<Double> linearPos = new List<Double>();
            for (int i = 0; i < VDc.Count; i++)
            {
                linearPos.Add((Double)VDc[i] * Hardware.LinearPosGain + Hardware.LinearPosBias);
            }
            return linearPos;
        }

        // Function for initial test of linear sensor.
        public void testLinearSensor()
        {
            // Write to the console
            Console.WriteLine("Test of Linear sensor active!");
            Console.WriteLine("Press enter to exit");
            // Read current position from sensor
            //int Position = ModCom.ReadModbus(Register.LinearPosition, 1, false);
            //Console.WriteLine(Position);
            // As long as "Enter" is not pressed, continue 

            //while (Console.ReadKey().Key != ConsoleKey.Enter)
            while (true)
            {
                int Position = ModCom.ReadModbus(Register.LinearPosition, 1, false);
                double PositionMM = Position * Hardware.LinearPosGain;
                Console.WriteLine("Position: ");
                Console.WriteLine(PositionMM.ToString());
            }
            Console.WriteLine("Exiting manual control");
        }


        // Function to allow the user to manually control the motor, i.e the syring, from the console.
        public void ManualControl()
        {
            // Write to the console
            Console.WriteLine("Manual control active");
			Console.WriteLine("Press enter to exit, + to increase and - to decrease position");
            // Read current position from motor
            int CurrentPosition = ModCom.ReadModbus(Register.Position, 2, true); 
            // Update last position to current position
			int LastPosition = CurrentPosition;
            // Define variable to save keyboard input from the user
			ConsoleKeyInfo input = new ConsoleKeyInfo();

            // As long as "Enter" is not pressed, continue 
			while (Console.ReadKey().Key != ConsoleKey.Enter)
			{
                // Read the keyboard input
				input = Console.ReadKey(true);

                // In input is "+", increase currenposition with 50
				if (input.KeyChar == '+')
				{
					CurrentPosition = CurrentPosition + 50;
					Console.WriteLine("position increased");
				}

                // In input is "-", decrease currenposition with 50
                if (input.KeyChar == '-')
				{
					CurrentPosition = CurrentPosition - 50;
					Console.WriteLine("position decreased");
				}

                // If currenposition was updated, update position of motor
				if (CurrentPosition != LastPosition)
				{
					ModCom.RunModbus(Register.TargetInput, CurrentPosition);
					LastPosition = CurrentPosition;
				}

			}
			Console.WriteLine("Exiting manual control");
        }
    }
}
