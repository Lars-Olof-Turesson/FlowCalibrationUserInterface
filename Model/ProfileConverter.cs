using System;
using System.Collections.Generic;
using System.Linq;

// Nanmespace is a way of organizing code in C#, just like directories in a computer. This namespace
// contains everything related to converting between volume/flow and position/speed.
namespace Model
{
    public class ProfileConverter
	// ProfileConverter - Contains functions for converting between flow and volume and flow to position and velocity.
	//					  Needs parameters related to the mechanical construction, syringe diameter                    
    // Units
    // Position [mm]
    // Velocity [mm/s]
    // Flow     [ml/s]
    // Volume   [ml]
    {
        public double SyringeDiameter { get; set; }
        public double SectionArea { get; set; }

        public ProfileConverter()
        {
            SyringeDiameter = 34; // [mm]
            SectionArea = Math.Pow((SyringeDiameter / 2), 2) * Math.PI; // [mm^2]
        }

        // Function to get flow [ml/s] from positons [mm]
        public List<double> FlowToPosition(List<double> times, List<double> flows)
        {
            List<double> positions = new List<double>();
            List<double> volume = Integrate(times,flows);

            positions.Add((volume[0] * 1000 / SectionArea));
            for (int i = 1; i <= volume.Count()-1; i++){
                positions.Add((volume[i] * 1000 / SectionArea)+positions[i-1]);
            }
            return positions;
        }

        // Function to get flow [ml/s] from velocities [mm/s]
        public List<double> FlowToVelocity(List<double> flows)
        {
            List<double> velocity = new List<double>();
            for (int i = 0; i < flows.Count(); i++){
                velocity.Add(flows[i] * 1000 /SectionArea);
            }
            return velocity;
        }

        // Function to get volume [ml] from positions [mm]
        public List<double> PositionToVolume(List<double> positions)
        {
            List<double> volumes = new List<double>();
            for (int i = 0; i < positions.Count(); i++){
                volumes.Add(positions[i]*SectionArea / 1000);
            }
            return volumes;
        }

        // Function to get flow [ml/s] from positions [mm]
        public List<Double> PositionToFlow(List<Double> positions, List<Double> times)
        {
            List<double> flows = new List<double>();
            for (int i = 1; i < positions.Count(); i++)
            {
                Double deltaPosition = positions[i] - positions[i - 1];
                Double deltaTime = times[i] - times[i - 1];
                flows.Add((deltaPosition / deltaTime) * SectionArea / 1000);
            }

            return flows;
        }

        // Function to get flow [ml] from velocitiies [mm/s]
        public List<double> VelocityToFlow(List<double> velocities)
        {
            List<double> flows = new List<double>();
            for (int i = 0; i < velocities.Count(); i++){
                flows.Add(velocities[i]*SectionArea / 1000);
            }
            return flows;
        }


        // Function to perform integration on given profile/graph.
        // The first value is lost.
        // How big is the error on the integration????????????????
        static List<double> Integrate(List<double> x, List<double> y)
        {
            // Throw an error if the given lists are not of equl length
            if (x.Count() != y.Count())
            {
                throw new Exception("Input lists not of equal length");
            }
            
            List<double> primitive = new List<double>();
            for (int i = 0; i <= x.Count() - 1 - 1; i++)
            {
                double areaForward = y[i] * (x[i + 1] - x[i]);
                //double areaBackward = y[i + 1] * (x[i + 1] - x[i]); 
                primitive.Add(areaForward);
            }
            return primitive;
        }
    }
}
