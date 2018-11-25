import * as React from "react";
import { Dialog } from "@extjs/ext-react";
import { Position } from "../../Common/States/ExtStates";
import { QualtityGateSetProps, QualtityGateSetView } from "./QualtityGateSetView";

export interface QualityGateSetWindowProps extends QualtityGateSetProps {
    position?: Position
}

export class QualityGateSetWindow extends React.Component<QualityGateSetWindowProps> {
    public render() {
        const { tabs, position = {} } = this.props;
        const hasErrors = tabs && tabs.length > 0;

        return (
            hasErrors &&
            <Dialog 
                displayed={hasErrors} 
                title="Quality gate errors" 
                maximizable
                resizable={{
                    dynamic: true,
                    edges: 'all'
                }}
                minHeight="50%"
                minWidth="60%"
                layout="fit"
                closeAction="destroy"
                {...position}
            >
                <QualtityGateSetView {...this.props} />
            </Dialog>
        );
    }
}