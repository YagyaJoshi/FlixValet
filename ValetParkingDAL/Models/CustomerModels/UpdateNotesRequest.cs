using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class UpdateNotesRequest
    {

        [Required]
        public long EnterExitId { get; set; }
        public string Notes { get; set; }

    }
}