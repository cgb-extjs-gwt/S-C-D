import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryQualityGroupField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountryQualityGroup();
    }
}