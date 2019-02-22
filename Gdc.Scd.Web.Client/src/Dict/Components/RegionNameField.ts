import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class RegionNameField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getRegions();
    }
}