import { DictField } from "./DictField";

export class ReactionTimeField extends DictField {
    public getItems() {
        return this.srv.getReactionTimeTypes();
    }
}