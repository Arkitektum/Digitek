using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class LedesystemObject
    {

        public UserInfo UserInfo { get; set; }
        public LedesystemModel ModelInputs { get; set; }

    }

    public class LedesystemModel
    {
        public string rkl { get; set; }
        public string bkl { get; set; }
        public bool? bygningOffentligUnderTerreng { get; set; }
    }
}
