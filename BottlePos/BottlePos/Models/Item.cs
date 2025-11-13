using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottlePos.Models
{
    public class Item
    {
        public string code { get; set; }
        public string id { get; set; }
        public string qty { get; set; }
        public string name { get; set; }
        public string alt_name { get; set; }
        public string description { get; set; }
        public object taxid { get; set; }
        public string unitspercase { get; set; }
        public string casecosttotal { get; set; }
        public string cost { get; set; }
        public string lastcost { get; set; }
        public string price { get; set; }
        public string webprice { get; set; }
        public string margin { get; set; }
        //public string markup { get; set; }
        public string supplierid { get; set; }
        public string categoryid { get; set; }
        public string minprice { get; set; }
        public string reorder_value { get; set; }
        public string reorder_point { get; set; }
        public string itemrank { get; set; }
        public string vendoritemno { get; set; }
        public object autoupdate { get; set; }
        public bool promptforqty { get; set; }
        public bool donottrackinventory { get; set; }
        public string notes { get; set; }
        public string itemtype { get; set; }
        public string reminddate { get; set; }
        public object closeout { get; set; }
        //public bool donotdiscountitem { get; set; }
        public string showtoweb { get; set; }
        //public bool showhideinventory { get; set; }
        //public bool allowebt { get; set; }
        public string vendorname { get; set; }
        public List<string> itemtags { get; set; }
        public bool shortcutkeys { get; set; }
        public string shortcutname { get; set; }
        public string itemsortno { get; set; }
        public string color_code { get; set; }
        //public bool itemviewonprompt { get; set; }
        public List<object> additionalcharges { get; set; }
        public string pointsmultiplier { get; set; }
        public string pointsvalue { get; set; }
        public string type { get; set; }
        public string percenttype { get; set; }
        //public List<StockCode> stock_codes { get; set; }
        public string merge_ids { get; set; }
        public List<Modifier> modifiers { get; set; }
        public List<object> additionalPrice { get; set; }
        public string cat_group_name { get; set; }
        public string total_stock { get; set; }
        public string seprate_total_stock { get; set; }
        public string upcfull { get; set; }
        public string category_name { get; set; }
        public string supplier_name { get; set; }
        public int sort_index { get; set; }
    }

    public class Modifier
    {
        public string type { get; set; }
        public string qty { get; set; }
        public string minqty { get; set; }
        public string maxqty { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string cost { get; set; }
        public string lastcost { get; set; }
        public string margin { get; set; }
        public string markup { get; set; }
        public bool viewonprompt { get; set; }
    }
}
