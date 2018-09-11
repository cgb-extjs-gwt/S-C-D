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
}

export class OwnApproveRejectComponent extends React.Component<OwnApproveRejectProps> {
    public render() {
        const { onApprove, onReject, bundleId, costBlockId } = this.props;

        return (
            <Container flex={1} layout="vbox">
                <Toolbar flex={1}>
                    <Button text="Approve" handler={this.onApprove} flex={1}/>
                    <Button text="Reject" handler={this.onReject} flex={1}/>
                </Toolbar> 

                <QualityGateWindowContainer bundleId={bundleId} costBlockId={costBlockId}/>
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