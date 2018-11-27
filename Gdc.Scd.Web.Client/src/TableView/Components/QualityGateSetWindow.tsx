import * as React from "react";
import { Dialog } from "@extjs/ext-react";
import { Position } from "../../Common/States/ExtStates";
import { QualtityGateSetProps, QualtityGateSetView } from "./QualtityGateSetView";
import { Modal } from "../../Common/Components/Modal";

export interface QualityGateSetWindowProps extends QualtityGateSetProps {
    position?: Position
}

export class QualityGateSetWindow extends React.Component<QualityGateSetWindowProps> {
    public render() {
        const { tabs, position = {}, onCancel, onSave } = this.props;
        const hasErrors = tabs && tabs.length > 0;

        console.log('QualityGateSetWindow', hasErrors);

        return (
            hasErrors &&
            <Modal displayed={hasErrors} title="QualityGateSetWindow">
                <div>
                    <h1>QualityGateSetWindow content</h1>
                </div>
            </Modal>
        );
    }
}