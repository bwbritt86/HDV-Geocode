using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Geocoding.Google;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;



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

                            //Retrieve Lat-Long coordinates
                            String[] latLong = new String[2];
                            latLong = geocoder(crimeAddress);

                            //put retrieved data into LatLong object
                            llTbl.OffenseType = aTbl.OffenseType;
                            llTbl.Latitude = latLong[0];
                            llTbl.Longitude = latLong[1];

                            //Add LatLong object to list
                            latLongList.Add(llTbl);
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


        static String[] geocoder(string address)
        {
            //Create url used to make requests to Google Geocoding API
            String[] latLong = new String[2];
            String apiKey = ""; //Put API Key here
            String url = "https://maps.googleapis.com/maps/api/geocode/xml?address=" + address + "&sensor=false&key=" + apiKey;

            //Make geocoding request
            WebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = request.GetResponse();
                if (response != null)
                {
                    XPathDocument document = new XPathDocument(response.GetResponseStream());
                    XPathNavigator navigator = document.CreateNavigator();

                    // get response status
                    XPathNodeIterator statusIterator = navigator.Select("/GeocodeResponse/status");
                    while (statusIterator.MoveNext())
                    {
                        if (statusIterator.Current.Value != "OK")
                        {
                            Console.WriteLine("Error: response status = '" + statusIterator.Current.Value + "'");
                        }
                    }

                    // get results
                    XPathNodeIterator resultIterator = navigator.Select("/GeocodeResponse/result");
                    while (resultIterator.MoveNext())
                    {
                        XPathNodeIterator formattedAddressIterator = resultIterator.Current.Select("formatted_address");
                        while (formattedAddressIterator.MoveNext())
                        {
                            //Console.WriteLine(" formatted_address: " + formattedAddressIterator.Current.Value);
                        }

                        XPathNodeIterator geometryIterator = resultIterator.Current.Select("geometry");
                        while (geometryIterator.MoveNext())
                        {
                            //Console.WriteLine(" geometry: ");

                            XPathNodeIterator locationIterator = geometryIterator.Current.Select("location");
                            while (locationIterator.MoveNext())
                            {
                                //Console.WriteLine("     location: ");

                                XPathNodeIterator latIterator = locationIterator.Current.Select("lat");
                                while (latIterator.MoveNext())
                                {
                                    //Save latitude value
                                    latLong[0] = (String)latIterator.Current.Value;
                                }

                                XPathNodeIterator lngIterator = locationIterator.Current.Select("lng");
                                while (lngIterator.MoveNext())
                                {
                                    //save longitude value
                                    latLong[1] = (String)lngIterator.Current.Value;
                                }
                            }

                            XPathNodeIterator locationTypeIterator = geometryIterator.Current.Select("location_type");
                            while (locationTypeIterator.MoveNext())
                            {
                                //Console.WriteLine("         location_type: " + locationTypeIterator.Current.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //Console.WriteLine("Clean up");
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }

            //Send Lat-Long coordinates back to insertGeocode()
            return latLong;
        }
    }
    
}
