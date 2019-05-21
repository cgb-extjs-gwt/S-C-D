import * as React from "react";
import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export function fillSogInfo(sog) {
    return <div><strong>{sog.name}</strong>({sog.description})</div>;
};

export class SogField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getSog();
    }
}