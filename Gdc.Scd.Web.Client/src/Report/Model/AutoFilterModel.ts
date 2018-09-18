import { AutoColumnType } from "./AutoColumnType";

export interface AutoFilterModel {

    text: string;

    name: string;

    type?: AutoColumnType;

    value?: any;

}