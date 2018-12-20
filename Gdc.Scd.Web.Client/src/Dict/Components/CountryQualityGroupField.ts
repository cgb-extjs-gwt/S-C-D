import { DictField } from "./DictField";

export class CountryQualityGroupField extends DictField {
    public getItems() {
        return this.srv.getCountryQualityGroup();
    }
}