using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileLoadandInsert
{
    

    public partial class MainWindow : Window
    {
       // private object ComtradeID;

        public MainWindow()
        {
            InitializeComponent();
        }
        // Function to determine if the file is Proedison (replace with actual logic)
        //private bool DetermineIsProedison(string filePath)
        //{
        //    return filePath.Contains("Proedison"); // Example logic
        //}

        //// Function to check if the file has HDR (replace with actual logic)
        //private bool CheckForHDR(string filePath)
        //{
        //    return File.ReadAllLines(filePath).Any(line => line.Contains("HDR")); // Example logic
        //}

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Open file from local disk
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CFG Files (*.cfg)|*.cfg"; // Filter to allow only .cfg files
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {

            // Get file path and name
            string filePath = FilePathTextBox.Text;  
            string fileName = System.IO.Path.GetFileName(filePath);
            //int ErrorFlag = 0; // Initialize error flag as 0 (no error)


            // Validate the selected file
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Please select a valid file.");
                return;
            }

            try
            {
                // Connection string to connect to the database
                string connectionString = "Data Source=DHEERAJLENOVO83;Initial Catalog=ProWave;Integrated Security=True;TrustServerCertificate=True;";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        //int ComtradeID = 1; // Store the generated ComtradeID

                        try
                        {



                        //     // Allow explicit IDs to be inserted
                        //    string setIdentityInsertQuery = "SET IDENTITY_INSERT COMTRADE ON;";
                        //    using (SqlCommand setIdentityCmd = new SqlCommand(setIdentityInsertQuery, con, transaction))
                        //    {
                        //        setIdentityCmd.ExecuteNonQuery();
                        //    }

                        //    // Insert file info and specify ComtradeID
                        //    string insertFileInfoQuery = @"
                        //    INSERT INTO COMTRADE (ComtradeID, FileName, FilePath, Error, IsProedison, HasHDR)
                        //    VALUES (@ComtradeID, @FileName, @FilePath, @Error, @IsProedison, @HasHDR)";

                        //    using (SqlCommand fileInfoCmd = new SqlCommand(insertFileInfoQuery, con, transaction))
                        //    {
                        //        // Logic for IsProedison and HasHDR
                        //        bool isProedison = DetermineIsProedison(filePath);
                        //        bool hasHDR = CheckForHDR(filePath);

                        //        // Add parameters to the insert query
                        //        fileInfoCmd.Parameters.AddWithValue("@ComtradeID", ComtradeID);
                        //        fileInfoCmd.Parameters.AddWithValue("@FileName", fileName);
                        //        fileInfoCmd.Parameters.AddWithValue("@FilePath", filePath);
                        //        fileInfoCmd.Parameters.AddWithValue("@Error", ErrorFlag);
                        //        fileInfoCmd.Parameters.AddWithValue("@IsProedison", isProedison);
                        //        fileInfoCmd.Parameters.AddWithValue("@HasHDR", hasHDR);

                        //        // Execute the query
                        //        fileInfoCmd.ExecuteNonQuery();
                        //    }

                        //    // Turn off the ability to insert explicit IDs
                        //    setIdentityInsertQuery = "SET IDENTITY_INSERT COMTRADE OFF;";
                        //    using (SqlCommand setIdentityCmd = new SqlCommand(setIdentityInsertQuery, con, transaction))
                        //    {
                        //        setIdentityCmd.ExecuteNonQuery();
                        //    }

                        // Get the count of existing records to generate ComtradeID
                         int ComtradeID = 0;
                        string countQuery = "SELECT COUNT(*) FROM CFGTABLE";
                        using (SqlCommand countCmd = new SqlCommand(countQuery, con, transaction))
                        {
                            ComtradeID = (int)countCmd.ExecuteScalar() + 1;
                        }


                        // Parse the file and extract necessary values
                        var fileLines = File.ReadAllLines(filePath);

                            // Check if the file has the minimum necessary lines
                            if (fileLines.Length < 10)
                            {
                                MessageBox.Show("The file does not contain enough lines. Please verify the file structure.");
                                return;
                            }
                            
                            

                            // Parse first line (Relay-1,780,2013)
                            string[] firstLine = fileLines[0].Split(',');
                            if (firstLine.Length < 3)
                            {
                                MessageBox.Show("Invalid format in the first line.");
                                return;
                            }
                            string station = firstLine[0];
                            string deviceID = firstLine[1];
                            string year = firstLine[2];
                            short version = short.Parse(year);

                            // Parse second line to get the offset integer
                            string[] secondLine = fileLines[1].Split(',');
                            if (secondLine.Length == 0 || !int.TryParse(secondLine[0], out int offset))
                            {
                                MessageBox.Show("Invalid format in the second line or offset value is missing.");
                                return;
                            }
                            // get the counts of analoge and digital
                            string analog = secondLine[1];
                            // convert 26A to 26
                            int analogeCount = int.Parse(new string(analog.TakeWhile(char.IsDigit).ToArray()));

                            string digital = secondLine[2];
                            int  digitalCount = int.Parse(new string(digital.TakeWhile(char.IsDigit).ToArray())); ;

                          
                            

                            for (int i = 2; i < analogeCount+2; i++)
                            {
                                //parsing analoge values
                                string[] analogeValues = fileLines[i].Split(",");

                                
                                int AnalogueIndex = int.Parse(analogeValues[0]);
                                string ChannelID = analogeValues[1];
                                string Phase = analogeValues[2];
                                string CCBM = analogeValues[3];
                                string Unit = analogeValues[4];
                                string Multiplier = analogeValues[5];
                                float Offset = float.Parse(analogeValues[6]);
                                float Skew = float.Parse(analogeValues[7]);
                                float Minvalue = float.Parse(analogeValues[8]);
                                float Maxvalue = float.Parse(analogeValues[9]);
                                float Primary = float.Parse(analogeValues[10]);
                                float Secondary = float.Parse(analogeValues[11]);
                                string Channeltype = analogeValues[12];

                                // sql query for inserting the value to the table  
                                string insertAnalogeQuery = @"
                                INSERT INTO ANALOGE
                                    (ComtradeID, AnalogIndex, ChannelID, Phase, CCBM, Unit, Multiplier, Offset, Skew, MinValue, MaxValue, PrimaryValue, Secondaryvalue, ChannelType)
                                VALUES 
                                    (@ComtradeID, @AnalogIndex, @ChannelID, @Phase, @CCBM, @Unit, @Multiplier, @Offset, @Skew, @MinValue, @MaxValue, @PrimaryValue, @SecondaryValue, @ChannelType)";

                                using (SqlCommand cmdAnalogue = new SqlCommand(insertAnalogeQuery, con, transaction))
                                {
                                    // Add parameters for the insert query
                                    cmdAnalogue.Parameters.AddWithValue("@ComtradeID", ComtradeID);
                                    cmdAnalogue.Parameters.AddWithValue("@AnalogIndex", AnalogueIndex);
                                    cmdAnalogue.Parameters.AddWithValue("@ChannelID", ChannelID);
                                    cmdAnalogue.Parameters.AddWithValue("@Phase", Phase);
                                    cmdAnalogue.Parameters.AddWithValue("@CCBM", CCBM);
                                    cmdAnalogue.Parameters.AddWithValue("@Unit", Unit);
                                    cmdAnalogue.Parameters.AddWithValue("@Multiplier", Multiplier);
                                    cmdAnalogue.Parameters.AddWithValue("@Offset", Offset);
                                    cmdAnalogue.Parameters.AddWithValue("@Skew", Skew);
                                    cmdAnalogue.Parameters.AddWithValue("@MinValue", Minvalue);
                                    cmdAnalogue.Parameters.AddWithValue("@MaxValue", Maxvalue);
                                    cmdAnalogue.Parameters.AddWithValue("@PrimaryValue", Primary);
                                    cmdAnalogue.Parameters.AddWithValue("@SecondaryValue", Secondary);
                                    cmdAnalogue.Parameters.AddWithValue("@ChannelType", Channeltype);

                                    // Execute the insert query for analog data
                                    cmdAnalogue.ExecuteNonQuery();
                                }
                            }

                            // parsing and insertig in digital value

                            for (int i = analogeCount+2; i < analogeCount+2+digitalCount; i++)
                            {
                                // parsing digital value
                                string[] digitalValues = fileLines[i].Split(",");

                                int DigitalIndex = int.Parse(digitalValues[0]);
                                string ChannelID = digitalValues[1];
                                string Phase = digitalValues[2];
                                string CCBM = digitalValues[3];
                                int InitialState = int.Parse(digitalValues[4]);

                                string insertDigitalQuery = @"
                                INSERT INTO DIGITAL
                                        (ComtradeID, DigitalIndex, ChannelID, Phase, CCBM, InitialState)
                                VALUES
                                        (@ComtradeID, @DigitalIndex, @ChannelID, @Phase, @CCBM, @InitialState)";

                                using (SqlCommand cmdDigital = new SqlCommand(insertDigitalQuery, con, transaction)) 
                                {
                                    // Add parameters for the insert query
                                    cmdDigital.Parameters.AddWithValue("@ComtradeID", ComtradeID);
                                    cmdDigital.Parameters.AddWithValue("@DigitalIndex", DigitalIndex);
                                    cmdDigital.Parameters.AddWithValue("@ChannelID", ChannelID);
                                    cmdDigital.Parameters.AddWithValue("@Phase", Phase);
                                    cmdDigital.Parameters.AddWithValue("@CCBM", CCBM);
                                    cmdDigital.Parameters.AddWithValue("@InitialState", InitialState);


                                    // Execute the insert query for analog data
                                    cmdDigital.ExecuteNonQuery();
                                }
                            }






                            // Calculate the start line for timestamp and other data
                            int startLineIndex = 1 + offset;

                            // Ensure the file has enough lines for the calculated index
                            if (fileLines.Length > startLineIndex + 100) // Need at least 10 lines after the start line
                            {
                                MessageBox.Show("The file does not contain enough data lines based on the offset.");
                                return;
                            }

                            // Parse the necessary values from the file
                            string frequencyValue = fileLines[startLineIndex + 1]; // Tracking Frequency
                            float frequency = float.Parse(frequencyValue);

                            string sampleRateValue = fileLines[startLineIndex + 2]; // Sample rate
                            int sampleRate = int.Parse(sampleRateValue);

                            string[] sampleRateLine = fileLines[startLineIndex + 3].Split(',');
                            if (sampleRateLine.Length < 2)
                            {
                                MessageBox.Show("Invalid format for sample rate and last sample count.");
                                return;
                            }
                            string sampleCountvalue = sampleRateLine[0];
                            float sampleCount = float.Parse(sampleCountvalue);

                            string lastSampleCountValue = sampleRateLine[1];
                            int lastSampleCount = int.Parse(lastSampleCountValue);

                            string firstTime = fileLines[startLineIndex + 4];
                            DateTime firstTimeStamp  = DateTime.MaxValue;
                            //DateTime firstTimeStamp =DateTime.Parse(firstTime);
                            try
                            {
                                 firstTimeStamp = DateTime.ParseExact(firstTime, "dd/MM/yyyy HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                                // Use dateTime for database operations
                            }
                            catch (FormatException)
                            {
                                // Handle format exceptions if the parsing fails
                                Console.WriteLine("Invalid date format.");
                            }


                            string triggertime = fileLines[startLineIndex + 5];
                            DateTime TriggerTime = DateTime.MaxValue;
                            //DateTime triggerTime = DateTime.Parse(triggertime);
                            try
                            {
                                 TriggerTime = DateTime.ParseExact(triggertime, "dd/MM/yyyy HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                                // Use dateTime for database operations
                            }
                            catch (FormatException)
                            {
                                // Handle format exceptions if the parsing fails
                                Console.WriteLine("Invalid date format.");
                            }




                            string dataType = fileLines[startLineIndex + 6];
                            string timeMultiplierValue = fileLines[startLineIndex + 7];
                            float timeMultiplier = float.Parse(timeMultiplierValue);

                            string[] localUtc = fileLines[startLineIndex + 8].Split(",");
                            if (localUtc.Length < 2)
                            {
                                MessageBox.Show("Invalid format for Local and UTC times.");
                                return;
                            }
                            string localTime = localUtc[0];
                            string utcTime = localUtc[1];

                            string[] timeLeapSecond = fileLines[startLineIndex + 9].Split(',');
                            if (timeLeapSecond.Length < 2)
                            {
                                MessageBox.Show("Invalid format for Time Quality Indicator and Leap Second.");
                                return;
                            }
                            string timeQuality = timeLeapSecond[0];
                            string LeapSecond = timeLeapSecond[1];
                            short leapSecond = short.Parse(LeapSecond);

                            // Insert data into the database
                            string insertCFGQuery = @"
                                INSERT INTO CFGTABLE
                                    (ComtradeID, Station, DeviceID, CfgVersion, Frequency, SampleRate, SampleCountHz, LastSampleCount, FirstSampleTime, TriggerTime, DataType, TimeMultiplier, LocalTime, UTCTime, TimeQualityIndicatorCode, LeapSecond)
                                VALUES 
                                    (@ComtradeID, @Station, @DeviceID, @CfgVersion, @Frequency, @SampleRate, @SampleCountHz, @LastSampleCount, @FirstSampleTime, @TriggerTime, @DataType, @TimeMultiplier, @LocalTime, @UTCTime, @TimeQualityIndicatorCode, @LeapSecond)";

                            using (SqlCommand cmd = new SqlCommand(insertCFGQuery, con, transaction))
                            {
                                // Add parameters for the insert query
                                cmd.Parameters.AddWithValue("@ComtradeID", ComtradeID);
                                cmd.Parameters.AddWithValue("@Station", station);
                                cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                                cmd.Parameters.AddWithValue("@CfgVersion", version);
                                cmd.Parameters.AddWithValue("@Frequency", frequency);
                                cmd.Parameters.AddWithValue("@SampleRate", sampleRate);
                                cmd.Parameters.AddWithValue("@SampleCountHz", sampleCount);
                                cmd.Parameters.AddWithValue("@LastSampleCount", lastSampleCount);
                                cmd.Parameters.AddWithValue("@FirstSampleTime", firstTimeStamp);
                                cmd.Parameters.AddWithValue("@TriggerTime", TriggerTime);
                                cmd.Parameters.AddWithValue("@DataType", dataType);
                                cmd.Parameters.AddWithValue("@TimeMultiplier", timeMultiplier);
                                cmd.Parameters.AddWithValue("@LocalTime", localTime);
                                cmd.Parameters.AddWithValue("@UTCTime", utcTime);
                                cmd.Parameters.AddWithValue("@TimeQualityIndicatorCode", timeQuality);
                                cmd.Parameters.AddWithValue("@LeapSecond", leapSecond);

                                // Execute the insert query
                                cmd.ExecuteNonQuery();
                            }

                            // Commit the transaction after successful insert
                            transaction.Commit();
                            MessageBox.Show("Data inserted successfully!");
                        }
                        catch (Exception ex)
                        {
                            // Rollback transaction if any error occurs
                            transaction.Rollback();
                            
                            // Re-insert the COMTRADE record with errorFlag = 1
                            //string updateErrorFlagQuery = @"
                            //UPDATE COMTRADE 
                            //SET Error = 1 
                            //WHERE ComtradeID = @ComtradeID";

                            //using (SqlCommand updateErrorCmd = new SqlCommand(updateErrorFlagQuery, con))
                            //{
                            //    updateErrorCmd.Parameters.AddWithValue("@ComtradeID", ComtradeID);
                            //    updateErrorCmd.ExecuteNonQuery();
                            //}

                            MessageBox.Show("Error during insertion: " + ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
