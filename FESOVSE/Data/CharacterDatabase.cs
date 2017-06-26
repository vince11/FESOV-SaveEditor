using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FESOVSE.Data
{
    class CharacterDatabase
    {
        XDocument data;

        public CharacterDatabase()
        {
            string xml = Properties.Resources.Characters; //load the xml file from resource
            System.IO.StringReader myXml = new System.IO.StringReader(xml); //parse xml filename to string
            data = XDocument.Load(myXml);
        }

        private Character FromElement(XElement row)
        {
            var name = row.Attribute("name").Value;
            var charID = row.Attribute("id").Value;
            IEnumerable<XAttribute> baseStats = row.Element("base").Attributes();
            IEnumerable<XAttribute> maxStats = row.Element("max").Attributes();
            List<int> bs = new List<int>();
            List<int> ms = new List<int>();
            foreach(XAttribute xa in baseStats)
            {
                bs.Add(Int32.Parse(xa.Value));
            }
            foreach (XAttribute xa in maxStats)
            {
                ms.Add(Int32.Parse(xa.Value));
            }
            return new Character
            {
                Name = name,
                CharID = charID,
                StartAddress = -1,
                BaseStats = bs,
                MaxStats = ms
            };
        }

        public List<Character> getAll()
        {
            IEnumerable<XElement> elements = (from x in data.Root.Descendants("character")
                                              select x).OrderBy(y => y.Attribute("name").Value);
           

            List<Character> chars = new List<Character>();
            foreach(XElement e in elements)
            {
                chars.Add(FromElement(e));
            }

            return chars;
        }
    }
}
