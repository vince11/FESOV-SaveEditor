using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FESOVSE.Data
{
    class CharacterClassDatabase
    {
        XDocument data;

        public CharacterClassDatabase()
        {
            string xml = Properties.Resources.Classes;
            System.IO.StringReader myXml = new System.IO.StringReader(xml);
            data = XDocument.Load(myXml);
        }

        private CharacterClass FromElement(XElement row)
        {
            var id = row.Attribute("id").Value;
            var name = row.Attribute("name").Value;
            return new CharacterClass
            {
                ClassID = id,
                Name = name
            };
        }

        public List<CharacterClass> getAll()
        {
            IEnumerable<XElement> elements = from x in data.Root.Descendants("class") select x;
            List<CharacterClass> cc = new List<CharacterClass>();
            foreach(XElement e in elements)
            {
                cc.Add(FromElement(e));
            }
            return cc;
        }
    }
}
