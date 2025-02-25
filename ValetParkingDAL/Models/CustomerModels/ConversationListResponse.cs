using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class ConversationListResponse
    {
        public List<Conversation> ConversationList { get; set; }

    }

    public class Conversation
    {
        public long Id { get; set; }

        public string Message { get; set; }

        public bool IsFromCustomer { get; set; }

        public DateTime ConversationDate {get;set;}
    }

}