using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models
{

    public class ValidateDateTime : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime datetime;
            bool check = DateTime.TryParse(value.ToString(), out datetime);

            if (check)
                return true;

            return false;
        }
    }
}