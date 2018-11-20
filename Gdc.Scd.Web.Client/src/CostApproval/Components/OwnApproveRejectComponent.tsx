import * as React from "react";
import { Toolbar, Button, Container } from "@extjs/ext-react";
import { QualityGateWindowContainer } from "./QualityGateWindowContainer";

export interface OwnApproveRejectActions {
    onApprove?()
    onReject?()
}

export interface OwnApproveRejectProps extends OwnApproveRejectActions {
    bundleId: number
    costBlockId: string
    costElementId: string
}

export class OwnApproveRejectComponent extends React.Component<OwnApproveRejectProps> {
    public render() {
        const { onApprove, onReject, bundleId, costBlockId, costElementId } = this.props;

        return (
            <Container flex={1} layout="vbox" minHeight="50">
                <Toolbar flex={1}>
                    <Button text="Approve" handler={this.onApprove} flex={1}/>
                    <Button text="Reject" handler={this.onReject} flex={1}/>
                </Toolbar> 

                <QualityGateWindowContainer bundleId={bundleId} costBlockId={costBlockId} costElementId={costElementId}/>
            </Container>
        );
    }

    private onApprove = () => {
        const { onApprove } = this.props;
        
        onApprove && onApprove();
    }

    private onReject = () => {
        const { onReject } = this.props;

        onReject && onReject();
    }
}