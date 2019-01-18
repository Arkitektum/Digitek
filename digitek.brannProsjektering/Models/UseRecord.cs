using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class UseRecord
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Model { get; set; }
        public string InputJson { get; set; }
        public int ResponseCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
