using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// Nanmespace is a way of organizing code in C#, just like directories in a computer. This namespace
// contains everything related to exporting the recorded data to a csv-file and importing data from
// a csv-file.
namespace Model
{
    // A class for exporting data to a csv-file
    public class DataExporter
    {
        // Function for saving time and given values in a csv-file at the given filepath
        public static void SaveTimeAndValuesToCsv(IList<Double> times, IList<Double> values, String filePath)
        {
            // Throw an error if the number of values is not equal to the number of times 
            if (times.Count() != values.Count())
            {
                throw new Exception("Input lists not of equal length");
            }

            // Create a stringbuilder
            StringBuilder stringBuilder = new StringBuilder();

            // Read the data and convert to strings
            for(int i=0; i< values.Count(); i++)
            {
                String time = times[i].ToString(CultureInfo.InvariantCulture);
                String value = values[i].ToString(CultureInfo.InvariantCulture);
                String line = string.Format("{0},{1}", time, value);
                stringBuilder.AppendLine(line);
            }

            // Create the csv-file
            File.WriteAllText(filePath, stringBuilder.ToString());
        }

        // Function to load values from a given csv-file.
        public static void LoadTimeAndValuesFromCsv(IList<Double> times, IList<Double> values, String filePath)
        {
            times.Clear();
            values.Clear();
            String [] lines = File.ReadAllLines(filePath);
            foreach(String line in lines)
            {
                String[] entries = line.Split(',');
                Double time = 0;
                Double value = 0;
                Double.TryParse(entries[0], NumberStyles.Number, CultureInfo.InvariantCulture, out time);
                Double.TryParse(entries[1], NumberStyles.Number, CultureInfo.InvariantCulture, out value);

                times.Add(time);
                values.Add(value);
            }
        }
        public static void SaveTimeAnd5ValuesToCsv(IList<Double> times, IList<Double> datas1, IList<int> datas2, IList<Double> datas3, IList<Double> datas4, IList<Double> datas5, String filePath)
        {
            Console.Write(times.Count());
            Console.Write(datas1.Count());
            Console.Write(datas2.Count());
            Console.Write(datas3.Count());
            Console.Write(datas4.Count());
            Console.Write(datas5.Count());
            // Throw an error if the number of values is not equal to the number of times 
            if (times.Count() != datas1.Count())
            {

                throw new Exception("Input lists not of equal length");
            }

            // Create a stringbuilder
            StringBuilder stringBuilder = new StringBuilder();

            // Read the data and convert to strings
            for (int i = 0; i < times.Count(); i++)
            {
                String time = times[i].ToString(CultureInfo.InvariantCulture);
                String data1 = datas1[i].ToString(CultureInfo.InvariantCulture);
                String data2 = datas2[i].ToString(CultureInfo.InvariantCulture);
                String data3 = datas3[i].ToString(CultureInfo.InvariantCulture);
                String data4 = datas4[i].ToString(CultureInfo.InvariantCulture);
                String data5 = datas5[i].ToString(CultureInfo.InvariantCulture);


                String line = string.Format("{0},{1},{2},{3},{4},{5}", time, data1, data2, data3, data4, data5);
                stringBuilder.AppendLine(line);
            }

            // Create the csv-file
            File.WriteAllText(filePath, stringBuilder.ToString());
        }

    }
}
