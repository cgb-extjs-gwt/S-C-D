import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTimeField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getReactionTimeTypes();
    }
}