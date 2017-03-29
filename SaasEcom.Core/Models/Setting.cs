using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    public class Setting
    {
        [Key]
        [MaxLengthAttribute(255)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
