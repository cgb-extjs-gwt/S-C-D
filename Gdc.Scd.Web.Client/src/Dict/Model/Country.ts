import { NamedId } from "../../Common/States/CommonStates";

export class Country implements NamedId {
    public id: string;

    public name: string;

    public isMaster: boolean;

    public canOverrideTransferCostAndPrice: boolean;

    public canStoreListAndDealerPrices: boolean;
}

