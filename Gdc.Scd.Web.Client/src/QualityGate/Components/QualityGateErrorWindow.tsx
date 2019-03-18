import * as React from "react";
import { Dialog, Container } from "@extjs/ext-react";
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
            hasErrors 
                ? <Container {...position}>
                    <Dialog 
                        displayed={hasErrors} 
                        title="Quality gate errors" 
                        maximizable
                        resizable={{
                            dynamic: true,
                            edges: 'all'
                        }}
                        height="50%"
                        width="60%"
                        layout="fit"
                        closeAction="destroy"
                    >
                        <QualityGateErrorContainer {...(this.props as any)}  />
                    </Dialog>
                </Container>
                : null
        );
    }
}