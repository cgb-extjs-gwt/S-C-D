import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryGroupLutField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountryGroupLuts();
    }
}