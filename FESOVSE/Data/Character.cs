using System.Collections.Generic;

namespace FESOVSE.Data
{
    class Character
    {
        public string CharID { get; set; }
        public string Name { get; set; }
        public int StartAddress { get; set; }
        public int ItemAddress { get; set; }
        public List<int> BaseStats { get; set; }
        public List<int> MaxStats { get; set; }
    }
}
