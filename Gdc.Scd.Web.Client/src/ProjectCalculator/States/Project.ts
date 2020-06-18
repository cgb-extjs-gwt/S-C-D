import { NamedId } from "../../Common/States/CommonStates";

export enum DayOfWeek {
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday 
}

export enum PeriodType {
    Minutes,
    Hours,
    Days,
    Weeks,
    Months,
    Years
}

export interface DayHour {
    day?: DayOfWeek
    hour?: number
}

export interface AvailabilityProjCalc {
    start: DayHour
    end: DayHour
    value?: number
}

export interface ReactionTimeProjCalc {
    minutes?: number
    periodType?: PeriodType
    value?: number
}

export interface DurationProjCalc {
    months?: number
    periodType?: PeriodType
    value?: number
}

export interface ProjectItem {
    id: number
    wgId?: number
    countryId?: number
    availability: AvailabilityProjCalc
    reactionTime: ReactionTimeProjCalc
    reactionTypeId?: number
    serviceLocationId?: number
    duration: DurationProjCalc
    isRecalculation: boolean
    onsiteHourlyRates?: number
    fieldServiceCost: {
        timeAndMaterialShare?: number
        travelCost?: number
        labourCost?: number
        performanceRate?: number
        travelTime?: number
        oohUpliftFactor?: number
    }
    reinsurance: {
        flatfee?: number
        upliftFactor?: number
        currencyId?: number
    }
    markupOtherCosts: {
        markup?: number
        markupFactor?: number
        prolongationMarkupFactor?: number
        prolongationMarkup?: number
    }
    logisticsCosts: {
        expressDelivery?: number
        highAvailabilityHandling?: number
        standardDelivery?: number
        standardHandling?: number
        returnDeliveryFactory?: number
        taxiCourierDelivery?: number
    }
    availabilityFee: {
        totalLogisticsInfrastructureCost?: number
        stockValueFj?: number
        stockValueMv?: number
        averageContractDuration?: number
    }
}

export interface Project extends NamedId<number> {
    creationDate?: Date
    user: NamedId<number>
    projectItems: ProjectItem[]
}