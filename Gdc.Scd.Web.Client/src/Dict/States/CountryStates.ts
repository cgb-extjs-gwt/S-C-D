import { NamedId } from "../../Common/States/CommonStates";

export interface CountryManagementState{
    country: NamedId,
    canOverrideListAndDealerPrices: boolean,
    showDealerPrice: boolean,
    canOverrideTransferCostAndPrice: boolean
}