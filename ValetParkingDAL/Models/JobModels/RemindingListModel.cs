using System;
using System.Collections.Generic;
using System.Text;

namespace ValetParkingDAL.Models.JobModels
{
    public class RemindingListModel
    {

        public List<StaffInfoModel> StaffTokensList { get; set; }

        public List<ReminderDataModel> ReminderList { get; set; }

        public List<LocationBadgeCountModel> LocationBadgeCountList { get; set; }

    }
}
