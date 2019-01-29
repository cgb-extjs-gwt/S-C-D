import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTypeField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getReactionTypes();
    }
}