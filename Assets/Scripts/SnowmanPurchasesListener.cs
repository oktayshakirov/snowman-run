using UnityEngine;

public class SnowmanPurchasesListener : Purchases.UpdatedCustomerInfoListener
{
    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        CoinPurchaseGrant.ProcessCustomerInfo(customerInfo);
    }
}
