import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryGroupDigitField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountryGroupDigits();
    }
}