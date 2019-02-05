import { CurrencyType } from "./CurrencyType";

export class HwCostFilterModel {
    country?: string[];
    wg?: string[];
    availability?: string[];
    duration?: string[];
    reactionType?: string[];
    reactionTime?: string[];
    serviceLocation?: string[];
    proActive?: string[];
    currency?: CurrencyType;
}