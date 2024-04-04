using BottlePos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BottlePos
{
    class Program
    {
        static void Main(string[] args)
        {

            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting current in pOSSettings.PosDetails)
                {
                    try
                    {
                        if (current.PosName.ToUpper() == "BOTTLEPOSAPI") // Get the data from request and attached to list 
                        {


                            var data = GetData(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.Username, current.StoreSettings.POSSettings.Password, current.StoreSettings.POSSettings.AuthUrl, current.StoreSettings.POSSettings.ItemUrl, current.StoreSettings.POSSettings.FtpUserName, current.StoreSettings.POSSettings.FtpPassword);
                            var jObj = (JObject.Parse(data)["data"]);
                            Dictionary<object, object> dictObj = jObj.ToObject<Dictionary<object, object>>();
                            var itemsObj = dictObj.ToList().Select(s => s.Value).ToList();
                            var itemList = new List<Item>();
                            foreach (var item in itemsObj)
                            {
                                itemList.Add(JsonConvert.DeserializeObject<Item>(item.ToString()));
                            }
                            BottleposcsvConverter(itemList, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.FtpUserName, current.StoreSettings.POSSettings.FtpPassword);
                            Console.WriteLine();



                        }



                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        new clsEmail().sendEmail(DeveloperId, "", "", "Error in BottlePos@" + DateTime.UtcNow + current.StoreSettings.StoreId + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in BottlePos@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
        }
        private static string GetData(int Storeid, string Username, string Password, string AuthUrl, string ItemUrl, string FtpUserName, string FtpPassword) /// Send the requesting with username and password 
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var BaseUrl = AuthUrl;
                Password = ComputeSha256Hash(Password);
                var dataObj = JsonConvert.SerializeObject(new { username = Username, password = Password });
                BaseUrl = $"{BaseUrl}?data={dataObj}";
                var responseData = string.Empty;
                HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });
                httpClient.BaseAddress = new Uri(BaseUrl);
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                HttpResponseMessage response = new HttpResponseMessage();
                Task.Run(async () =>
                {
                    response = await httpClient.PostAsync("", new StringContent(""));
                    responseData = response.Content.ReadAsStringAsync().Result;
                    response.EnsureSuccessStatusCode();
                }).Wait();
                var BaseUrl1 = ItemUrl;
                response = new HttpResponseMessage();
                Task.Run(async () =>
                {
                    response = await httpClient.GetAsync(BaseUrl1);
                    responseData = response.Content.ReadAsStringAsync().Result;
                    response.EnsureSuccessStatusCode();
                }).Wait();
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + Storeid);
            }
            return "";
        }

        private static string ComputeSha256Hash(string rawData) /// Converting Password using sha256Hash
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static void BottleposcsvConverter(List<Item> productList, int storeid, decimal tax, string FtpUserName, string FtpPassword) /// List to Datamodel conversion
        {

            try
            {
                string deposit = ConfigurationManager.AppSettings["deposit"];
                string markupprice = ConfigurationManager.AppSettings["markupprice"];
                string folderPath = ConfigurationManager.AppSettings.Get("BaseDirectory");
                string taxMixers = ConfigurationManager.AppSettings.Get("taxMixers");
                string taxMixers11875 = ConfigurationManager.AppSettings.Get("taxMixers11875");
                string excludeOutOfStock = ConfigurationManager.AppSettings.Get("excludeOutOfStock");
                string specifiedUOM = ConfigurationManager.AppSettings.Get("specifiedUOM");
                string staticQTY999 = ConfigurationManager.AppSettings.Get("staticQty999");

                List<datatableModel> pf = new List<datatableModel>();
                List<FullNameProductModel> pd = new List<FullNameProductModel>();

                foreach (var item in productList)
                {
                    try
                    {
                        datatableModel pdf = new datatableModel();
                        FullNameProductModel fdf = new FullNameProductModel();
                        pdf.StoreID = storeid;
                        pdf.upc = item.code;
                        string abc = pdf.upc;
                        string[] number = abc.Split(',');

                        string up = Regex.Match(number[0], @"^\d+$").ToString();
                        if (!string.IsNullOrEmpty(up))
                        {
                            pdf.upc = '#' + up;
                            fdf.upc = '#' + up;
                            pdf.sku = '#' + up;
                            fdf.sku = '#' + up;
                        }
                        else
                        {
                            continue;
                        }

                        decimal qty = Convert.ToDecimal(item.total_stock);
                        pdf.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;

                        pdf.pack = "1";
                        fdf.pack = 1;
                        pdf.uom = item.description;
                        fdf.uom = item.description;
                        pdf.Tax = tax;
                        string b = item.name;
                        if (b.Contains("\n"))
                        {
                            b = b.Replace("\n", String.Empty);
                        }
                        pdf.StoreProductName = b;
                        fdf.pname = b;
                        string a = item.name;
                        if (a.Contains("\n"))
                        {
                            a = a.Replace("\n", String.Empty);
                        }
                        pdf.StoreDescription = a.Trim();
                        fdf.pdesc = a.Trim();
                        //if (markupprice.Contains(storeid.ToString()))
                        //{
                        //    var pri= Convert.ToDecimal(item.price);
                        //    var pr = pri + pri / 100 * 7;
                        //    pdf.Price = Math.Round(pr,2);
                        //    fdf.Price = Math.Round(pr, 2);
                        //}
                        //else
                        //{
                        pdf.Price = Convert.ToDecimal(item.price);
                        fdf.Price = Convert.ToDecimal(item.price);
                        //}
                        if (pdf.Price <= 0 || fdf.Price <= 0)
                        {
                            continue;
                        }
                        pdf.Start = "";
                        pdf.End = "";


                        //if (storeid == 10755 && (item.category_name.ToUpper().Contains("LIQUOR") && item.description.ToUpper().Replace("/s+", "").Contains("50ML") || item.category_name.ToUpper().Contains("BEER SINGLES") || item.category_name.ToUpper().Contains("SINGLE BEER")))
                        //{
                        //    continue;
                        //}



                        //if (storeid == 10755 && (item.category_name.ToUpper().Contains("LIQUOR") && item.category_name.ToUpper().Contains("TEQUILA") && item.category_name.ToUpper().Contains("WHISKY") && item.category_name.ToUpper().Contains("BRANDY") && item.category_name.ToUpper().Contains("BOURBON") && item.category_name.ToUpper().Contains("VODKA") && item.category_name.ToUpper().Contains("GIN") && item.category_name.ToUpper().Contains("COGNAC") && item.category_name.ToUpper().Contains("LIQUERES & CORDI") && item.category_name.ToUpper().Contains("RTD COCKTAILS") && item.category_name.ToUpper().Contains("IRISH WHISKEY") && item.category_name.ToUpper().Contains("MEZCAL") && item.category_name.ToUpper().Contains("LIQUEUR") && item.category_name.ToUpper().Contains("WHISKEY") && item.category_name.ToUpper().Contains("BOURBONS") && item.description.ToUpper().Replace("/s+", "").Contains("50ML") || item.category_name.ToUpper().Contains("BEER SINGLES") || item.category_name.ToUpper().Contains("SINGLE BEER")))
                        //{
                        //    continue;
                        //}

                        string[] liquorKeywords = { "LIQUOR", "MINIATURES", "TEQUILA", "WHISKY", "BRANDY", "BOURBON", "VODKA", "GIN", "COGNAC", "LIQUERES & CORDI", "RTD COCKTAILS", "IRISH WHISKEY", "MEZCAL", "LIQUEUR", "WHISKEY", "BOURBONS" };

                        string pattern = @"\b50ML\b";
                        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);


                        if (specifiedUOM.Contains(storeid.ToString()) && (liquorKeywords.Any(keyword => item.category_name.ToUpper().Contains(keyword)) && regex.IsMatch(item.description.ToUpper().Replace(" ", "")) || item.category_name.ToUpper().Contains("BEER SINGLES") || item.category_name.ToUpper().Contains("SINGLE BEER")))
                        {
                            continue;
                        }
                        else
                        {
                            fdf.pcat = item.category_name.ToString().Trim();
                            if (Regex.IsMatch(fdf.pcat, @"\d+"))
                            {
                                fdf.pcat = item.cat_group_name.ToString().Trim();
                            }
                        }



                        if (storeid != 11833 && fdf.pcat == "CIGARS" || fdf.pcat == "NON UPC CIGARS" || fdf.pcat == "ALLOCATED BOURBON" || fdf.pcat == "ALLOCATED" || fdf.pcat == "SEMI ALLOCATED BOURBON")
                        {
                            continue;
                        }

                        if (storeid == 11833 && fdf.pcat == "ALLOCATED BOURBON" || fdf.pcat == "ALLOCATED" || fdf.pcat == "SEMI ALLOCATED BOURBON")
                        {
                            continue;
                        }


                        if (storeid == 11458) //static qty as per ticket #20867
                        {
                            if (fdf.pcat == "BAR & GLASSWARE" || fdf.pcat == "BAR ESSENTIALS" || fdf.pcat == "CLUB SODA" || fdf.pcat == "COCKTAIL MIXES" || fdf.pcat == "DRINK GARNISHMENTS" || fdf.pcat == "ENERGY DRINKS" || fdf.pcat == "FOOD & SNACKS" || fdf.pcat == "GINGER BEER" || fdf.pcat == "JUICE" || fdf.pcat == "LIGHTERS" || fdf.pcat == "SNACKS" || fdf.pcat == "SODA" || fdf.pcat == "SPORTS DRINKS" || fdf.pcat == "SUPPLIES" || fdf.pcat == "SYRUPS" || fdf.pcat == "TONIC" || fdf.pcat == "WATER")
                            {
                                pdf.Qty = 999;
                            }
                        }
                        if (storeid == 11846)
                        {
                            if (fdf.pcat.Contains("GROC") || fdf.pcat.Contains("CHEW") || fdf.pcat.Contains("LOTTO") || fdf.pcat.Contains("SUND"))
                            {
                                pdf.Tax = Convert.ToDecimal(0.062);
                            }
                            else
                            {
                                pdf.Tax = tax;
                            }
                        }
                        fdf.pcat1 = "";
                        fdf.pcat2 = "";
                        fdf.country = "";
                        fdf.region = "";
                        if (taxMixers.Contains(storeid.ToString()))
                        {
                            if (fdf.pcat == "MIXERS")
                            {
                                pdf.Tax = Convert.ToDecimal(0.091);
                            }
                        }
                        if (taxMixers11875.Contains(storeid.ToString()))
                        {
                            if (fdf.pcat == "MIXER")
                            {
                                pdf.Tax = Convert.ToDecimal(0.06);
                            }
                        }
                        if (number.Length == 2)
                        {
                            pdf.altupc1 = '#' + number.ElementAt(1);
                        }
                        else if (number.Length == 3)
                        {
                            pdf.altupc1 = '#' + number.ElementAt(1);
                            pdf.altupc2 = '#' + number.ElementAt(2);
                        }
                        else if (number.Length == 4)
                        {
                            pdf.altupc1 = '#' + number.ElementAt(1);
                            pdf.altupc2 = '#' + number.ElementAt(2);
                            pdf.altupc3 = '#' + number.ElementAt(3);
                        }
                        if (deposit.Contains(storeid.ToString()))
                        {
                            if (fdf.pcat == "BEER")
                            {
                                pdf.deposit = Convert.ToDecimal(0.05);
                            }
                        }

                        if (staticQTY999.Contains(storeid.ToString()))
                        {
                            pdf.Qty = 999;
                        }



                        if (excludeOutOfStock.Contains(storeid.ToString()))
                        {
                            if (pdf.Qty > 0)
                            {
                                pf.Add(pdf);
                                pd.Add(fdf);
                            }
                        }

                        else
                        {
                            pf.Add(pdf);
                            pd.Add(fdf);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                GenerateCSVFile.GenerateCSVFiles(pf, "PRODUCT", storeid, folderPath, FtpUserName, FtpPassword);
                GenerateCSVFile.GenerateCSVFiles(pd, "FULLNAME", storeid, folderPath, FtpUserName, FtpPassword);
                Console.WriteLine("Generated BottlePos" + storeid + "product csv File........");
                Console.WriteLine("Geneerated  Bottlepos" + storeid + "Fullname csv File......");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
    public class clsProductList
    {
        public bool StatusVal { get; set; }
        public int StatusCode { get; set; }
        public string StatusMsg { get; set; }
        public string Price { get; set; }
        public string SessionID { get; set; }

        public string Url { get; set; }
        public class Data
        {
            public string UPC { get; set; }
            public string SKU { get; set; }
            public string ItemName { get; set; }
            public decimal Price { get; set; }
            public decimal Cost { get; set; }
            public decimal SALEPRICE { get; set; }
            public string SizeName { get; set; }
            public string PackName { get; set; }
            public string Vintage { get; set; }
            public string Department { get; set; }
            public decimal PriceA { get; set; }
            public decimal PriceB { get; set; }
            public decimal PriceC { get; set; }
            public decimal total_stock { get; set; }
            public decimal tax { get; set; }
        }

        public class items
        {
            public List<Data> item { get; set; }
        }
    }
    public class PTECHclsProductList
    {
        public bool StatusVal { get; set; }
        public int StatusCode { get; set; }
        public string StatusMsg { get; set; }
        public string Price { get; set; }
        public string SessionID { get; set; }

        public string Url { get; set; }
        public class Data
        {
            public string UPC { get; set; }
            public string SKU { get; set; }
            public string ItemName { get; set; }
            public decimal Price { get; set; }
            public decimal Cost { get; set; }
            public decimal SALEPRICE { get; set; }
            public string SizeName { get; set; }
            public string PackName { get; set; }
            public string Vintage { get; set; }
            public string Department { get; set; }
            public decimal PriceA { get; set; }
            public decimal PriceB { get; set; }
            public decimal PriceC { get; set; }
            public Int32 TotalQty { get; set; }
            public decimal tax { get; set; }
        }
        public class items
        {
            public List<Data> item { get; set; }
        }
    }

    public class datatableModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public string pack { get; set; }
        public string uom { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal deposit { get; set; }
    }
    public class ProductsModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public Int64 Qty { get; set; }
        public string sku { get; set; }
        public string pack { get; set; }
        public string uom { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal Deposit { get; set; }

    }
    class FullNameProductModel
    {
        public string pname { get; set; }
        public string pdesc { get; set; }
        public string upc { get; set; }
        public string sku { get; set; }
        public decimal Price { get; set; }
        public string uom { get; set; }
        public int pack { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string country { get; set; }
        public string region { get; set; }
    }
}
