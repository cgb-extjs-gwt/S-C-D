import { DictField } from "./DictField";

export class ProActiveField extends DictField {

    protected nameField: string = 'externalName';

    public getItems() {
        return this.srv.getProActive();
    }
}