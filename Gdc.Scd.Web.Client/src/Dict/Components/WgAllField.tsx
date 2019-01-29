import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class WgAllField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getWgWithMultivendor();
    }
}