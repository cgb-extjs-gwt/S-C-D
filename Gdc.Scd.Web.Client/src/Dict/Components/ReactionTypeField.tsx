import { DictField } from "./DictField";

export class ReactionTypeField extends DictField {
    public getItems() {
        return this.srv.getReactionTypes();
    }
}