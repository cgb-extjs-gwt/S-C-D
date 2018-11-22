import { DictField } from "./DictField";

export class CountryGroupDigitField extends DictField {
    public getItems() {
        return this.srv.getCountryGroupDigits();
    }
}