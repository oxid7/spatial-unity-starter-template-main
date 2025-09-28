using System;
using System.Collections.Generic;
using UnityEngine;

public class NFTMarketLandViewer : MonoBehaviour
{
    [Header("Query")]
    [Min(0)] public int landId = 12;
    [Tooltip("If true, call the API in OnEnable() automatically.")]
    public bool autoFetchOnEnable = false;

    [Tooltip("If true, only keep orders where order.landId == landId.")]
    public bool filterOrdersByLandId = true;

    [Header("Status (read-only)")]
    [SerializeField] private bool isLoading;
    [SerializeField] private long lastStatusCode;
    [SerializeField] private string lastError;

    [Header("Market Summary (read-only)")]
    public int landMarketCount;
    public string landMarketVolume;
    public int otherLandMarketCount;
    public string otherLandMarketVolume;

    [Header("Orders (read-only)")]
    public List<OrderView> orders = new List<OrderView>();

    [Serializable]
    public class OrderView
    {
        public int id;
        public int landId;
        public string land;
        public int nftId;
        public string nftType;
        public int model;
        public string modelCaption;
        public int series;
        public string seriesCaption;
        public string expireDate;       // keep as string for Inspector; parse when you need DateTime
        public string currency;         // "bnb" or "sixp"
        public double price;
        public double landTaxPercent;
        public double uplineCoachPercent;
        public double masterCoachPercent;
        public int mainSpecification;
        public string mainSpecificationTitle;
        public string mainSpecificationShortTitle;
        public string mainSpecificationUnit;
        public string imageUrl;
        public string animationUrl;
        public int openSeaId;
    }

    void OnEnable()
    {
        if (autoFetchOnEnable) FetchNow();
    }

    [ContextMenu("Fetch Now")]
    public void FetchNow()
    {
        if (SixpackApiClient.Instance == null)
        {
            lastError = "SixpackApiClient.Instance not found in scene.";
            Debug.LogError(lastError);
            return;
        }

        // Clear previous
        isLoading = true;
        lastError = null;
        lastStatusCode = 0;
        orders.Clear();
        landMarketCount = 0;
        landMarketVolume = null;
        otherLandMarketCount = 0;
        otherLandMarketVolume = null;

        SixpackApiClient.Instance.FetchNFTMarketList(landId, res =>
        {
            isLoading = false;
            lastStatusCode = res.StatusCode;

            if (!res.IsSuccess)
            {
                lastError = $"HTTP {res.StatusCode}: {res.ErrorMessage}";
                Debug.LogError("[NFTMarket] " + lastError);
                return;
            }

            var msg = res.Data?.message;
            if (msg == null)
            {
                lastError = "Empty message in response.";
                Debug.LogWarning("[NFTMarket] " + lastError);
                return;
            }

            // Summary
            landMarketCount = msg.landMarket != null ? msg.landMarket.count : 0;
            landMarketVolume = msg.landMarket != null ? msg.landMarket.volume : null;
            otherLandMarketCount = msg.otherLandMarket != null ? msg.otherLandMarket.count : 0;
            otherLandMarketVolume = msg.otherLandMarket != null ? msg.otherLandMarket.volume : null;

            // Orders
            orders.Clear();
            if (msg.orders != null)
            {
                foreach (var o in msg.orders)
                {
                    if (filterOrdersByLandId && o.landId != landId) continue;

                    orders.Add(new OrderView
                    {
                        id = o.id,
                        landId = o.landId,
                        land = o.land,
                        nftId = o.nftId,
                        nftType = o.nftType,
                        model = o.model,
                        modelCaption = o.modelCaption,
                        series = o.series,
                        seriesCaption = o.seriesCaption,
                        expireDate = o.expireDate,
                        currency = o.currency,
                        price = o.price,
                        landTaxPercent = o.landTaxPercent,
                        uplineCoachPercent = o.uplineCoachPercent,
                        masterCoachPercent = o.masterCoachPercent,
                        mainSpecification = o.mainSpecification,
                        mainSpecificationTitle = o.mainSpecificationTitle,
                        mainSpecificationShortTitle = o.mainSpecificationShortTitle,
                        mainSpecificationUnit = o.mainSpecificationUnit,
                        imageUrl = o.imageUrl,
                        animationUrl = o.animationUrl,
                        openSeaId = o.openSeaId
                    });
                }
            }

            Debug.Log($"[NFTMarket] Loaded {orders.Count} order(s) for landId={landId}. HTTP {lastStatusCode}");
        });
    }
}
