import { NamedId } from "../../Common/States/CommonStates";

export interface BundleDetailGroup {
    historyValueId: number
    wg: NamedId
    newValue: number
    oldValue?: number
    countryGroupAvgValue?: number
    isRegionError: boolean
    isPeriodError: boolean
    coordinates: { [key: string]: NamedId<number>[] }
}

export interface QualityGateResult {
    errors: BundleDetailGroup[]
    hasErrors: boolean
}