import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryGroupIsoCodeField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountryGroupIsoCode();
    }
}