import { DictField } from "./DictField";

export class CountryGroupLutField extends DictField {
    public getItems() {
        return this.srv.getCountryGroupLuts();
    }
}