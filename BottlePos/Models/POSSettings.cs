using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottlePos
{
    public class POSSettings
    {
        public List<POSSetting> PosDetails { get; set; }
        public void IntializeStoreSettings()
        {
            DataSet dsResult = new DataSet();
            List<POSSetting> posdetails = new List<POSSetting>();
            try
            {
                List<SqlParameter> sparams = new List<SqlParameter>();
                sparams.Add(new SqlParameter("@PosId", 67));
                string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add(sparams[0]);
                        cmd.CommandText = "usp_ts_GetStorePosSetting";
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dsResult);
                        }
                    }
                }
                if (dsResult != null || dsResult.Tables.Count > 0)
                {
                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        POSSetting pobj = new POSSetting();
                        pobj.Setting = dr["Settings"].ToString();
                        StoreSetting obj = new StoreSetting();
                        obj.StoreId = Convert.ToInt32(dr["StoreId"] == DBNull.Value ? 0 : dr["StoreId"]);
                        obj.POSSettings = JsonConvert.DeserializeObject<Setting>(pobj.Setting);
                        pobj.PosName = dr["PosName"].ToString();
                        pobj.PosId = Convert.ToInt32(dr["PosId"]);
                        pobj.StoreSettings = obj;
                        if (pobj.StoreSettings.POSSettings != null)
                        {
                            pobj.StoreSettings.POSSettings.categoriess = obj.POSSettings.categoriess;
                            pobj.StoreSettings.POSSettings.Upc = obj.POSSettings.Upc;
                        }
                        posdetails.Add(pobj);
                    }
                }
                PosDetails = posdetails;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();

            }

        }
    }
    public class POSSetting
    {
        public int PosId { get; set; }
        public string PosName { get; set; }
        public StoreSetting StoreSettings { get; set; }
        public string Setting { get; set; }
    }
    public class Setting
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public int AccountID { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public string merchantId { get; set; }
        public string Username { get; set; }
        public string AuthUrl { get; set; }
        public string ItemUrl { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
        public int SHOPID { get; set; }
        public string Code { get; set; }
        public string tokenid { get; set; }
        public string instock { get; set; }
        public string category { get; set; }
        public string BaseUrl { get; set; }
        public decimal tax { get; set; }
        public string PosFileName { get; set; }
        public string PosFileName2 { get; set; }
        public string APIKey { get; set; }
        public int StoreMapId { get; set; }
        public decimal liquortax { get; set; }
        public decimal liquortaxrateperlitre { get; set; }
        public List<categories> categoriess { set; get; }
        public string LocationId { get; set; }
        public bool IsSalePrice { get; set; }
        public bool IsMarkUpPrice { get; set; }
        public int MarkUpValue { get; set; }
        public bool IsApi { get; set; }
        public List<UPC> Upc { get; set; }
        public decimal beertax { get; set; }
        public decimal winetax { get; set; }
        public int client_id { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
    }
    public class categories
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal taxrate { get; set; }
        public Boolean selected { get; set; }
    }
    public class UPC
    {
        public string upccode { get; set; }
    }
    public class StoreSetting
    {
        public int StoreId { get; set; }
        public Setting POSSettings { get; set; }
    }
    public class storecat
    {
        public string catid { get; set; }
        public string catname { get; set; }
    }
}
