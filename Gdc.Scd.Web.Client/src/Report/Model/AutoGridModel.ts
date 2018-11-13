import { AutoFilterModel } from "./AutoFilterModel";
import { AutoColumnModel } from "./AutoColumnModel";

export interface AutoGridModel {

    id: string;

    name: string;

    title?: string;

    fields: AutoColumnModel[];

    filter: AutoFilterModel[];

}