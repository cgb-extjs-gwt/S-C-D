import { Action } from "redux";
import { NamedId } from "../../Common/States/CommonStates";

export interface CapabilityMatrixDto {
    countries: NamedId[]
    warrantyGroups: NamedId[]
    availabilityTypes: NamedId[]
    durationTypes: NamedId[]
    reactTypes: NamedId[]
    reactionTimeTypes: NamedId[]
    serviceLocationTypes: NamedId[]
}

