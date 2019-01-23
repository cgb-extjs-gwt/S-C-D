import { DictField } from "./DictField";

export class SwDigitField extends DictField {
    public getItems() {
        return this.srv.getSwDigit();
    }
}