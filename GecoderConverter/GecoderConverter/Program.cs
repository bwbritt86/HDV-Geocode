using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;


namespace GecoderConverter
{ 
    class Program
    {
        static void Main(string[] args)
        {
            //Billy (Do one month at a time)
            /*insertGeocode("January", "2017");
            insertGeocode("February", "2017");
            insertGeocode("March", "2017");*/

            //Cody (Do one month at a time)
            /*insertGeocode("April", "2017");
            insertGeocode("May", "2017");
            insertGeocode("June", "2017");*/

            //Jacob (Do one month at a time)
            /*insertGeocode("July", "2017");
            insertGeocode("August", "2017");
            insertGeocode("September", "2017");*/

            //Terrence (Do one month at a time)
            /*insertGeocode("October", "2017");
            insertGeocode("November", "2017");
            insertGeocode("December", "2017");*/



            //****Wait until November to do these****//

            //Billy (Do one month at a time)
            /*insertGeocode("January", "2016");
            insertGeocode("February", "2016");
            insertGeocode("March", "2016");*/

            //Cody (Do one month at a time)
            /*insertGeocode("April", "2016");
            insertGeocode("May", "2016");
            insertGeocode("June", "2016");*/

            //Jacob (Do one month at a time)
            /*insertGeocode("July", "2016");
            insertGeocode("August", "2016");
            insertGeocode("September", "2016");*/

            //Terrence (Do one month at a time)
            /*insertGeocode("October", "2016");
            insertGeocode("November", "2016");
            insertGeocode("December", "2016");*/
        }

        static void insertGeocode(String month, String year)
        {
            //Set database connection string
            String conStr = "Data Source = dcm.uhcl.edu; Initial Catalog = c438818fa01g3; User ID = c438818fa01g3; Password = 6552950";

            int iteration = 0;

            using (SqlConnection conn = new SqlConnection(conStr))
            {

                //Create list to hold retrieved Lat-Long coordinate objects              
                List<LatLongTbl> latLongList = new List<LatLongTbl>();

                //Select all fields from table
                String selectTbl = "SELECT * FROM " + month + "_" + year;
                SqlCommand sqlCommandSelect = new SqlCommand(selectTbl, conn);
                
                //Retrieve information from database
                try
                {
                    conn.Open();
                    SqlDataReader reading = sqlCommandSelect.ExecuteReader();
                    {
                        while (reading.Read())
                        {
                            //Create table objects for housing data
                            AddressTbl aTbl = new AddressTbl();
                            LatLongTbl llTbl = new LatLongTbl();

                            //Retrieve row from address table
                            aTbl.OffenseType = (String)reading["Offense_Type"];
                            aTbl.Number = (String)reading["Number"];
                            aTbl.Street = (String)reading["Street"];
                            aTbl.Type = (String)reading["Type"];
                            aTbl.Suffix = (String)reading["Suffix"];

                            //remove frivolous whitespace
                            aTbl.OffenseType = aTbl.OffenseType.Replace(" ", "");
                            aTbl.Number = aTbl.Number.Replace(" ", "");
                            aTbl.Street = aTbl.Street.Replace(" ", "");
                            aTbl.Type = aTbl.Type.Replace(" ", "");
                            aTbl.Suffix = aTbl.Suffix.Replace(" ", "");

                            //retrieve two numbers from address field and split them
                            String[] num = aTbl.Number.Split(new[] { '-' },
                                                                    StringSplitOptions.RemoveEmptyEntries);

                            //Calculate average of two numbers
                            int numA = Int32.Parse(num[0]);
                            int numB = Int32.Parse(num[1]);
                            int stNum = ((Int32.Parse(num[0]) + Int32.Parse(num[1])) / 2);

                            //Create address string for use with geocoder
                            String crimeAddress = null;

                            if (aTbl.Suffix == "" && aTbl.Type == "")
                            {
                                crimeAddress = stNum.ToString() + "+" + aTbl.Street + ",+HOUSTON,+TX";
                            }
                            else if (aTbl.Suffix != "" && aTbl.Type == "")
                            {
                                crimeAddress = stNum.ToString() + "+" + aTbl.Suffix + "+" + aTbl.Street + ",+HOUSTON,+TX";
                            }
                            else if (aTbl.Suffix == "" && aTbl.Type != "")
                            {
                                crimeAddress = stNum.ToString() + "+" + aTbl.Street + "+" + aTbl.Type + ",+HOUSTON,+TX";
                            }
                            else
                            {
                                crimeAddress = stNum.ToString() + "+" + aTbl.Street + "+" + aTbl.Type + ",+HOUSTON,+TX";
                            }

                            System.Threading.Thread.Sleep(2000);

                            //Retrieve Lat-Long coordinates
                            String[] latLong = new String[2];
                            latLong = osmGeocoder(crimeAddress);

                            //put retrieved data into LatLong object
                            llTbl.OffenseType = aTbl.OffenseType;
                            llTbl.Latitude = latLong[0];
                            llTbl.Longitude = latLong[1];

                            
                            //check if latLong array has data
                            if (latLong[0] != "" && latLong[1] != "")
                            {
                                Console.WriteLine("\n\nlat: " + latLong[0] + "\nlong: " + latLong[1]);

                                //Add LatLong object to list
                                latLongList.Add(llTbl);
                            }
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }

                //Create new table for Latitude and Longitude coordinates
                String createTbl = "CREATE TABLE " + month + "_" + year + "_coord(Offense_Type char(50),Latitude char(50),Longitude char(50));";
                SqlCommand createCommand = new SqlCommand(createTbl, conn);
                try
                {
                    conn.Open();
                    createCommand.ExecuteNonQuery();
                }
                finally
                {
                    conn.Close();
                }

                //Insert data into database
                try
                {
                    conn.Open();

                    //Run through all data in object and insert into table
                    foreach (LatLongTbl lLongTbl in latLongList)
                    {
                        string insertRow = "INSERT INTO " + month + "_" + year + "_coord (Offense_Type,Latitude,Longitude)"
                          + " VALUES ('" + lLongTbl.OffenseType + "','" + lLongTbl.Latitude + "','" + lLongTbl.Longitude + "');";
                        SqlCommand insertCommand = new SqlCommand(insertRow, conn);
                        insertCommand.ExecuteNonQuery();

                        iteration++;
                        Console.WriteLine("Row = " + iteration);
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        static String[] osmGeocoder(string address)
        {
            String[] latLong = new string[2];

            //Insert account email for openstreetmap URL
            string email = ""; //<-- put your email address you used to make openstreetmap account here

            //create openstreetmap URL with parameters
            String url = "https://nominatim.openstreetmap.org/search?q=" + address + "&format=json&polygon=1&email=" + email +"&addressdetails=1";

            using (var w = new WebClient())
            {
                try
                {
                    //Retrieve address information as JSON file
                    var json_data = w.DownloadString(url);

                    //Retrieve latitude and longitude from JSON string
                    var r = (JArray)JsonConvert.DeserializeObject(json_data);

                    latLong[0] = ((JValue)r[0]["lat"]).Value as string;
                    latLong[1] = ((JValue)r[0]["lon"]).Value as string;
                }
                catch (Exception ex)
                {
                    latLong[0] = "";
                    latLong[1] = "";
                }
            }
            return latLong;
        }       
    }
    
}
