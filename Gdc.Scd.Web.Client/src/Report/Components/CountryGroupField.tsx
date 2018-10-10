import { DictField } from "./DictField";

export class CountryGroupField extends DictField {
    public getItems() {
        return this.srv.getCountryGroups();
    }
}