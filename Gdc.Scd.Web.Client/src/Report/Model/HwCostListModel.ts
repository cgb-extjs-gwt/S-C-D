export class HwCostListModel {
    public Id: string;

    public Country: string;

    public CanOverrideTransferCostAndPrice?: boolean;

    public CanStoreListAndDealerPrices?: boolean;

    public Wg: string;

    public Availability: string;

    public Duration: string;

    public ReactionType: string;

    public ReactionTime: string;

    public ServiceLocation: string;

    public FieldServiceCost?: number;

    public ServiceSupportCost?: number;

    public Logistic?: number;

    public AvailabilityFee?: number;

    public HddRetention?: number;

    public Reinsurance?: number;

    public TaxAndDutiesW?: number;

    public TaxAndDutiesOow?: number;

    public MaterialW?: number;

    public MaterialOow?: number;

    public ProActive?: number;

    public ServiceTC?: number;

    public ServiceTCManual?: number;

    public ServiceTP?: number;

    public ServiceTPManual?: number;

    public ListPrice?: number;

    public DealerDiscount?: number;

    public DealerPrice?: number;

    public OtherDirect?: number;

    public LocalServiceStandardWarranty?: number;

    public Credits?: number;
}