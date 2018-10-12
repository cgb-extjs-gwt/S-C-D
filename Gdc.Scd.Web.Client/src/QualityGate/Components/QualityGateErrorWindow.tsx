import * as React from "react";
import { Dialog } from "@extjs/ext-react";
import { QualityGateErrorView, QualityGateErrorProps } from "./QualityGateErrorView";
import { QualityGateErrorContainer, QualityGateErrorContainerProps } from "./QualityGateErrorContainer";

export class QualityGateErrorWindow extends React.Component<QualityGateErrorContainerProps> {
    public render() {
        const { errors } = this.props;
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
                minHeight="600"
                minWidth="700"
                layout="fit"
                closeAction="destroy"
            >
                <QualityGateErrorContainer {...(this.props as any)}  />
            </Dialog>
        );
    }
}