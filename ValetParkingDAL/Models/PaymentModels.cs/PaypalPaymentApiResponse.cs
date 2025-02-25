using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PaypalPaymentApiResponse
    {
        public object CreditCardVerification { get; set; }
        public object Transaction { get; set; }
        public object Plan { get; set; }
        public object Subscription { get; set; }
        public object Errors { get; set; }
        public object Parameters { get; set; }
        public object Message { get; set; }
        public Target Target { get; set; }
    }
    public class BillingAddress1
    {
        public object Id { get; set; }
        public object CustomerId { get; set; }
        public object FirstName { get; set; }
        public object LastName { get; set; }
        public object Company { get; set; }
        public object StreetAddress { get; set; }
        public object ExtendedAddress { get; set; }
        public object Locality { get; set; }
        public object Region { get; set; }
        public object PostalCode { get; set; }
        public object CountryCodeAlpha2 { get; set; }
        public object CountryCodeAlpha3 { get; set; }
        public object CountryCodeNumeric { get; set; }
        public object CountryName { get; set; }
        public object PhoneNumber { get; set; }
        public object CreatedAt { get; set; }
        public object UpdatedAt { get; set; }
    }

    public class CreditCard
    {
        public string Bin { get; set; }
        public object CardholderName { get; set; }
        public int CardType { get; set; }
        public object CreatedAt { get; set; }
        public object CustomerId { get; set; }
        public object IsDefault { get; set; }
        public bool IsVenmoSdk { get; set; }
        public object IsExpired { get; set; }
        public object IsNetworkTokenized { get; set; }
        public int CustomerLocation { get; set; }
        public string LastFour { get; set; }
        public object UniqueNumberIdentifier { get; set; }
        public List<object> Subscriptions { get; set; }
        public object Token { get; set; }
        public object UpdatedAt { get; set; }
        public BillingAddress1 BillingAddress { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public int Prepaid { get; set; }
        public int Payroll { get; set; }
        public int Debit { get; set; }
        public int Commercial { get; set; }
        public int Healthcare { get; set; }
        public int DurbinRegulated { get; set; }
        public string ImageUrl { get; set; }
        public object Verification { get; set; }
        public string AccountType { get; set; }
        public string CountryOfIssuance { get; set; }
        public string IssuingBank { get; set; }
        public string ProductId { get; set; }
        public string ExpirationDate { get; set; }
        public string MaskedNumber { get; set; }
    }

    public class CustomerDetails1
    {
        public object Id { get; set; }
        public object FirstName { get; set; }
        public object LastName { get; set; }
        public object Company { get; set; }
        public object Email { get; set; }
        public object Phone { get; set; }
        public object Fax { get; set; }
        public object Website { get; set; }
    }

    public class Descriptor
    {
        public object Name { get; set; }
        public object Phone { get; set; }
        public object Url { get; set; }
    }

    public class ShippingAddress
    {
        public object Id { get; set; }
        public object CustomerId { get; set; }
        public object FirstName { get; set; }
        public object LastName { get; set; }
        public object Company { get; set; }
        public object StreetAddress { get; set; }
        public object ExtendedAddress { get; set; }
        public object Locality { get; set; }
        public object Region { get; set; }
        public object PostalCode { get; set; }
        public object CountryCodeAlpha2 { get; set; }
        public object CountryCodeAlpha3 { get; set; }
        public object CountryCodeNumeric { get; set; }
        public object CountryName { get; set; }
        public object PhoneNumber { get; set; }
        public object CreatedAt { get; set; }
        public object UpdatedAt { get; set; }
    }

    public class StatusHistory
    {
        public double Amount { get; set; }
        public int Status { get; set; }
        public DateTime Timestamp { get; set; }
        public int Source { get; set; }
        public string User { get; set; }
    }

    public class SubscriptionDetails
    {
        public object BillingPeriodEndDate { get; set; }
        public object BillingPeriodStartDate { get; set; }
    }

    public class CustomFields
    {
    }

    public class DisbursementDetails
    {
        public object SettlementAmount { get; set; }
        public object SettlementCurrencyIsoCode { get; set; }
        public object SettlementCurrencyExchangeRate { get; set; }
        public object FundsHeld { get; set; }
        public object Success { get; set; }
        public object DisbursementDate { get; set; }
    }

    public class Target
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
        public bool Recurring { get; set; }
        public object RefundedTransactionId { get; set; }
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