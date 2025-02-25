using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using ValetParkingBLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Square.Models;
using Square.Exceptions;


namespace ValetParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IEmail _mailService;
        private readonly IConfiguration _configuration;
        private readonly IWebhook _webhookRepo;

        public WebhookController(IEmail mailService, IConfiguration configuration, IWebhook webhookRepo)
        {
            _mailService = mailService;
            _configuration = configuration;
            _webhookRepo = webhookRepo;
        }


        [HttpPost]
        [Route("invoice")]
        public async Task<IActionResult> SquareWebhook()
        {
            try
            {
                // Read the incoming request body
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                var webhookEvent = JObject.Parse(json);
                // Extract the event type
                var eventType = webhookEvent["type"]?.ToString();

                // Handle the "invoice.created" event
                if (eventType == "invoice.created")
                {
                    // Extract the "invoice" object dynamically
                    var invoice = webhookEvent["data"]?["object"]?["invoice"];

                    if (invoice != null)
                    {
                        // Extract specific properties from the invoice object
                        var invoiceId = invoice["id"]?.ToString();
                        var customerId = invoice["primary_recipient"]?["customer_id"]?.ToString();
                        var amount = invoice["payment_requests"]?[0]?["computed_amount_money"]?["amount"].ToObject<decimal>() ?? 0;

                        _webhookRepo.UpdateCustomerAmount(customerId, amount);

                        Console.WriteLine("Invoice Created");
                    }
                    else
                    {
                        Console.WriteLine("Invoice object is null.");
                    }
                }
                else
                {
                    Console.WriteLine("Invoice Payment was failed!");
                }


                // Return Ok for unhandled events to avoid retries
                return Ok();
            }
            catch (ApiException ex)
            {
                //_mailService.Send("narsing.m@synsoftglobal.com", "EMS Webhook failed", ex.Message);
                _mailService.Send("mahimakhore.synsoft@gmail.com", "EMS Webhook failed", ex.Message);

                return Ok();
            }
        }
    }
}
