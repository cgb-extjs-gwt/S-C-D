import { DictField } from "./DictField";

export class CountryGroupIsoCodeField extends DictField {
    public getItems() {
        return this.srv.getCountryGroupIsoCode();
    }
}