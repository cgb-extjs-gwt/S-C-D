import { NamedId } from "../../Common/States/CommonStates";
import { Project } from "./Project";

export interface ProjectItemEditData {
    wgs: NamedId<number>[],
    countries: NamedId<number>[],
    reactionTimePeriods: NamedId<number>[],
    reactionTypes: NamedId<number>[],
    serviceLocations: NamedId<number>[],
    durationPeriods: NamedId<number>[],
    reinsuranceCurrencies: NamedId<number>[],
}

export interface ProjectCalculatorState {
    projectItemEditData: ProjectItemEditData
    selectedProject: Project
}