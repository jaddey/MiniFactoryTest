using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Тонкая обёртка над Unity IAP. Gameplay-код не зависит от API Unity IAP —
/// он подписывается на доменные события (ProductId + количество монет).
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    [SerializeField] private string _productId = "coins_pack_small";
    [SerializeField] private int _coinsPerPack = 500;

    private IStoreController _storeController;

    public bool IsInitialized => _storeController != null;

    /// <summary>productId, количество монет</summary>
    public event Action<string, int> PurchaseSucceeded;

    /// <summary>productId, причина</summary>
    public event Action<string, string> PurchaseFailed;

    public event Action Initialized;
    public event Action<string> InitializationFailed;

    private void Start()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        if (IsInitialized) return;

        var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
        // Fake Store: не требует реального продукта в Google Play.
        module.useFakeStoreAlways = true;
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
        var builder = ConfigurationBuilder.Instance(module);
        builder.AddProduct(_productId, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>Публичный метод для кнопки магазина.</summary>
    public void BuyCoinsPack()
    {
        if (!IsInitialized)
        {
            // ТЗ: обработка ситуации «IAP недоступен или не инициализировался»
            Debug.LogWarning("[IAPManager] Покупка невозможна: IAP не инициализирован");
            PurchaseFailed?.Invoke(_productId, "iap_not_initialized");
            return;
        }

        _storeController.InitiatePurchase(_productId);
    }

    #region IDetailedStoreListener

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        Debug.Log("[IAPManager] IAP инициализирован");
        Initialized?.Invoke();

        // Проверка, что продукт получен (ТЗ: «получение продукта»)
        var product = controller.products.WithID(_productId);
        if (product == null || !product.availableToPurchase)
            Debug.LogWarning($"[IAPManager] Продукт {_productId} недоступен для покупки");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAPManager] Ошибка инициализации IAP: {error}");
        InitializationFailed?.Invoke(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        OnInitializeFailed(error); // делегируем в базовую перегрузку
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id != _productId)
            return PurchaseProcessingResult.Complete;

        Debug.Log($"[IAPManager] Покупка успешна: {_productId}");
        PurchaseSucceeded?.Invoke(_productId, _coinsPerPack);
        return PurchaseProcessingResult.Complete; // consumable — подтверждаем сразу
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[IAPManager] Покупка не удалась: {product?.definition?.id}, причина: {failureReason}");
        PurchaseFailed?.Invoke(product?.definition?.id ?? _productId, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"[IAPManager] Покупка не удалась: {product?.definition?.id}, причина: {failureDescription.reason} {failureDescription.message}");
        PurchaseFailed?.Invoke(product?.definition?.id ?? _productId, failureDescription.reason.ToString());
    }

    #endregion
}