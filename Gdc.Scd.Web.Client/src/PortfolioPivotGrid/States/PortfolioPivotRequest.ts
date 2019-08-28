import { PortfolioFilterModel } from "../../Portfolio/Model/PortfolioFilterModel";

export interface RequestAxisItem {
    id: string
    dataIndex: string
    direction: string
    header: string
    aggregator: string
}

export interface PortfolioPivotRequest {
    keysSeparator: string
    grandTotalKey: string
    leftAxis: RequestAxisItem[]
    topAxis: RequestAxisItem[] 
    aggregate: RequestAxisItem[]
    filter: PortfolioFilterModel
}