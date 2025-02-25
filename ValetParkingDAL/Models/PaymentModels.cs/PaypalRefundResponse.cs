using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalRefundResponse
    {
        public object CreditCardVerification { get; set; }
        public object Transaction { get; set; }
        public object Plan { get; set; }
        public object Subscription { get; set; }
        public object Errors { get; set; }
        public object Parameters { get; set; }
        public object Message { get; set; }
        public Target1 Target { get; set; }
    }

    public class Target1
    {
        public string Id { get; set; }
        public List<object> AddOns { get; set; }
        public double Amount { get; set; }
        public object AvsErrorResponseCode { get; set; }
        public string AvsPostalCodeResponseCode { get; set; }
        public string AvsStreetAddressResponseCode { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public object Channel { get; set; }
        public DateTime CreatedAt { get; set; }
        public CreditCard CreditCard { get; set; }
        public string CurrencyIsoCode { get; set; }
        public CustomerDetails1 CustomerDetails { get; set; }
        public string CvvResponseCode { get; set; }
        public Descriptor Descriptor { get; set; }
        public List<object> Discounts { get; set; }
        public List<object> Disputes { get; set; }
        public int GatewayRejectionReason { get; set; }
        public string GraphQLId { get; set; }
        public string MerchantAccountId { get; set; }
        public object OrderId { get; set; }
        public object PlanId { get; set; }
        public bool ProcessedWithNetworkToken { get; set; }
        public string ProcessorAuthorizationCode { get; set; }
        public int ProcessorResponseType { get; set; }
        public string ProcessorResponseCode { get; set; }
        public string ProcessorResponseText { get; set; }
        public object ProcessorSettlementResponseCode { get; set; }
        public object ProcessorSettlementResponseText { get; set; }
        public object AdditionalProcessorResponse { get; set; }
        public string NetworkResponseCode { get; set; }
        public string NetworkResponseText { get; set; }
        public object VoiceReferralNumber { get; set; }
        public object PurchaseOrderNumber { get; set; }
        public object Recurring { get; set; }
        public string RefundedTransactionId { get; set; }
        public List<object> RefundIds { get; set; }
        public List<object> PartialSettlementTransactionIds { get; set; }
        public object AuthorizedTransactionId { get; set; }
        public object SettlementBatchId { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public int EscrowStatus { get; set; }
        public int Status { get; set; }
        public List<StatusHistory> StatusHistory { get; set; }
        public List<object> AuthorizationAdjustments { get; set; }
        public object SubscriptionId { get; set; }
        public SubscriptionDetails SubscriptionDetails { get; set; }
        public object TaxAmount { get; set; }
        public bool TaxExempt { get; set; }
        public int Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CustomFields CustomFields { get; set; }
        public object ServiceFeeAmount { get; set; }
        public DisbursementDetails DisbursementDetails { get; set; }
        public object ApplePayDetails { get; set; }
        public object AndroidPayDetails { get; set; }
        public object PayPalDetails { get; set; }
        public object PayPalHereDetails { get; set; }
        public object LocalPaymentDetails { get; set; }
        public object VenmoAccountDetails { get; set; }
        public object UsBankAccountDetails { get; set; }
        public object VisaCheckoutCardDetails { get; set; }
        public object SamsungPayCardDetails { get; set; }
        public int PaymentInstrumentType { get; set; }
        public object RiskData { get; set; }
        public object ThreeDSecureInfo { get; set; }
        public object FacilitatedDetails { get; set; }
        public object FacilitatorDetails { get; set; }
        public object ScaExemptionRequested { get; set; }
        public object DiscountAmount { get; set; }
        public object ShippingAmount { get; set; }
        public object ShipsFromPostalCode { get; set; }
        public object AchReturnCode { get; set; }
        public string NetworkTransactionId { get; set; }
        public DateTime AuthorizationExpiresAt { get; set; }
        public string RetrievalReferenceNumber { get; set; }
        public object AcquirerReferenceNumber { get; set; }
        public object InstallmentCount { get; set; }
        public List<object> Installments { get; set; }
        public List<object> RefundedInstallments { get; set; }
        public object Retried { get; set; }
    }



}