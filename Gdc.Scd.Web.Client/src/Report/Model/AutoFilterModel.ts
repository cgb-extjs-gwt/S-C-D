import { AutoFilterType } from "./AutoFilterType";

export interface AutoFilterModel {

    text: string;

    name: string;

    type?: AutoFilterType;

    multiSelect?: boolean;

    value?: any;

}