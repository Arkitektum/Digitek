using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class UserInfo
    {
        [DefaultValue("")]
        public string Name { get; set; }
        [DefaultValue("")]
        public string Company { get; set; }
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
