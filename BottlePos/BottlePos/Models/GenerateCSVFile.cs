using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;

namespace BottlePos
{
    class GenerateCSVFile
    {

        public static void GenerateCSVFiles<T>(IList<T> list, string Name, int StoreId, string BaseUrl,string FtpUserName,string FtpPassword)
        {

            // if (list == null || list.Count == 0) return "Quantity and Price are 0 or less than 0";
            if (!Directory.Exists(BaseUrl + "\\" + StoreId + "\\Upload\\"))
            {
                Directory.CreateDirectory(BaseUrl + "\\" + StoreId + "\\Upload\\");
            }
            string filename = Name + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";//Name:Product or FullName
            string fcname = BaseUrl + "\\" + StoreId + "\\Upload\\" + filename;
            // Console.WriteLine("Generating " + filename + " ........");
            //File.WriteAllText(BaseUrl + "\\" + StoreId + "\\Upload\\" + filename, csvData.ToString());
            // return filename;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;

            using (var sw = new StreamWriter(fcname))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    if (pi.Name != "Productid" && pi.Name != "cat")
                        sw.Write(pi.Name + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        if (pi.Name != "Productid" && pi.Name != "cat")
                        {
                            //this is the row+col intersection (the value)
                            string whatToWrite =
                            Convert.ToString(item.GetType()
                            .GetProperty(pi.Name)
                            .GetValue(item, null))
                            .Replace(',', ' ') + ',';
                            sw.Write(whatToWrite.Trim());
                        }
                    }
                    sw.Write(newLine);
                }
            }            
             UploadToFtp(filename,fcname, FtpUserName,FtpPassword); // Sending the files to FTP
        }
        public  static void UploadToFtp(string filename, string fcname,string FtpUserName,string FtpPassword)
        {
            try
            {
                #region Save Sourcefile to FTP
                string PFTPURL = ConfigurationManager.AppSettings.Get("PFTPServer");
                byte[] fileBytes = null;
                fileBytes = System.IO.File.ReadAllBytes(fcname);
                FtpWebRequest request;
                // Create FtpWebRequest object from the Uri provided
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + PFTPURL  + "/Upload/"  + filename));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(FtpUserName, FtpPassword);
                request.ContentLength = fileBytes.Length;
                request.UsePassive = true;
                request.UseBinary = true;
                request.ServicePoint.ConnectionLimit = fileBytes.Length;
                request.EnableSsl = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Close();
                }

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();

                FileInfo fa = new FileInfo(fcname);
                if (fa.Exists)
                    fa.Delete();
                #endregion
            }
           
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
