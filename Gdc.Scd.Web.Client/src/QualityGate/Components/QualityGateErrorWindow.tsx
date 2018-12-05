import * as React from "react";
import { Dialog } from "@extjs/ext-react";
import { QualityGateErrorView, QualityGateErrorProps } from "./QualityGateErrorView";
import { QualityGateErrorContainer, QualityGateErrorContainerProps } from "./QualityGateErrorContainer";
import { Position } from "../../Common/States/ExtStates";

export interface QualityGateErrorWindowProps extends QualityGateErrorContainerProps {
    position?: Position
}

export class QualityGateErrorWindow extends React.Component<QualityGateErrorWindowProps> {
    public render() {
        const { errors, position = {} } = this.props;
        const hasErrors = errors && errors.length > 0;

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
                <QualityGateErrorContainer {...(this.props as any)}  />
            </Dialog>
        );
    }
}