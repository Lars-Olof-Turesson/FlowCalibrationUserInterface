# Calibration device for flow and volume software

This software is for controlling a developed calibration device which goal is to output a known flow of air using a precision syringe and a high-quality servomotor.
The software consists of a graphical user interface and the backend for controlling the servomotor. With the graphical user interface, it is possible to define flow profiles as periodic function with specified sampling time, frequency, amplitude and the number of periods.
It is also possible to create flow profiles in external programs and save them as CSV files and load them into the program.
When the flow profile is run, the program will record the position at each sampling instance and with that information it will calculate the actual output volume and flow and present it to the user.

## Installation
1. Download the file: FlowCalibrationSetup-v-1-1.zip.
2. Extract the contents of the zip file.
3. Run the setup application.

It is strongly recommended to also download the graphical user interface from Simplex Motion: http://simplexmotion.com/products/simplexmotiontool/. This is a safe way to test that the servo functions properly and that everything is correctly connected before running the FlowCalibration user interface.

## Usage
1. Set up the hardware properly: make sure the rig sits steady on the table, plug in the power to the servo, plug in the USB cable to the computer, make sure the safety relay has power.
2. Open the program FlowCalibration Tool.
3. Find the name of the serial port, for example by connecting with the servo through the Simplex Motion Tool.
4. Press the connect button.
5. Choose flow profile from the drop down list and set the amplitude, frequency, sampling interval and that the number of repetitions. (The values that the frequency, sampling interval, number of repetitions can take is limited as follows: the minimum frequency is 0.1 rad per second, the minimum sampling interval is 0.04 seconds and the maximum number of repetitions is 10). Keep in mind that the program doesn't account for the physical limitations of the rig, so don't start with a too large amplitude. 
6. Manually move the syringe to a suitable starting position. (The servo can be moved by hand at all times except for when a flow profile is run.)
7. Press the run button.
8. When the run is complete, the results are shown under the Logger tab.

_Flow profiles and results can be saved as CSV files (Comma Separated Values) with time in the 1st column and flow/volume in the 2nd column. (It is easy to import and export files like this in, for example, Matlab)_


## Getting started with developement

This section will go through the installation process if you want to contribute to the project. 

It is strongly recommended to also download the graphical user interface from Simplex Motion (http://simplexmotion.com/products/simplexmotiontool/). This is a safe way to test that the servo functions properly and that everything is correctly connected before running the FlowCalibration user interface.

### Windows
1. Download Visual Studio. Free version: https://www.visualstudio.com/vs/community/
2. Install Visual Studio. Choose the installation package .NET Desktop development for a minimal installation.
3. Install Git.
3. Clone this repository. Easiest way is by using the Team Explorer included in Visual Studio.
4. Open the solution: <Repository root>/FlowCalibration.sln
5. Right-click on FlowCalibration/FlowCalibration.csproj and choose: Set as startup item.
6. Run the project.

_An alternative to Cloning this project directly is to first Fork it and then Clone your own Fork of this repository. This is the recommended way._

### Mac and Linux
Development in Mac or Linux isn't fully supported. It is not possible to run the graphical user interface on any other operating system than Windows. But it is possible to run the Model project on Mac or a Linux. With this, it is possible to control the servo and test functions via the console. 
On Mac, the installation steps are similar to the ones on Windows, but the Model project should be set as startup item.
On Linux, Mono can be used to run the Model project but that installation will not be covered in this document.

## Project structure
The project is structured as follows:

### FlowCalibration
* ControlPage - Handles the view and layout of the frontend. Data is accesed through bindings in ControlPageViewModel. 
				Contains all event reactions triggered by the user.

* ControlPageViewModel - Holds the data that should be shown in the view. Calls profileGenerator. Uses functions from the Model. 

* ProfileGenerator - Contains functions to generate lists with DataPoints that can be plotted in the view.

Model features that needs to be reached:
* Initialize modbus communication - Create an instance of ModbusCommunication with port name as argument. 
* Initialize motor control - Create an instance of MotorControl with the ModbusCommunication object as argument.
* Run flow profile - Profile converter takes a list of flows and times and returns a list with positions and times. 
						Motor control takes the list with positions and times and do the sequence while recording actual
						Position, Velocity and Time and returns that.
						Profile converter converts the Position and Velocity back to flow and volume to be plotted in the view.

### Model
	
* ProfileConverter - Contains functions for converting between flow and volume and flow to position and velocity.

* MotorControl - Controls the motor with a list of position and times or velocity and times. 
					Can convert from position to ticks or velocity to ticks per second and the other way.

* ModbusCommunication - Wrapper for the NModBus4.Serial library. Used as an interface between MotorControl and the NModBus4 library.

* Program - Class with console test program for the Model.

## Deploying the project/creating MSI installer
1. Get the extension _Microsoft Visual Studio 2017 installer projects_ by going to Tools > Extensions and Upgrades… And find the package under the Online category.
2. Right-click on the project _SetupFlowCalibration_ and choose _Build_.
3. The setup.exe file and the Windows installer package will be in the Debug or Release folder under the SetupFlowCalibration project depending on your solution configuration.
4. Take these two files and put them in a new folder containing the project name and the new version number and zip the folder.

_For more information on how to set up a Windows installer project from scratch, see this link: https://youtu.be/HeDBYc3ybxc_

## ToDo
* Make the program find the correct COM-port automatically.
* Test the connection with the servo by sending commands and see that the response from the servo is correct.
* When the run button is pressed in the GUI, the function that is running the flow profile should run in a separate thread. Right now everything is run in the same thread which makes the GUI freeze while the flow profile is running.

## Troubleshooting

Missing references to oxyplot and NModbus4. _Check that the NuGet package manager got installed. Go to Tools>Get Tools and Features... Check the box under Individual Components > Code tools > NuGet package manager. When this is done, the dependences should be installed automatically._


## Authors
Fredrik Kjellberg

Gustav Lindberg

Fredrik Schyum

Johan Lund

Lars Brown
