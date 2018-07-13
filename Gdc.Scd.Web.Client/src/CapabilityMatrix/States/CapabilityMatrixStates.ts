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

export class CapabilityMatrixItem {
    country: NamedId

    WG: NamedId

    availability: NamedId

    duration: NamedId

    reactType: NamedId

    reactionTime: NamedId

    serviceLocation: NamedId

    public hash(): string {
        return this.idOrMinus(this.country) + '|' +
            this.idOrMinus(this.WG) + '|' +
            this.idOrMinus(this.availability) + '|' +
            this.idOrMinus(this.duration) + '|' +
            this.idOrMinus(this.reactType) + '|' +
            this.idOrMinus(this.reactionTime) + '|' +
            this.idOrMinus(this.serviceLocation);
    }

    private idOrMinus(v: NamedId): string {
        return v && v.id ? v.id : '-1';
    }
}