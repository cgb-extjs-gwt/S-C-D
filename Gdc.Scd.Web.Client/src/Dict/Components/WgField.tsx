import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class WgField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getWG();
    }
}