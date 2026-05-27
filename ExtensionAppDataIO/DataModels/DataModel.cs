using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.DataModels {
    public enum AccountTypeEnum {
          Prospect,
       [System.Runtime.Serialization.EnumMember(Value = "Customer_Direct")]
          CustomerDirect,
       [System.Runtime.Serialization.EnumMember(Value = "Customer_Channel")]
          CustomerChannel,
       [System.Runtime.Serialization.EnumMember(Value = "Channel_Partner_Reseller")]
          ChannelPartnerReseller,
       [System.Runtime.Serialization.EnumMember(Value = "Installation_Partner")]
          InstallationPartner,
       [System.Runtime.Serialization.EnumMember(Value = "Technology_Partner")]
          TechnologyPartner,
          Other,
    }

    [Term("Account")]
    [Crud("Createable,Readable,Updateable")]
    public class Account : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Account";
    
       [ConcertoIdentifier]
       [Term("Account ID")]
       [Crud("Readable")]
       [MaxLength(18)]
       public string Id { get; set; }

       [Term("Account Name")]
       [Crud("Createable,Readable,Updateable")]
       public string? Name { get; set; }

       [Term("Shipping Latitude")]
       [Crud("Createable,Readable,Updateable")]
       public float? ShippingLatitude { get; set; }

       [Term("Deleted")]
       [Crud("Readable")]
       [JsonPropertyName("_61")]
       public bool? _61 { get; set; }

       [Term("Master Record ID")]
       [Crud("Readable")]
       [MaxLength(18)]
       public string? MasterRecordId { get; set; }

       [Term("Push Count")]
       [Crud("Readable")]
       public float? PushCount { get; set; }

       [Term("Account Type")]
       [Crud("Createable,Readable,Updateable")]
       public AccountTypeEnum? Type { get; set; }

       [Term("ChildAccounts")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       public Account[]? ChildAccounts { get; set; }

       [Term("Time Something Happened")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("time")]
       public System.DateTime? Time { get; set; }

       [Term("Master Record Object")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("msRecord")]
       public MasterRecordId? MsRecord { get; set; }

       [Term("Master Record Relationship")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("msRecord2")]
       public MasterRecordId? MsRecord2 { get; set; }
    }

    public enum MasterRecordIdTypeEnum {
          Prospect,
       [System.Runtime.Serialization.EnumMember(Value = "Customer_Direct")]
          CustomerDirect,
       [System.Runtime.Serialization.EnumMember(Value = "Customer_Channel")]
          CustomerChannel,
       [System.Runtime.Serialization.EnumMember(Value = "Channel_Partner_Reseller")]
          ChannelPartnerReseller,
       [System.Runtime.Serialization.EnumMember(Value = "Installation_Partner")]
          InstallationPartner,
       [System.Runtime.Serialization.EnumMember(Value = "Technology_Partner")]
          TechnologyPartner,
          Other,
    }

    [Term("MasterRecordId")]
    [Crud("Createable,Readable,Updateable")]
    public class MasterRecordId : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.MasterRecordId";

       [ConcertoIdentifier]
       [Term("MasterRecordId ID")]
       [Crud("Readable")]
       [MaxLength(18)]
       public string Id { get; set; }

       [Term("Deleted")]
       [Crud("Readable")]
       public bool Deleted { get; set; }

       [Term("Account ID")]
       [Crud("Readable")]
       [Relationship]
       public Account? AccountId { get; set; }

       [Term("Push Count")]
       [Crud("Readable")]
       public float? PushCount { get; set; }

       [Term("MasterRecordId Type")]
       [Crud("Createable,Readable,Updateable")]
       public MasterRecordIdTypeEnum? Type { get; set; }

       [Term("Shipping Latitude")]
       [Crud("Createable,Readable,Updateable")]
       public float? ShippingLatitude { get; set; }

       [Term("ChildMasterRecordIds")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       public MasterRecordId[]? ChildMasterRecordIds { get; set; }

       [Term("Address Relationship")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("addy")]
       public Address? Addy { get; set; }

       [Term("Address Object")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("addy2")]
       public Address? Addy2 { get; set; }
    }

    [Term("Address")]
    [Crud("Createable,Readable,Updateable")]
    public class Address : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Address";

       [ConcertoIdentifier]
       [Term("Address ID")]
       [Crud("Readable")]
       [MaxLength(18)]
       public string Id { get; set; }

       [Term("Name")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(18)]
       public string? Name { get; set; }

       [Term("Street 1")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(255)]
       [JsonPropertyName("street1")]
       public string? Street1 { get; set; }

       [Term("Street 2")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(255)]
       [JsonPropertyName("street2")]
       public string? Street2 { get; set; }

       [Term("Locality")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(255)]
       [JsonPropertyName("locality")]
       public string? Locality { get; set; }

       [Term("Subdivision")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(100)]
       [JsonPropertyName("subdivision")]
       public string? Subdivision { get; set; }

       [Term("Country or Region")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(100)]
       [JsonPropertyName("countryOrRegion")]
       public string? CountryOrRegion { get; set; }

       [Term("Postal Code")]
       [Crud("Createable,Readable,Updateable")]
       [MaxLength(20)]
       [JsonPropertyName("postalCode")]
       public string? PostalCode { get; set; }

       [Term("Contact Reference")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("primaryContact")]
       public Contact? PrimaryContact { get; set; }
    }

    [Term("Contact")]
    [Crud("Createable,Readable,Updateable,Deletable")]
    public class Contact : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Contact";

       [ConcertoIdentifier]
       [Term("Contact ID")]
       [Crud("Readable")]
       [JsonPropertyName("id")]
       public string Id { get; set; }

       [Term("Full Name")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("fullName")]
       public string FullName { get; set; }

       [Term("Email")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("email")]
       public string? Email { get; set; }

       [Term("Phone")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("phone")]
       public string? Phone { get; set; }

       [Term("Opportunity Link")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("currentOpportunity")]
       public Opportunity? CurrentOpportunity { get; set; }
    }

    [Term("Opportunity")]
    [Crud("Createable,Readable,Updateable,Deletable")]
    public class Opportunity : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Opportunity";

       [ConcertoIdentifier]
       [Term("Opportunity ID")]
       [Crud("Readable")]
       [JsonPropertyName("id")]
       public string Id { get; set; }

       [Term("Opportunity Name")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("name")]
       public string Name { get; set; }

       [Term("Amount")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("amount")]
       public float? Amount { get; set; }

       [Term("Close Date")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("closeDate")]
       public System.DateTime? CloseDate { get; set; }

       [Term("Order Relationship")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("associatedOrder")]
       public Order? AssociatedOrder { get; set; }
    }

    [Term("OrderLine")]
    [Crud("Createable,Readable,Updateable,Deletable")]
    public class OrderLine : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.OrderLine";

       [Term("Product Name")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("productName")]
       public string ProductName { get; set; }

       [Term("Quantity")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("quantity")]
       public int Quantity { get; set; }

       [Term("Unit Price")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("unitPrice")]
       public float UnitPrice { get; set; }

       [Term("Total Price")]
       [Crud("Readable")]
       [JsonPropertyName("totalPrice")]
       public float TotalPrice { get; set; }

       [Term("Related Invoice")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("relatedInvoice")]
       public Invoice? RelatedInvoice { get; set; }
    }

    [Term("Order")]
    [Crud("Createable,Readable,Updateable,Deletable")]
    public class Order : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Order";

       [ConcertoIdentifier]
       [Term("Order ID")]
       [Crud("Readable")]
       [JsonPropertyName("id")]
       public string Id { get; set; }

       [Term("Order Number")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("orderNumber")]
       public string OrderNumber { get; set; }

       [Term("Total Amount")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("totalAmount")]
       public float TotalAmount { get; set; }

       [Term("Order Date")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("orderDate")]
       public System.DateTime OrderDate { get; set; }

       [Term("Order Lines")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("orderLines")]
       public OrderLine[]? OrderLines { get; set; }

       [Term("Invoice Connection")]
       [Crud("Createable,Readable,Updateable")]
       [Relationship]
       [JsonPropertyName("orderInvoice")]
       public Invoice? OrderInvoice { get; set; }
    }

    [Term("Invoice")]
    [Crud("Createable,Readable,Updateable,Deletable")]
    public class Invoice : Concept {
       [JsonPropertyName("$class")]
       public override string _Class { get; } = "org.example@1.0.0.Invoice";

       [ConcertoIdentifier]
       [Term("Invoice ID")]
       [Crud("Readable")]
       [JsonPropertyName("id")]
       public string Id { get; set; }

       [Term("Invoice Number")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("invoiceNumber")]
       public string InvoiceNumber { get; set; }

       [Term("Amount")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("amount")]
       public float Amount { get; set; }

       [Term("Due Date")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("dueDate")]
       public System.DateTime DueDate { get; set; }

       [Term("Is Paid")]
       [Crud("Createable,Readable,Updateable")]
       [JsonPropertyName("isPaid")]
       public bool? IsPaid { get; set; }
    }
}

