using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FESOVSE.Data
{
    class ItemDatabase
    {
        XDocument data;

        public ItemDatabase()
        {
            string xml = Properties.Resources.Items;
            System.IO.StringReader myXml = new System.IO.StringReader(xml); 
            data = XDocument.Load(myXml);
        }

        private Item FromElement(XElement row)
        {
            var name = row.Attribute("name").Value;
            var itemID = row.Attribute("id").Value;
            var itemHex = row.Attribute("hex").Value;
            XElement desc = row.Element("description");
            var maxForges = desc.Attribute("maxStars");
            var isdlc = desc.Attribute("isDLC");
            return new Item
            {
                Name = name,
                ItemID = itemID,
                Hex = itemHex,
                MaxForges = (int)maxForges,
                isDLC = (bool)isdlc
            };
        }

        public List<Item> getAll()
        {
            var xml = (from x in data.Root.Descendants("item")
                       select x).OrderBy(x => x.Attribute("name").Value);

            List<Item> items = new List<Item>();
            foreach(XElement i in xml)
            {
                items.Add(FromElement(i));
            }
            return items;
        }
    }
}
