import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryGroupField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountryGroups();
    }
}